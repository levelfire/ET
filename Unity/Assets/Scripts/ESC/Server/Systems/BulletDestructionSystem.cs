using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.NetCode;

[UpdateInWorld(TargetWorld.Server)]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class BulletDestructionSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem m_EndSimEcb;

    protected override void OnCreate()
    {
        m_EndSimEcb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = m_EndSimEcb.CreateCommandBuffer().AsParallelWriter();

        Entities
        .WithAll<DestroyTag, BulletTag>()
        .ForEach((Entity entity, int entityInQueryIndex) =>
        {
            commandBuffer.DestroyEntity(entityInQueryIndex, entity);

        }).ScheduleParallel();

        m_EndSimEcb.AddJobHandleForProducer(Dependency);

    }
}