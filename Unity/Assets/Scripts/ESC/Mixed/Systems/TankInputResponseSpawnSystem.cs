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
[UpdateAfter(typeof(ExportPhysicsWorld))]
public partial class TankInputResponseSpawnSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;

    private GhostPredictionSystemGroup m_PredictionGroup;

    private Entity m_BulletPrefab;

    private const int k_CoolDownTicksCount = 10;


    protected override void OnCreate()
    {
        m_BeginSimEcb = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        m_PredictionGroup = World.GetOrCreateSystem<GhostPredictionSystemGroup>();

        RequireSingletonForUpdate<GameSettingsComponent>();
        RequireSingletonForUpdate<BulletAuthoringComponent>();
    }

    protected override void OnUpdate()
    {

        if (m_BulletPrefab == Entity.Null)
        {
            var foundPrefab = GetSingleton<BulletAuthoringComponent>().Prefab;
            m_BulletPrefab = GhostCollectionSystem.CreatePredictedSpawnPrefab(EntityManager, foundPrefab);
            return;
        }

        var commandBuffer = m_BeginSimEcb.CreateCommandBuffer().AsParallelWriter();

        var bulletVelocity = GetSingleton<GameSettingsComponent>().bulletVelocity;
        var bulletPrefab = m_BulletPrefab;
        var deltaTime = m_PredictionGroup.Time.DeltaTime;
        var currentTick = m_PredictionGroup.PredictingTick;

        var inputFromEntity = GetBufferFromEntity<TankCommand>(true);

        Entities
        .WithReadOnly(inputFromEntity)
        .WithAll<TankTag, TankCommand>()
        .ForEach((Entity entity, int entityInQueryIndex, ref PlayerStateAndOffsetComponent bulletOffset, in Rotation rotation, in Translation position, in PhysicsVelocity velocityComponent,
                in GhostOwnerComponent ghostOwner, in PredictedGhostComponent prediction) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(currentTick, prediction))
                return;

            var input = inputFromEntity[entity];

            TankCommand inputData;
            if (!input.GetDataAtTick(currentTick, out inputData))
                inputData.shoot = 0;

            if (inputData.selfDestruct == 1)
            {
                commandBuffer.AddComponent<DestroyTag>(entityInQueryIndex, entity);
            }

            var canShoot = bulletOffset.WeaponCooldown == 0 || SequenceHelpers.IsNewer(currentTick, bulletOffset.WeaponCooldown);
            if (inputData.shoot != 0 && canShoot)
            {
                var bullet = commandBuffer.Instantiate(entityInQueryIndex, bulletPrefab);
                commandBuffer.AddComponent(entityInQueryIndex, bullet, new PredictedGhostSpawnRequestComponent());


                var newPosition = new Translation { Value = position.Value + math.mul(rotation.Value, bulletOffset.Value).xyz };

                var vel = new PhysicsVelocity { Linear = (bulletVelocity * math.mul(rotation.Value, new float3(0, 0, 1)).xyz)/* + velocityComponent.Linear*/ };

                commandBuffer.SetComponent(entityInQueryIndex, bullet, newPosition);
                commandBuffer.SetComponent(entityInQueryIndex, bullet, vel);
                commandBuffer.SetComponent(entityInQueryIndex, bullet,
                    new GhostOwnerComponent { NetworkId = ghostOwner.NetworkId });


                bulletOffset.WeaponCooldown = currentTick + k_CoolDownTicksCount;
            }

        }).ScheduleParallel();

        m_BeginSimEcb.AddJobHandleForProducer(Dependency);
    }
}