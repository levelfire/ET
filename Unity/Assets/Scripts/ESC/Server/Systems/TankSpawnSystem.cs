using System.Diagnostics;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;
using UnityEngine;
public struct TankSpawnInProgressTag : IComponentData
{
}

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public partial class TankSpawnSystem : SystemBase
{

    private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;
    private Entity m_Prefab;

    protected override void OnCreate()
    {
        m_BeginSimEcb = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();


        RequireSingletonForUpdate<GameSettingsComponent>();
    }

    protected override void OnUpdate()
    {
        if (m_Prefab == Entity.Null)
        {
            m_Prefab = GetSingleton<TankAuthoringComponent>().Prefab;
            return;
        }

        var commandBuffer = m_BeginSimEcb.CreateCommandBuffer();
        var playerPrefab = m_Prefab;
        var rand = new Unity.Mathematics.Random((uint)Stopwatch.GetTimestamp());
        var gameSettings = GetSingleton<GameSettingsComponent>();

        var playerStateFromEntity = GetComponentDataFromEntity<PlayerSpawningStateComponent>();

        var commandTargetFromEntity = GetComponentDataFromEntity<CommandTargetComponent>();
        var networkIdFromEntity = GetComponentDataFromEntity<NetworkIdComponent>();

        Entities
        .ForEach((Entity entity, in TankSpawnRequestRpc request,
            in ReceiveRpcCommandRequestComponent requestSource) =>
        {
            commandBuffer.DestroyEntity(entity);

            if (!playerStateFromEntity.HasComponent(requestSource.SourceConnection) ||
                !commandTargetFromEntity.HasComponent(requestSource.SourceConnection) ||
                commandTargetFromEntity[requestSource.SourceConnection].targetEntity != Entity.Null ||
                playerStateFromEntity[requestSource.SourceConnection].IsSpawning != 0)
                return;

            var player = commandBuffer.Instantiate(playerPrefab);

            var width = gameSettings.levelWidth * .2f;
            var height = gameSettings.levelHeight * .2f;
            var depth = gameSettings.levelDepth * .2f;


            var pos = new Translation
            {
                Value = new float3(
                    rand.NextFloat(-width, width),
                    0,
                    rand.NextFloat(-depth, depth))
            };

            var rot = new Rotation { Value = Quaternion.identity };

            commandBuffer.SetComponent(player, pos);
            commandBuffer.SetComponent(player, rot);
            
            commandBuffer.SetComponent(player, new GhostOwnerComponent { NetworkId = networkIdFromEntity[requestSource.SourceConnection].Value });
            commandBuffer.SetComponent(player, new TankEntityComponent { TankEntity = requestSource.SourceConnection });
            //TODO
            commandBuffer.SetComponent(player, new TankTeamComponent { TeamId = networkIdFromEntity[requestSource.SourceConnection].Value });

            commandBuffer.AddComponent(player, new TankSpawnInProgressTag());

            playerStateFromEntity[requestSource.SourceConnection] = new PlayerSpawningStateComponent { IsSpawning = 1 };
        }).Schedule();


        m_BeginSimEcb.AddJobHandleForProducer(Dependency);
    }
}

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateBefore(typeof(GhostSendSystem))]
public partial class TankCompleteSpawnSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;

    protected override void OnCreate()
    {
        m_BeginSimEcb = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = m_BeginSimEcb.CreateCommandBuffer();

        var playerStateFromEntity = GetComponentDataFromEntity<PlayerSpawningStateComponent>();
        var commandTargetFromEntity = GetComponentDataFromEntity<CommandTargetComponent>();
        var connectionFromEntity = GetComponentDataFromEntity<NetworkStreamConnection>();

        Entities.WithAll<TankSpawnInProgressTag>().
            ForEach((Entity entity, in TankEntityComponent tank) =>
            {
                if (!playerStateFromEntity.HasComponent(tank.TankEntity) ||
                    !connectionFromEntity[tank.TankEntity].Value.IsCreated)
                {
                    commandBuffer.DestroyEntity(entity);
                    return;
                }

                commandBuffer.RemoveComponent<TankSpawnInProgressTag>(entity);

                commandTargetFromEntity[tank.TankEntity] = new CommandTargetComponent { targetEntity = entity };
                playerStateFromEntity[tank.TankEntity] = new PlayerSpawningStateComponent { IsSpawning = 0 };
            }).Schedule();

        m_BeginSimEcb.AddJobHandleForProducer(Dependency);
    }
}