using Unity.Entities;

[GenerateAuthoringComponent]
public struct DropItemComponent : IComponentData
{
    public DropItemComponent(int uuid,int itemid)
    {
        Uuid = uuid;
        ItemId = itemid;
        age = 0;
        maxAge = 10;//TODO
    }

    public int Uuid;
    public int ItemId;
    public float age;
    public float maxAge;
}
