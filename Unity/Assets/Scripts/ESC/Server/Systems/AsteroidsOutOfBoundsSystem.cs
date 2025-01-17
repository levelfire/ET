using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.NetCode;


[UpdateInWorld(TargetWorld.Server)]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(EndFixedStepSimulationEntityCommandBufferSystem))]
public partial class AsteroidsOutOfBoundsSystem : SystemBase
{
    private EndFixedStepSimulationEntityCommandBufferSystem m_EndFixedStepSimECB;

    protected override void OnCreate()
    {
        m_EndFixedStepSimECB = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();

        RequireSingletonForUpdate<GameSettingsComponent>();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = m_EndFixedStepSimECB.CreateCommandBuffer().AsParallelWriter();

        var settings = GetSingleton<GameSettingsComponent>();

        Entities
        .WithAll<AsteroidTag>()
        .ForEach((Entity entity, int entityInQueryIndex, in Translation position) =>
        {
            if (Mathf.Abs(position.Value.x) > settings.levelWidth / 2 ||
                Mathf.Abs(position.Value.y) > settings.levelHeight / 2 ||
                Mathf.Abs(position.Value.z) > settings.levelDepth / 2)
            {
                commandBuffer.AddComponent(entityInQueryIndex, entity, new DestroyTag());
                return;
            }

        }).ScheduleParallel();

        m_EndFixedStepSimECB.AddJobHandleForProducer(Dependency);

    }
}