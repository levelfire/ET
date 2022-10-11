using Unity.Entities;

[GenerateAuthoringComponent]
public struct TankStacksCountComponent : IComponentData
{
    public int EmpStackCount;
}