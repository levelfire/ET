using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using System;
using Unity.Assertions;
using Unity.Mathematics;

namespace Unity.Physics.Stateful
{
    public struct StatefulTriggerEventExclude : IComponentData {}

    public class StatefulTriggerEventBufferAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<StatefulTriggerEvent>(entity);
        }
    }
}
