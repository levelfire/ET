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
    private NativeList<ushort> m_PosList;

    protected override void OnCreate()
    {
        m_BeginSimEcb = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<SendClientBlockChangeRpc>(), ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>()));
        RequireSingletonForUpdate<GameSettingsComponent>();

        m_PosList = new NativeList<ushort>(16, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        m_PosList.Dispose();
    }

    protected override void OnUpdate()
    {

        //We must declare our local variables before using them within a job (.ForEach)
        var commandBuffer = m_BeginSimEcb.CreateCommandBuffer();
        var rpcFromEntity = GetBufferFromEntity<OutgoingRpcDataStreamBufferComponent>();
        var gameSettingsEntity = GetSingletonEntity<GameSettingsComponent>();
        var getGameSettingsComponentData = GetComponentDataFromEntity<GameSettingsComponent>();

        m_PosList.Clear();
        var posList = m_PosList;
        var posHandle = Entities
            .ForEach((Entity entity, in SendClientBlockChangeRpc request, in ReceiveRpcCommandRequestComponent requestSource) =>
            {
                if (posList.Length >= posList.Capacity)
                    return;

                commandBuffer.DestroyEntity(entity);

                if (!rpcFromEntity.HasComponent(requestSource.SourceConnection))
                    return;

                Debug.Log($"Rpc Client Recv block Change {request.Pos}");
                posList.Add(request.Pos);
            }).Schedule(Dependency);

        posHandle.Complete();

        if (posList.Length > 0)
        {
            Entities
                .WithReadOnly(posList)
                .WithAll<BlockRTag>()
                .ForEach((Entity entity, int entityInQueryIndex, in Translation trans) =>
                {
                    for (int i = 0; i < posList.Length; ++i)
                    {
                        if (MethHelper.SetPos((int)trans.Value.x, (int)trans.Value.z) == posList[i])
                        {
                            Debug.Log($"Rpc find block Change {posList[i]}");
                            commandBuffer.SetComponent(entity, new HpComponent { Value = 1 });
                            continue;
                        }
                    }
                }).Schedule();
        }

        m_BeginSimEcb.AddJobHandleForProducer(Dependency);
    }
}
