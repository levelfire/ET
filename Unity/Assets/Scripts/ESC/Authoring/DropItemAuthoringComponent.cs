using Unity.Entities;

[GenerateAuthoringComponent]
public struct DropItemAuthoringComponent : IComponentData
{
    public Entity Prefab;
}