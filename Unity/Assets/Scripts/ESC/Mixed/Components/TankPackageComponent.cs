using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct TankPackageComponent : IComponentData
{
    //[GhostField]
    //private int eWeapon;
    //[GhostField]
    //private int eArmor;
    //[GhostField]
    //private int ePassive;
    //[GhostField]
    //private int eActiveA;
    //[GhostField]
    //private int eActiveB;

    ////TODO cfg?
    //const int bulletid_comm = 1;
    //const int weapon_default_level = 1;
    //const int slot_index_0 = 0;
    //const int itemid_reborn = 500001;
    //const int equip_act1_index = 0;
    //const int equip_act2_index = 1;
    //const int equip_passive_index = 2;

    //public Dictionary<int, float> DisId = new Dictionary<int, float>();

    ////key itemid
    //Dictionary<int, PackageItem> items = new Dictionary<int, PackageItem>();
    ////key uuid
    //Dictionary<int, PackageItem> itemsPick = new Dictionary<int, PackageItem>();

    //Dictionary<EnumItemPackageType, int> item_counts = new Dictionary<EnumItemPackageType, int> {
    //    {EnumItemPackageType.Weapon,0},
    //    {EnumItemPackageType.Armor,0},
    //    {EnumItemPackageType.Active,0},
    //    {EnumItemPackageType.Passive,0},
    //    {EnumItemPackageType.Auto,0},
    //};

}