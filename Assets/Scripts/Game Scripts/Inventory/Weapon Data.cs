using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Inventory/Weapon Data")]
public class WeaponData : ItemData
{
    [Header("Weapon Data")]
    public WeaponProperties properties;

    public override string GetItemTypeString() => "Weapon";
}

[System.Serializable]
public class WeaponProperties
{
    public float damage = 10,
        bulletSpeed = 30,
        spread = 1;
    public int piercing = 0;
    public bool isMelee;
}
