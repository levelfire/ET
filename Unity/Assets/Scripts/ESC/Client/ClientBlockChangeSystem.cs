using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
[UpdateBefore(typeof(RpcSystem))]
public partial class ClientBlockChangeSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;

    protected override void OnCreate()
    {
        m_BeginSimEcb = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<SendClientBlockChangeRpc>(), ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>()));
        RequireSingletonForUpdate<GameSettingsComponent>();
    }

    protected override void OnUpdate()
    {

        //We must declare our local variables before using them within a job (.ForEach)
        var commandBuffer = m_BeginSimEcb.CreateCommandBuffer();
        var rpcFromEntity = GetBufferFromEntity<OutgoingRpcDataStreamBufferComponent>();
        var gameSettingsEntity = GetSingletonEntity<GameSettingsComponent>();
        var getGameSettingsComponentData = GetComponentDataFromEntity<GameSettingsComponent>();

        NativeList<ushort> posList = new NativeList<ushort>();

        //var posHandle = Entities
        //.ForEach((Entity entity, in SendClientBlockChangeRpc request, in ReceiveRpcCommandRequestComponent requestSource) =>
        //{
        //    commandBuffer.DestroyEntity(entity);

        //    if (!rpcFromEntity.HasComponent(requestSource.SourceConnection))
        //        return;
        //    Debug.Log($"Rpc Client Recv blockChange{request.Pos}");
        //    posList.Add(request.Pos);
        //}).Schedule(Dependency);

        var posHandle = Entities
            .ForEach((Entity entity, in SendClientBlockChangeRpc request, in ReceiveRpcCommandRequestComponent requestSource) =>
            {
                commandBuffer.DestroyEntity(entity);

                if (!rpcFromEntity.HasComponent(requestSource.SourceConnection))
                    return;
                Debug.Log($"Rpc Client Recv blockChange{request.Pos}");
                posList.Add(request.Pos);
            }).Schedule(Dependency);

        Dependency = Entities
            .WithReadOnly(posList)
            .WithAll<BlockRTag>()
            .ForEach((Entity entity, int entityInQueryIndex, in Translation trans) =>
            {
                for (int i = 0; i < posList.Length; ++i)
                {
                    if (MethHelper.SetPos((int)trans.Value.x, (int)trans.Value.z) == posList[i])
                    {
                        Debug.Log($"Rpc find blockChange{posList[i]}");
                        return;
                    }
                }

            }).Schedule(posHandle);

        m_BeginSimEcb.AddJobHandleForProducer(Dependency);
    }
}
