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
public partial class AsteroidSpawnSystem : SystemBase
{
    private EntityQuery m_AsteroidQuery;

    private BeginSimulationEntityCommandBufferSystem m_BeginSimECB;

    private EntityQuery m_GameSettingsQuery;

    private Entity m_Prefab;

    private EntityQuery m_ConnectionGroup;

    protected override void OnCreate()
    {
        m_AsteroidQuery = GetEntityQuery(ComponentType.ReadWrite<AsteroidTag>());

        m_BeginSimECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        m_GameSettingsQuery = GetEntityQuery(ComponentType.ReadWrite<GameSettingsComponent>());

        RequireForUpdate(m_GameSettingsQuery);

        m_ConnectionGroup = GetEntityQuery(ComponentType.ReadWrite<NetworkStreamConnection>());
    }

    protected override void OnUpdate()
    {

    }
}