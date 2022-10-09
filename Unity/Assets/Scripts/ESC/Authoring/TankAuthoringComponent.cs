using Unity.Entities;

[GenerateAuthoringComponent]
public struct TankAuthoringComponent : IComponentData
{
    public Entity Prefab;
}
