using Unity.Entities;

[GenerateAuthoringComponent]
public struct EmpAuthoringComponent : IComponentData
{
    public Entity Prefab;
}