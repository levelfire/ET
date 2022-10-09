using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics.Stateful;
using Unity.Rendering;
using UnityEngine;

public struct TriggerVolumeChangeMaterial : IComponentData
{
    public Entity ReferenceEntity;
}

public class TriggerVolumeChangeMaterialAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject ReferenceGameObject = null;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new TriggerVolumeChangeMaterial
        {
            ReferenceEntity = ReferenceGameObject != null ? conversionSystem.GetPrimaryEntity(ReferenceGameObject) : Entity.Null
        });
    }
}
