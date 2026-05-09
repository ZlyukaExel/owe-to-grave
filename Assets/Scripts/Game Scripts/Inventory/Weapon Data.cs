using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Inventory/Weapon Data")]
public class WeaponData : ItemData
{
    [Header("Weapon Data")]
    public WeaponProperties properties;

    public override string GetItemTypeString() => "Weapon";
}
