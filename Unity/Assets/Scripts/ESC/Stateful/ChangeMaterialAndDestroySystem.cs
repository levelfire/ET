using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics.Stateful;
using Unity.Rendering;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(StatefulTriggerEventBufferSystem))]
public partial class ChangeMaterialAndDestroySystem : SystemBase
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
            .WithName("ChangeMaterialOnTriggerEnter")
            .WithoutBurst()
            .ForEach((Entity e, ref DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer) =>
            {
                for (int i = 0; i < triggerEventBuffer.Length; i++)
                {
                    var triggerEvent = triggerEventBuffer[i];
                    var otherEntity = triggerEvent.GetOtherEntity(e);

                    if (triggerEvent.State == StatefulEventState.Stay || !nonTriggerMask.Matches(otherEntity))
                    {
                        continue;
                    }

                    if (triggerEvent.State == StatefulEventState.Enter)
                    {
                        var volumeRenderMesh = EntityManager.GetSharedComponentData<RenderMesh>(e);
                        var overlappingRenderMesh = EntityManager.GetSharedComponentData<RenderMesh>(otherEntity);
                        overlappingRenderMesh.material = volumeRenderMesh.material;

                        commandBuffer.SetSharedComponent(otherEntity, overlappingRenderMesh);

                        commandBuffer.AddComponent(e, new DestroyTag { });
                        var hashp = EntityManager.HasComponent<HpComponent>(otherEntity);
                        if (hashp)
                        {
                            var hpcom = EntityManager.GetComponentData<HpComponent>(otherEntity);
                            var newhp = hpcom.Value - 1;
                            if (newhp <= 0)
                            {
                                commandBuffer.AddComponent(otherEntity, new DestroyTag { });
                            }
                            else
                            {
                                commandBuffer.SetComponent(otherEntity, new HpComponent { Value = newhp });
                            }
                        }
                        else
                        {
                            commandBuffer.AddComponent(otherEntity, new DestroyTag { });
                        }
                    }
                    else
                    {
                        //commandBuffer.AddComponent(otherEntity, new DestroyTag { });
                    }
                }
            }).Run();

        m_CommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}