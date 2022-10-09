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


////[UpdateInWorld(TargetWorld.ClientAndServer)]
//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//[
//    UpdateBefore(typeof(BuildPhysicsWorld))
//]
//public partial class TankCastRaySystem : SystemBase
//{
//    private BuildPhysicsWorld m_BuildPhysicsWorldSystem;
//    protected override void OnCreate()
//    {
        
//        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
//    }

//    protected override void OnUpdate()
//    {
//        PhysicsWorld world = m_BuildPhysicsWorldSystem.PhysicsWorld;

//        Entities
//        .WithAll<TankTag>()
//        .ForEach((Entity entity, ref PhysicsVelocity velocity,
//        in Rotation rotation, in Translation position, in PlayerStateAndOffsetComponent offset) =>
//        {
//            if (velocity.Linear.x == 0 && velocity.Linear.z == 0)
//            {
//                return;
//            }
//            Debug.Log("RayCast: " + position.ToString());
//            var raybegin = new Translation { Value = position.Value + math.mul(rotation.Value, offset.RayBegin).xyz };
//            var rayend = new Translation { Value = position.Value + math.mul(rotation.Value, offset.RayEnd).xyz };


//            var input = new RaycastInput()
//            {
//                Start = raybegin.Value,
//                End = rayend.Value,

//                Filter = new CollisionFilter
//                {
//                    BelongsTo = 1 << 4,
//                    CollidesWith = (1 << 5) | (1 << 6), // all 1s, so all layers, collide with everything
//                    GroupIndex = 0
//                }
//            };

//            if (world.CastRay(input, out var hit))
//            {
//                Debug.Log("RayCast hit: " + position.ToString());
//                velocity.Linear = 0;
//            }
//        }).Schedule();

//        m_BuildPhysicsWorldSystem.AddInputDependencyToComplete(Dependency);
//    }
//}

