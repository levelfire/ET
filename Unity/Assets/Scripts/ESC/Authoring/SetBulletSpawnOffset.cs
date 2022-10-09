using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SetBulletSpawnOffset : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject bulletSpawn;
    public GameObject rayBegin;
    public GameObject rayEnd;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var bulletOffset = default(PlayerStateAndOffsetComponent);

        var offsetVector = bulletSpawn.transform.position;
        bulletOffset.Value = new float3(offsetVector.x, offsetVector.y, offsetVector.z);

        var offsetRayBegin = rayBegin.transform.position;
        bulletOffset.RayBegin = new float3(offsetRayBegin.x, offsetRayBegin.y, offsetRayBegin.z);
        var offsetRayEnd = rayEnd.transform.position;
        bulletOffset.RayEnd = new float3(offsetRayEnd.x, offsetRayEnd.y, offsetRayEnd.z);

        dstManager.AddComponentData(entity, bulletOffset);
    }
}
