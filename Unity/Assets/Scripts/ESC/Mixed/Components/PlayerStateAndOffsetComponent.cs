using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

public struct PlayerStateAndOffsetComponent : IComponentData
{
    public float3 Value;
    public float3 RayBegin;
    public float3 RayEnd;
    [GhostField]
    public int State;
    public uint WeaponCooldown;

}