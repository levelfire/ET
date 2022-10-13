using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.NetCode;
using System.Diagnostics;

[UpdateInWorld(TargetWorld.Server)]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class DropItemDestructionSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem m_EndSimEcb;
    

    protected override void OnCreate()
    {
        m_EndSimEcb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = m_EndSimEcb.CreateCommandBuffer().AsParallelWriter();

        var rand = new Unity.Mathematics.Random((uint)Stopwatch.GetTimestamp());

        Entities
        .WithAll<DestroyTag, DropItemTag>()
        .ForEach((Entity entity, int entityInQueryIndex,in Translation trans) =>
        {
            commandBuffer.DestroyEntity(entityInQueryIndex, entity); 

        }).ScheduleParallel();

        m_EndSimEcb.AddJobHandleForProducer(Dependency);

    }
}