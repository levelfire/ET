using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


[UpdateAfter(typeof(TransformSystemGroup))]
[UpdateInWorld(TargetWorld.Client)]
public partial class TankDataSyncSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
    }

    protected override void OnUpdate()
    {
        var networkIdComponent = GetSingleton<NetworkIdComponent>();
        Entities
            .WithoutBurst()
            .WithAll<TankTag, HpComponent>()
            .ForEach((Entity entity, int entityInQueryIndex, in HpComponent hpCom, in GhostOwnerComponent ghostOwnerComponent) =>
            {
                if (ghostOwnerComponent.NetworkId != networkIdComponent.Value)
                    return;

                Debug.Log($"client tank hp {hpCom.Value}");

            }).Schedule();
    }
}

