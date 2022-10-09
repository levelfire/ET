using Unity.Entities;

[GenerateAuthoringComponent]
public struct BlockAuthoringComponent : IComponentData
{
    public Entity Prefab;
}
