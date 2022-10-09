//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Unity.NetCode;
//using Unity.Networking.Transport.Utilities;
//using Unity.Collections;
//using Unity.Physics;
//using Unity.Physics.Systems;
//using Unity.Jobs;
//using UnityEngine;


//[UpdateInWorld(TargetWorld.ClientAndServer)]
//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//[UpdateAfter(typeof(BuildPhysicsWorld)), UpdateBefore(typeof(StepPhysicsWorld))]
//public partial class TankInputRaySystem : SystemBase
//{
//    BuildPhysicsWorld m_BuildPhysicsWorldSystem;


//    protected override void OnCreate()
//    {
//        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
//    }

//    protected override void OnUpdate()
//    {
//        //var currentTick = m_PredictionGroup.PredictingTick;
//        //var deltaTime = m_PredictionGroup.Time.DeltaTime;

//        //var playerForce = GetSingleton<GameSettingsComponent>().playerForce;

//        //var inputFromEntity = GetBufferFromEntity<TankCommand>(true);
//        PhysicsWorld world = m_BuildPhysicsWorldSystem.PhysicsWorld;
//        Entities
//        ////.WithoutBurst()
//        .ForEach((Entity entity,
//                ref PhysicsVelocity velocity,
//                in Rotation rotation, in Translation position, in PlayerStateAndOffsetComponent offset
//                ) =>
//        {
//        //    //    //if (velocity.Linear == 0)
//        //    //    //{
//        //    //    //    return;
//        //    //    //}
//        //    //    //Debug.Log("RayCast: " + position.ToString());
//        //    //    var raybegin = new Translation { Value = position.Value + math.mul(rotation.Value, offset.RayBegin).xyz };
//        //    //    var rayend = new Translation { Value = position.Value + math.mul(rotation.Value, offset.RayEnd).xyz };

//        //    //    //////var rayInput = new Unity.Physics.RaycastInput();
//        //    //    //////rayInput.Start = raybegin.Value;
//        //    //    //////rayInput.End = rayend.Value;
//        //    //    //////rayInput.Filter = Unity.Physics.CollisionFilter.Default;
//        //    //    //////bool hit = collWorld.CastRay(rayInput);
//        //    //    //////if (hit)
//        //    //    //////{
//        //    //    //////    velocity.Linear = 0;
//        //    //    //////}
//        //    //    ///
//        //    //    //if (Raycast(raybegin.Value, rayend.Value) != Entity.Null)
//        //    //    //{

//        //    //    //}

//        //    //    var input = new RaycastInput()
//        //    //    {
//        //    //        Start = raybegin.Value,
//        //    //        End = rayend.Value,

//        //    //        Filter = new CollisionFilter
//        //    //        {
//        //    //            BelongsTo = 1 << 4,
//        //    //            CollidesWith = (1 << 5) | (1 << 6), // all 1s, so all layers, collide with everything
//        //    //            GroupIndex = 0
//        //    //        }
//        //    //    };

//        //    //    if (world.CastRay(input, out var hit))
//        //    //    {
//        //    //        //Debug.Log("RayCast hit: " + position.ToString());
//        //    //        velocity.Linear = 0;
//        //    //    }
//        //    //    //////return;  //always false :(

//        }).Schedule();
//            ////.Run();
//            ////.Schedule();
//            //////.ScheduleParallel();
//    }

//    //public Entity Raycast(float3 RayFrom, float3 RayTo)
//    //{
//    //    var physicsWorldSystem = m_BuildPhysicsWorld;
//    //    var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
//    //    RaycastInput input = new RaycastInput()
//    //    {
//    //        Start = RayFrom,
//    //        End = RayTo,
//    //        Filter = new CollisionFilter()
//    //        {
//    //            BelongsTo = 1 << 4,
//    //            CollidesWith = (1 << 5) | (1 << 6), // all 1s, so all layers, collide with everything
//    //            GroupIndex = 0
//    //        }
//    //    };

//    //    Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
//    //    bool haveHit = collisionWorld.CastRay(input, out hit);
//    //    if (haveHit)
//    //    {
//    //        // see hit.Position
//    //        // see hit.SurfaceNormal
//    //        Entity e = physicsWorldSystem.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
//    //        return e;
//    //    }
//    //    return Entity.Null;
//    //}
//}

