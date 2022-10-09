using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
[UpdateBefore(typeof(RpcSystem))]
public partial class ClientLoadGameSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;

    protected override void OnCreate()
    {
        m_BeginSimEcb = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<SendClientGameRpc>(), ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>()));
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
        .ForEach((Entity entity, in SendClientGameRpc request, in ReceiveRpcCommandRequestComponent requestSource) =>
        {
            commandBuffer.DestroyEntity(entity);

            if (!rpcFromEntity.HasComponent(requestSource.SourceConnection))
                return;

            getGameSettingsComponentData[gameSettingsEntity] = new GameSettingsComponent
            {
                levelWidth = request.levelWidth,
                levelHeight = request.levelHeight,
                levelDepth = request.levelDepth,
                playerForce = request.playerForce,
                bulletVelocity = request.bulletVelocity
            };

            commandBuffer.AddComponent(requestSource.SourceConnection, default(NetworkStreamInGame));

            var levelReq = commandBuffer.CreateEntity();
            commandBuffer.AddComponent(levelReq, new SendServerGameLoadedRpc());
            commandBuffer.AddComponent(levelReq, new SendRpcCommandRequestComponent { TargetConnection = requestSource.SourceConnection });

            Debug.Log("Client loaded game");
        }).Schedule();

        m_BeginSimEcb.AddJobHandleForProducer(Dependency);
    }
}
