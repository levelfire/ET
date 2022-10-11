using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Physics.Stateful;
using Unity.Rendering;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(StatefulTriggerEventBufferSystem))]
public partial class EmpTriggerSystem : SystemBase
{
    private EndFixedStepSimulationEntityCommandBufferSystem m_CommandBufferSystem;

    private EntityQueryMask m_NonTriggerMask;

    protected override void OnCreate()
    {
        m_CommandBufferSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        m_NonTriggerMask = EntityManager.GetEntityQueryMask(
            GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[]
                {
                    typeof(StatefulTriggerEvent)
                }
            })
        );
    }

    protected override void OnUpdate()
    {
        var commandBuffer = m_CommandBufferSystem.CreateCommandBuffer();

        var nonTriggerMask = m_NonTriggerMask;

        Entities
            .WithAll<EmpTag>()
            .WithoutBurst()
            .ForEach((Entity e, ref DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer) =>
            {
                for (int i = 0; i < triggerEventBuffer.Length; i++)
                {
                    Debug.Log($"trigger emp 1");
                    var triggerEvent = triggerEventBuffer[i];
                    var otherEntity = triggerEvent.GetOtherEntity(e);

                    if (triggerEvent.State == StatefulEventState.Stay || !nonTriggerMask.Matches(otherEntity))
                    {
                        continue;
                    }
                    Debug.Log($"trigger emp 2");
                    var hasCom = EntityManager.HasComponent<TankStacksCountComponent>(otherEntity);
                    if (hasCom)
                    {
                        Debug.Log($"trigger emp 3");
                        var StackCom = EntityManager.GetComponentData<TankStacksCountComponent>(otherEntity);
                        int newStack = StackCom.EmpStackCount;
                        if (triggerEvent.State == StatefulEventState.Enter)
                        {
                            newStack += 1;
                            if (newStack == 1 && !EntityManager.HasComponent<EmpDotTag>(otherEntity))
                            {
                                commandBuffer.AddComponent(otherEntity, new EmpDotTag { });
                            }
                            Debug.Log($"enter emp {newStack}");
                        }
                        else 
                        {
                            newStack -= 1;
                            if (newStack == 0 && EntityManager.HasComponent<EmpDotTag>(otherEntity))
                            {
                                commandBuffer.RemoveComponent<EmpDotTag>(otherEntity);
                            }
                            Debug.Log($"leave emp {newStack}");
                        }
                        commandBuffer.SetComponent(otherEntity, new TankStacksCountComponent { EmpStackCount = newStack });
                    } 
                }
            }).Run();

        m_CommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}