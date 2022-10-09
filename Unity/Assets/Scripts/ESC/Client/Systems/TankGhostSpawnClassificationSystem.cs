using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

[UpdateInWorld(TargetWorld.Client)]
[UpdateInGroup(typeof(GhostSimulationSystemGroup))]
[UpdateAfter(typeof(GhostSpawnClassificationSystem))]
public partial class TankGhostSpawnClassificationSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;

    private Entity m_CameraPrefab;

    protected override void OnCreate()
    {
        m_BeginSimEcb = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireSingletonForUpdate<NetworkIdComponent>();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = m_BeginSimEcb.CreateCommandBuffer().AsParallelWriter();

        var camera = m_CameraPrefab;
        var networkIdComponent = GetSingleton<NetworkIdComponent>();
        var commandTargetFromEntity = GetComponentDataFromEntity<CommandTargetComponent>(false);

        Entities
        .WithAll<TankTag>()
        .WithNone<TankClassifiedTag>()
        .ForEach((Entity entity, int entityInQueryIndex, in GhostOwnerComponent ghostOwnerComponent) =>
        {
            commandBuffer.AddComponent(entityInQueryIndex, entity, new TankClassifiedTag());

        }).ScheduleParallel();     

        m_BeginSimEcb.AddJobHandleForProducer(Dependency);
    }
}

