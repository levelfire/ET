using System.Diagnostics;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;
using Unity.Physics;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public partial class BlockSpawnSystem : SystemBase
{
    private EntityQuery m_BlockQuery;
    private EntityQuery m_WaterQuery;
    private EntityQuery m_GrassQuery;

    private BeginSimulationEntityCommandBufferSystem m_BeginSimECB;

    private EntityQuery m_GameSettingsQuery;

    private Entity m_Prefab;
    private Entity m_PrefabWater;
    private Entity m_PrefabGrass;

    private EntityQuery m_ConnectionGroup;

    private long m_Ts;

    protected override void OnCreate()
    {
        m_BlockQuery = GetEntityQuery(ComponentType.ReadWrite<BlockTag>());
        m_WaterQuery = GetEntityQuery(ComponentType.ReadWrite<WaterTag>());
        m_GrassQuery = GetEntityQuery(ComponentType.ReadWrite<GrassTag>());

        m_BeginSimECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        m_GameSettingsQuery = GetEntityQuery(ComponentType.ReadWrite<GameSettingsComponent>());

        RequireForUpdate(m_GameSettingsQuery);

        m_ConnectionGroup = GetEntityQuery(ComponentType.ReadWrite<NetworkStreamConnection>());

        m_Ts = System.DateTimeOffset.Now.ToUnixTimeSeconds() + 60;
    }

    protected override void OnUpdate()
    {
        if (m_Prefab == Entity.Null)
        {
            m_Prefab = GetSingleton<BlockAuthoringComponent>().Prefab;
            m_PrefabWater = GetSingleton<WaterAuthoringComponent>().Prefab;
            m_PrefabGrass = GetSingleton<GrassAuthoringComponent>().Prefab;
            return;
        }

        if (System.DateTimeOffset.Now.ToUnixTimeSeconds() >= m_Ts)
        {
            Application.Quit();
            return;
        }

        var settings = GetSingleton<GameSettingsComponent>();

        var commandBuffer = m_BeginSimECB.CreateCommandBuffer();

        var count = m_BlockQuery.CalculateEntityCountWithoutFiltering();
        var countWater = m_WaterQuery.CalculateEntityCountWithoutFiltering();
        var countGrass = m_GrassQuery.CalculateEntityCountWithoutFiltering();

        var blockPrefab = m_Prefab;
        var blockPrefabWater = m_PrefabWater;
        var blockPrefabGrass = m_PrefabGrass;

        var rand = new Unity.Mathematics.Random((uint)Stopwatch.GetTimestamp());

        Job
        .WithCode(() => {
            for (int i = count; i < settings.numBlocks; ++i)
            {
                var padding = 0.1f;

                var xPosition = rand.NextFloat(-1f * ((settings.levelWidth) / 2 - padding), (settings.levelWidth) / 2 - padding);
                var zPosition = rand.NextFloat(-1f * ((settings.levelDepth) / 2 - padding), (settings.levelDepth) / 2 - padding);
               
                var pos = new Translation { Value = new float3(xPosition, 0, zPosition) };

                
                var e = commandBuffer.Instantiate(blockPrefab);
                
                commandBuffer.SetComponent(e, pos);

                commandBuffer.SetComponent(e, new HpComponent { Value = 2 });
            }
            for (int i = countWater; i < settings.numBlocks; ++i)
            {
                var padding = 0.1f;

                var xPosition = rand.NextFloat(-1f * ((settings.levelWidth) / 2 - padding), (settings.levelWidth) / 2 - padding);
                var zPosition = rand.NextFloat(-1f * ((settings.levelDepth) / 2 - padding), (settings.levelDepth) / 2 - padding);
               
                var pos = new Translation { Value = new float3(xPosition, -0.1f, zPosition) };

                var e = commandBuffer.Instantiate(blockPrefabWater);

                commandBuffer.SetComponent(e, pos);
            }
            for (int i = countGrass; i < settings.numBlocks; ++i)
            {
                var padding = 0.1f;

                var xPosition = rand.NextFloat(-1f * ((settings.levelWidth) / 2 - padding), (settings.levelWidth) / 2 - padding);
                var zPosition = rand.NextFloat(-1f * ((settings.levelDepth) / 2 - padding), (settings.levelDepth) / 2 - padding);
                
                var pos = new Translation { Value = new float3(xPosition, -0.2f, zPosition) };

                var e = commandBuffer.Instantiate(blockPrefabGrass);

                commandBuffer.SetComponent(e, pos);
            }
        }).Schedule();

        m_BeginSimECB.AddJobHandleForProducer(Dependency);
    }
}