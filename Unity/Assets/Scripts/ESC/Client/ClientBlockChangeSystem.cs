using Unity.Entities;
using Unity.NetCode;
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

        Entities
        .ForEach((Entity entity, in SendClientBlockChangeRpc request, in ReceiveRpcCommandRequestComponent requestSource) =>
        {
            commandBuffer.DestroyEntity(entity);

            if (!rpcFromEntity.HasComponent(requestSource.SourceConnection))
                return;
            Debug.Log($"Rpc Client Recv blockChange{request.Pos}");
        }).Schedule();

        m_BeginSimEcb.AddJobHandleForProducer(Dependency);
    }
}
