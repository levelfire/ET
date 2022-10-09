using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;
using Unity.Networking.Transport.Utilities;
using Unity.Collections;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;
using UnityEngine;


[UpdateInWorld(TargetWorld.ClientAndServer)]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
public partial class TankInputResponseMovementSystem : SystemBase
{
    private GhostPredictionSystemGroup m_PredictionGroup;
    private BuildPhysicsWorld m_BuildPhysicsWorldSystem;

    protected override void OnCreate()
    {
        m_PredictionGroup = World.GetOrCreateSystem<GhostPredictionSystemGroup>();
        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();

        Entity physicsSingleton = EntityManager.CreateEntity();
        EntityManager.AddComponentData(physicsSingleton, new PredictedPhysicsConfig { PhysicsTicksPerSimTick = 1, DisableWhenNoConnections = true });

        var smoothing = World.GetExistingSystem<GhostPredictionSmoothingSystem>();
        smoothing?.RegisterSmoothingAction<Translation, DefaultUserParams>(DefaultTranslateSmoothingAction.Action);
        //smoothing?.RegisterSmoothingAction<Translation, DefaultUserParams>(TankSmoothingAction.Action);
        //smoothing?.RegisterSmoothingAction<Translation>(TankSmoothingAction.Action);
        //smoothing?.RegisterSmoothingAction<Translation>(DefaultTranslateSmoothingAction.Action);
    }

