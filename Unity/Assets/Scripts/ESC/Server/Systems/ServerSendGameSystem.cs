using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

public struct SentClientGameRpcTag : IComponentData
{
}

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateBefore(typeof(RpcSystem))]
public partial class ServerSendGameSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem m_Barrier;

    protected override void OnCreate()
    {
        m_Barrier = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        RequireSingletonForUpdate<GameSettingsComponent>();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = m_Barrier.CreateCommandBuffer();

        var serverData = GetSingleton<GameSettingsComponent>();

        Entities
        .WithNone<SentClientGameRpcTag>()
        .ForEach((Entity entity, in NetworkIdComponent netId) =>
        {
            commandBuffer.AddComponent(entity, new SentClientGameRpcTag());
            var req = commandBuffer.CreateEntity();
            commandBuffer.AddComponent(req, new SendClientGameRpc
            {
                levelWidth = serverData.levelWidth,
                levelHeight = serverData.levelHeight,
                levelDepth = serverData.levelDepth,
                playerForce = serverData.playerForce,
                bulletVelocity = serverData.bulletVelocity,
            });

            commandBuffer.AddComponent(req, new SendRpcCommandRequestComponent { TargetConnection = entity });
        }).Schedule();

        m_Barrier.AddJobHandleForProducer(Dependency);
    }
}