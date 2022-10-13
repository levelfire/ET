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
public partial class EmpDamageSystem : SystemBase
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
        .WithAll<EmpDotTag>()
        .ForEach((Entity entity, int entityInQueryIndex, ref HpComponent hpcom) =>
        {
            Debug.Log($"EmpDamageSystem hp {hpcom.Value}");
            var newhp = hpcom.Value - 1;
            if (newhp <= 0)
            {
                commandBuffer.AddComponent<DestroyTag>(entityInQueryIndex,entity);
            }
            else
            {
                commandBuffer.SetComponent(entityInQueryIndex,entity, new HpComponent { Value = newhp });
            }
        }).ScheduleParallel();

        //Entities
        //.WithAll<EmpTag>()
        //.ForEach((Entity entity, int entityInQueryIndex) =>
        //{
        //    var scale = new NonUniformScale { Value = new float3(10, 10, 10) };
        //    commandBuffer.AddComponent<NonUniformScale>(entityInQueryIndex,entity, scale);
        //    scale.Value = new float3(10, 10, 10);
        //    commandBuffer.SetComponent(entityInQueryIndex,entity, scale);
        //}).ScheduleParallel();

        m_EndFixedStepSimECB.AddJobHandleForProducer(Dependency);

    }
}