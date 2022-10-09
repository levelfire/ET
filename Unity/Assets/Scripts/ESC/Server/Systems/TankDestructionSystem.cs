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

//We are going to update LATE once all other systems are complete
//because we don't want to destroy the Entity before other systems have
//had a chance to interact with it if they need to
[UpdateInWorld(TargetWorld.Server)]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class TankDestructionSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem m_EndSimEcb;

    protected override void OnCreate()
    {
        //We grab the EndSimulationEntityCommandBufferSystem to record our structural changes
        m_EndSimEcb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        //We add "AsParallelWriter" when we create our command buffer because we want
        //to run our jobs in parallel
        var commandBuffer = m_EndSimEcb.CreateCommandBuffer().AsParallelWriter();

        var commandTargetFromEntity = GetComponentDataFromEntity<CommandTargetComponent>(false);

        Entities
        .WithNativeDisableParallelForRestriction(commandTargetFromEntity)
        .WithAll<DestroyTag, TankTag>()
        .ForEach((Entity entity, int entityInQueryIndex, in TankEntityComponent playerEntity) =>
        {
            // Reset the CommandTargetComponent on the Network Connection Entity to the player
            //We are able to find the NCE the player belongs to through the PlayerEntity component
            var state = commandTargetFromEntity[playerEntity.TankEntity];
            state.targetEntity = Entity.Null;
            commandTargetFromEntity[playerEntity.TankEntity] = state;

            //Then destroy the entity
            commandBuffer.DestroyEntity(entityInQueryIndex, entity);

        }).ScheduleParallel();

        //We then add the dependencies of these jobs to the EndSimulationEntityCOmmandBufferSystem
        //that will be playing back the structural changes recorded in this sytem
        m_EndSimEcb.AddJobHandleForProducer(Dependency);

    }
}
