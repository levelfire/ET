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
public partial class BlockDestructionSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem m_BeginSimECB;
    private EndSimulationEntityCommandBufferSystem m_EndSimEcb;
    

    private Entity m_PrefabDropItem;

    protected override void OnCreate()
    {
        m_BeginSimECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        m_EndSimEcb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        if (m_PrefabDropItem == Entity.Null)
        {
            m_PrefabDropItem = GetSingleton<DropItemAuthoringComponent>().Prefab;
            return;
        }

        var commandBuffer = m_EndSimEcb.CreateCommandBuffer().AsParallelWriter();
        var commandBufferBegin = m_BeginSimECB.CreateCommandBuffer().AsParallelWriter();

        var Prefab = m_PrefabDropItem;
        var rand = new Unity.Mathematics.Random((uint)Stopwatch.GetTimestamp());

        Entities
        .WithAll<DestroyTag, BlockTag>()
        .ForEach((Entity entity, int entityInQueryIndex,in Translation trans) =>
        {
            if (rand.NextBool())
            {
                var pos = new Translation { Value = new float3(trans.Value.x, 0, trans.Value.z) };
                var e = commandBufferBegin.Instantiate(entityInQueryIndex, Prefab);
                commandBufferBegin.SetComponent(entityInQueryIndex, e, pos);
                commandBufferBegin.SetComponent(entityInQueryIndex, e, new DropItemComponent(e.Index,101));
            }

            commandBuffer.DestroyEntity(entityInQueryIndex, entity); 

        }).ScheduleParallel();

        m_EndSimEcb.AddJobHandleForProducer(Dependency);

    }
}