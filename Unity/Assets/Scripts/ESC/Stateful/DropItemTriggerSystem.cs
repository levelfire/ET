using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Physics.Stateful;
using UnityEngine;

[UpdateInWorld(TargetWorld.Server)]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(StatefulTriggerEventBufferSystem))]
public partial class DropItemTriggerSystem : SystemBase
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
            .WithAll<DropItemTag>()
            .WithNone<DestroyTag>()
            .ForEach((Entity e,ref DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer,in DropItemComponent drop) =>
            {
                for (int i = 0; i < triggerEventBuffer.Length; i++)
                {
                    var triggerEvent = triggerEventBuffer[i];
                    var otherEntity = triggerEvent.GetOtherEntity(e);

                    if (triggerEvent.State == StatefulEventState.Stay || !nonTriggerMask.Matches(otherEntity))
                    {
                        continue;
                    }
                    bool pick = false;
                    DynamicBuffer<PickData> pickList = GetBuffer<PickData>(otherEntity);
                    DynamicBuffer<PackageData> packageList = GetBuffer<PackageData>(otherEntity);
                    if (triggerEvent.State == StatefulEventState.Enter)
                    {
                        Debug.Log($"pick Enter len {pickList.Length} cap {pickList.Capacity}");
                        Debug.Log($"packageList Enter len {packageList.Length} cap {packageList.Capacity}");
                        //TODO
                        if (packageList.Length < packageList.Capacity)
                        {
                            packageList.Add(new PackageData() { Uuid = drop.Uuid, ItemId = drop.ItemId });
                            pick = true;
                        }
                        else if(pickList.Length < pickList.Capacity)
                        {
                            pickList.Add(new PickData() { Uuid = drop.Uuid, ItemId = drop.ItemId });
                        }
                        
                        if (pick)
                        {
                            Debug.Log("pick up");
                            commandBuffer.AddComponent(e, new DestroyTag { });
                            return;
                        }
                    }
                    else 
                    {
                        Debug.Log("pick Leave");
                        //TODO use tank stay in client world
                        for (int j = 0; j < pickList.Length; j++)
                        {
                            if (pickList[j].Uuid == drop.Uuid)
                            {
                                Debug.Log($"pick remove{j}");
                                pickList.RemoveAt(j);
                                break;
                            }
                        }
                    }
                }
            }).Schedule();

        m_CommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}