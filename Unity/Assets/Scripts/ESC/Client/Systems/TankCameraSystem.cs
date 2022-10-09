using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


[UpdateAfter(typeof(TransformSystemGroup))]
[UpdateInWorld(TargetWorld.Client)]
public partial class TankCameraSystem : SystemBase
{
    Quaternion newQuaternion = Quaternion.identity;
    //float lastX = 0;
    //float lastZ = 0;
    //bool bind = false;
    protected override void OnCreate()
    {
        newQuaternion.eulerAngles = new Vector3(90, 0, 0);

        //RequireSingletonForUpdate<PredictionSwitchingSpawner>();
        RequireSingletonForUpdate<NetworkIdComponent>();
    }

    protected override void OnUpdate()
    {
        var networkIdComponent = GetSingleton<NetworkIdComponent>();
        Entities
            .WithoutBurst()
            .WithAll<TankTag, TankClassifiedTag>()
            .ForEach((Entity entity, int entityInQueryIndex, in Translation trans, in GhostOwnerComponent ghostOwnerComponent,in PhysicsVelocity velocity) =>
            {
                if (ghostOwnerComponent.NetworkId != networkIdComponent.Value)
                    return;
                //if (velocity.Linear.x != 0 || velocity.Linear.z != 0)
                //{
                //    UnityEngine.Camera.main.transform.rotation = newQuaternion;
                //    //UnityEngine.Camera.main.transform.position = trans.Value + new float3(0, 20, 0);
                //    var nX = (trans.Value.x + UnityEngine.Camera.main.transform.position.x) / 2;
                //    var nZ = (trans.Value.z + UnityEngine.Camera.main.transform.position.z) / 2;
                //    UnityEngine.Camera.main.transform.position = new float3(nX, 20, nZ);
                //    //UnityEngine.Camera.main.transform.position = new float3(trans.Value.x, 20, trans.Value.z);
                //}
                //else if (UnityEngine.Camera.main.transform.position.x != trans.Value.x
                //&& UnityEngine.Camera.main.transform.position.z != trans.Value.z)
                //{
                //    UnityEngine.Camera.main.transform.rotation = newQuaternion;
                //    UnityEngine.Camera.main.transform.position = new float3(trans.Value.x, 20, trans.Value.z);
                //}

                //if (math.abs(lastX - velocity.Linear.x) > 0.1f || math.abs(lastZ - velocity.Linear.z) > 0.1f)
                //{
                //    lastX = velocity.Linear.x;
                //    lastZ = velocity.Linear.z;
                //    var nX = (trans.Value.x + UnityEngine.Camera.main.transform.position.x) / 2;
                //    var nZ = (trans.Value.z + UnityEngine.Camera.main.transform.position.z) / 2;
                //    UnityEngine.Camera.main.transform.position = new float3(nX, 20, nZ);
                //}
                //else
                //{
                //    UnityEngine.Camera.main.transform.rotation = newQuaternion;
                //    UnityEngine.Camera.main.transform.position = new float3(trans.Value.x, 20, trans.Value.z);
                //}

                //if(trans.Value.x != lastX)
                //{
                //    //Debug.Log("tank trans x:" + trans.Value.x.ToString());
                //    //UnityEngine.Debug.Log($"tank trans x: {trans.Value.x}");
                //    lastX = trans.Value.x;
                //}

                //if (!bind)
                //{
                //    UnityEngine.Camera.main.transform.rotation = newQuaternion;
                //    UnityEngine.Camera.main.transform.position = new float3(trans.Value.x, 20, trans.Value.z);
                //    bind = true;
                //}

                UnityEngine.Camera.main.transform.rotation = newQuaternion;
                UnityEngine.Camera.main.transform.position = new float3(trans.Value.x, 20, trans.Value.z);
                //var nX = (trans.Value.x + UnityEngine.Camera.main.transform.position.x) / 2;
                //var nZ = (trans.Value.z + UnityEngine.Camera.main.transform.position.z) / 2;
                //UnityEngine.Camera.main.transform.position = new float3(nX, 20, nZ);

            }).Run();
    }
}
