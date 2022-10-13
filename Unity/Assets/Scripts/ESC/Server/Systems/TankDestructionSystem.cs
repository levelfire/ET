//using Unity.Burst;
//using Unity.Entities;
//using Unity.Collections;
//using Unity.Mathematics;
//using Unity.Jobs;
//using Unity.Transforms;
//using UnityEngine;
//using Unity.NetCode;

//[UpdateInWorld(TargetWorld.Server)]
//[UpdateInGroup(typeof(LateSimulationSystemGroup))]
//public partial class TankDestructionSystem : SystemBase
//{
//    private EndSimulationEntityCommandBufferSystem m_EndSimEcb;

//    protected override void OnCreate()
//    {
//        m_EndSimEcb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
//    }

//    protected override void OnUpdate()
//    {
//        var commandBuffer = m_EndSimEcb.CreateCommandBuffer().AsParallelWriter();

//        Entities
//        .WithAll<DestroyTag, TankTag>()
//        .ForEach((Entity entity, int entityInQueryIndex) =>
//        {
//            commandBuffer.DestroyEntity(entityInQueryIndex, entity);

//        }).ScheduleParallel();

//        m_EndSimEcb.AddJobHandleForProducer(Dependency);

//    }
//}


using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.NetCode;
using System.Diagnostics;

//We are going to update LATE once all other systems are complete
//because we don't want to destroy the Entity before other systems have
//had a chance to interact with it if they need to
[UpdateInWorld(TargetWorld.Server)]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class TankDestructionSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem m_BeginSimECB;
    private EndSimulationEntityCommandBufferSystem m_EndSimEcb;

    private Entity m_PrefabDropItem;
    protected override void OnCreate()
    {
        //We grab the EndSimulationEntityCommandBufferSystem to record our structural changes
        m_EndSimEcb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_BeginSimECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        if (m_PrefabDropItem == Entity.Null)
        {
            m_PrefabDropItem = GetSingleton<DropItemAuthoringComponent>().Prefab;
            return;
        }
        var Prefab = m_PrefabDropItem;

        //We add "AsParallelWriter" when we create our command buffer because we want
        //to run our jobs in parallel
        var commandBuffer = m_EndSimEcb.CreateCommandBuffer().AsParallelWriter();
        var commandBufferBegin = m_BeginSimECB.CreateCommandBuffer().AsParallelWriter();
        var commandTargetFromEntity = GetComponentDataFromEntity<CommandTargetComponent>(false);

        var rand = new Unity.Mathematics.Random((uint)Stopwatch.GetTimestamp());

        Entities
        .WithNativeDisableParallelForRestriction(commandTargetFromEntity)
        .WithAll<DestroyTag, TankTag>()
        .ForEach((Entity entity, int entityInQueryIndex, in TankEntityComponent playerEntity, in Translation trans) =>
        {
            // Reset the CommandTargetComponent on the Network Connection Entity to the player
            //We are able to find the NCE the player belongs to through the PlayerEntity component
            var state = commandTargetFromEntity[playerEntity.TankEntity];
            state.targetEntity = Entity.Null;
            commandTargetFromEntity[playerEntity.TankEntity] = state;

            DynamicBuffer<PackageData> packageList = GetBuffer<PackageData>(entity);
            for (int i = 0; i < packageList.Length; i++)
            {
                var pos = new Translation { Value = new float3(rand.NextInt((int)trans.Value.x - 5, (int)trans.Value.x + 5), 0, rand.NextInt((int)trans.Value.z - 5, (int)trans.Value.z + 5)) };
                var e = commandBufferBegin.Instantiate(entityInQueryIndex, Prefab);
                commandBufferBegin.SetComponent(entityInQueryIndex, e, pos);
                commandBufferBegin.SetComponent(entityInQueryIndex, e, new DropItemComponent(packageList[i].Uuid, packageList[i].ItemId));
            }

            //Then destroy the entity
            commandBuffer.DestroyEntity(entityInQueryIndex, entity);

        }).Schedule();//.ScheduleParallel();

        //We then add the dependencies of these jobs to the EndSimulationEntityCOmmandBufferSystem
        //that will be playing back the structural changes recorded in this sytem
        m_EndSimEcb.AddJobHandleForProducer(Dependency);

    }
}