    protected override void OnUpdate()
    {
        var currentTick = m_PredictionGroup.PredictingTick;
        var deltaTime = m_PredictionGroup.Time.DeltaTime;

        var playerForce = GetSingleton<GameSettingsComponent>().playerForce;

        var inputFromEntity = GetBufferFromEntity<TankCommand>(true);
        PhysicsWorld world = m_BuildPhysicsWorldSystem.PhysicsWorld;

        Entities
        .WithReadOnly(inputFromEntity)
        .WithAll<TankTag, TankCommand>()
        .ForEach((Entity entity, int entityInQueryIndex, ref Rotation rotation, ref PhysicsVelocity velocity,
                in GhostOwnerComponent ghostOwner, in PredictedGhostComponent prediction,
                in Translation position, in PlayerStateAndOffsetComponent offset) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(currentTick, prediction))
                return;

            var input = inputFromEntity[entity];

            TankCommand inputData;
            if (!input.GetDataAtTick(currentTick, out inputData))
                inputData.shoot = 0;

            if (!(inputData.right == 1 || inputData.left == 1 || inputData.thrust == 1 || inputData.reverseThrust == 1))
            {
                velocity.Linear = 0;
            }
            else
            {
                Quaternion newQuaternion = Quaternion.identity;

                velocity.Linear = 0;
                if (inputData.right == 1)
                {
                    velocity.Linear.x = playerForce;
                    newQuaternion.eulerAngles = new Vector3(0, 90, 0);
                }
                else if (inputData.left == 1)
                {
                    velocity.Linear.x = -playerForce;
                    newQuaternion.eulerAngles = new Vector3(0, -90, 0);
                }
                else if (inputData.thrust == 1)
                {
                    velocity.Linear.z = playerForce;
                    newQuaternion.eulerAngles = new Vector3(0, 0, 0);
                }
                else if (inputData.reverseThrust == 1)
                {
                    velocity.Linear.z = -playerForce;
                    newQuaternion.eulerAngles = new Vector3(0, 180, 0);
                }
                rotation.Value = newQuaternion;

                var ColFil = new CollisionFilter
                {
                    BelongsTo = 1 << 4,
                    CollidesWith = (1 << 5) | (1 << 6),
                    GroupIndex = 0
                };

                var lenRay = 0.2f;
                var raybegin = new Translation { Value = position.Value + math.mul(rotation.Value, offset.RayBegin).xyz };
                var rayend = new Translation { Value = position.Value + math.mul(rotation.Value, new float3(offset.RayBegin.x, 0, offset.RayBegin.z + lenRay)).xyz };
                var rayinput = new RaycastInput()
                {
                    Start = raybegin.Value,
                    End = rayend.Value,

                    Filter = ColFil
                };
                if (world.CastRay(rayinput))
                {
                    velocity.Linear = 0;
                    return;
                }
                var begin2 = new float3(offset.RayBegin.x + 0.25f, 0, offset.RayBegin.z);
                var raybegin2 = new Translation { Value = position.Value + math.mul(rotation.Value, begin2).xyz };
                var rayend2 = new Translation { Value = position.Value + math.mul(rotation.Value, new float3(begin2.x, 0, begin2.z + lenRay)).xyz };

                var rayinput2 = new RaycastInput()
                {
                    Start = raybegin2.Value,
                    End = rayend2.Value,

                    Filter = ColFil
                };
                if (world.CastRay(rayinput2))
                {
                    velocity.Linear = 0;
                    return;
                }

                var begin3 = new float3(offset.RayBegin.x - 0.25f, 0, offset.RayBegin.z);
                var raybegin3 = new Translation { Value = position.Value + math.mul(rotation.Value, begin3).xyz };
                var rayend3 = new Translation { Value = position.Value + math.mul(rotation.Value, new float3(begin3.x, 0, begin3.z + lenRay)).xyz };

                var rayinput3 = new RaycastInput()
                {
                    Start = raybegin3.Value,
                    End = rayend3.Value,

                    Filter = ColFil
                };
                if (world.CastRay(rayinput3))
                {
                    velocity.Linear = 0;
                    return;
                }

                var ColFil2 = new CollisionFilter
                {
                    BelongsTo = 1 << 4,
                    CollidesWith = (1 << 4) | (1 << 5) | (1 << 6),
                    GroupIndex = 0
                };

                var begin4 = new float3(offset.RayBegin.x - 0.5f, 0, offset.RayBegin.z);
                var raybegin4 = new Translation { Value = position.Value + math.mul(rotation.Value, begin4).xyz };
                var rayend4 = new Translation { Value = position.Value + math.mul(rotation.Value, new float3(begin4.x, 0, begin4.z + lenRay)).xyz };

                var rayinput4 = new RaycastInput()
                {
                    Start = raybegin4.Value,
                    End = rayend4.Value,

                    Filter = ColFil2
                };

                if (world.CastRay(rayinput4))
                {
                    velocity.Linear = 0;
                    return;
                }

                var begin5 = new float3(offset.RayBegin.x + 0.5f, 0, offset.RayBegin.z);
                var raybegin5 = new Translation { Value = position.Value + math.mul(rotation.Value, begin5).xyz };
                var rayend5 = new Translation { Value = position.Value + math.mul(rotation.Value, new float3(begin5.x, 0, begin5.z + lenRay)).xyz };

                var rayinput5 = new RaycastInput()
                {
                    Start = raybegin5.Value,
                    End = rayend5.Value,

                    Filter = ColFil2
                };

                if (world.CastRay(rayinput5))
                {
                    velocity.Linear = 0;
                    return;
                }
            }

        }).Schedule();
        //.ScheduleParallel();

        m_BuildPhysicsWorldSystem.AddInputDependencyToComplete(Dependency);
    }

    public bool Raycast(PhysicsWorld world, Translation position,Quaternion quaternion, float3 RayFrom)
    {
        var raybegin = new Translation { Value = position.Value + math.mul(quaternion, RayFrom).xyz };
        var rayend = new Translation { Value = position.Value + math.mul(quaternion, new float3(RayFrom.x, 0, RayFrom.z + 0.5f)).xyz };

        var rayinput = new RaycastInput()
        {
            Start = raybegin.Value,
            End = rayend.Value,

            Filter = new CollisionFilter
            {
                BelongsTo = 1 << 4,
                CollidesWith = (1 << 5) | (1 << 6), // all 1s, so all layers, collide with everything
                GroupIndex = 0
            }
        };

        return world.CastRay(rayinput);
    }

    public bool RaycastMul(PhysicsWorld world, Translation position, Quaternion quaternion, float3 RayFrom)
    {
        return Raycast(world, position, quaternion, RayFrom)
            || Raycast(world, position, quaternion, new float3(RayFrom.x - 0.25f, 0, RayFrom.z))
            || Raycast(world, position, quaternion, new float3(RayFrom.x + 0.25f, 0, RayFrom.z));
    }
}
