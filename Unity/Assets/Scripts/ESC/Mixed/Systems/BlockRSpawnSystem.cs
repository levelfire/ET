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

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class BlockRSpawnSystem : SystemBase
{
    private EntityQuery m_BlockQuery;

    private BeginSimulationEntityCommandBufferSystem m_BeginSimECB;

    private EntityQuery m_GameSettingsQuery;

    private Entity m_Prefab;

    private EntityQuery m_ConnectionGroup;

    private long m_Ts;

    protected override void OnCreate()
    {
        m_BlockQuery = GetEntityQuery(ComponentType.ReadWrite<BlockTag>());

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
            m_Prefab = GetSingleton<BlockRAuthoringComponent>().Prefab;
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

        var blockPrefab = m_Prefab;

        var rand = new Unity.Mathematics.Random((uint)Stopwatch.GetTimestamp());

        Job
        .WithCode(() => {
            for (int i = count; i < 100; ++i)
            {
                int x = i % 10;
                int z = i / 10;
                var pos = new Translation { Value = new float3(x, 0, z) };
                var e = commandBuffer.Instantiate(blockPrefab);
                commandBuffer.SetComponent(e, pos);
                commandBuffer.SetComponent(e, new HpComponent { Value = 2 });
            }
        }).Schedule();

        m_BeginSimECB.AddJobHandleForProducer(Dependency);
    }
}