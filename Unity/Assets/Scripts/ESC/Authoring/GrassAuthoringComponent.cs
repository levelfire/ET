using Unity.Entities;

[GenerateAuthoringComponent]
public struct GrassAuthoringComponent : IComponentData
{
    public Entity Prefab;
}
