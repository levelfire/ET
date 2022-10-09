using AOT;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public unsafe struct TankSmoothingAction
{
    public abstract class DefaultStaticUserParams
    {
        public static readonly SharedStatic<float> maxDist = SharedStatic<float>.GetOrCreate<DefaultStaticUserParams, MaxDistKey>();
        public static readonly SharedStatic<float> delta = SharedStatic<float>.GetOrCreate<DefaultStaticUserParams, DeltaKey>();

        static DefaultStaticUserParams()
        {
            maxDist.Data = 10;
            delta.Data = 1;
        }
        class MaxDistKey { }
        class DeltaKey { }
    }

    public static PortableFunctionPointer<GhostPredictionSmoothingSystem.SmoothingActionDelegate>
        Action =
            new PortableFunctionPointer<GhostPredictionSmoothingSystem.SmoothingActionDelegate>(SmoothingAction);

    [BurstCompile(DisableDirectCall = true)]
    [MonoPInvokeCallback(typeof(GhostPredictionSmoothingSystem.SmoothingActionDelegate))]
    private static void SmoothingAction(void* currentData, void* previousData, void* usrData)
    {
        ref var trans = ref UnsafeUtility.AsRef<Translation>(currentData);
        ref var backup = ref UnsafeUtility.AsRef<Translation>(previousData);

        float maxDist = DefaultStaticUserParams.maxDist.Data;
        float delta = DefaultStaticUserParams.delta.Data;

        if (usrData != null)
        {
            ref var userParam = ref UnsafeUtility.AsRef<DefaultUserParams>(usrData);
            maxDist = userParam.maxDist;
            delta = userParam.delta;
        }

        var dist = math.distance(trans.Value, backup.Value);
        //UnityEngine.Debug.Log($"Custom smoothing, diff {trans.Value - backup.Value}, dist {dist}");
        if (dist > 0)
            trans.Value = backup.Value + (trans.Value - backup.Value) / dist;// (dist * 15);
        //UnityEngine.Debug.Log($"Smoothing trans, trans x: {trans.Value.x}, backup x: {backup.Value.x}");
        //Debug.Log("Smoothing trans:" + trans.Value.x + " backup:" + backup.Value.x);
        //var dist = math.distance(trans.Value, backup.Value);
        //if (dist < maxDist && dist > delta && dist > 0)
        //if (dist > 0)
        //{
        //    //Debug.Log("Smoothing :" + trans.Value.x);
        //    //int d = (int)(dist * 10000);
        //    //Debug.Log("TankSmoothing dist:" + dist);
        //    //Debug.Log("TankSmoothing old pos:" + trans.Value);
        //    trans.Value = backup.Value + (trans.Value - backup.Value) * delta / (dist * 100);
        //    //Debug.Log("TankSmoothing new pos:" + trans.Value);
        //    //trans.Value = backup.Value;
        //}
        //trans.Value = backup.Value;
    }
}