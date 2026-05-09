using UnityEngine;

[CreateAssetMenu(fileName = "New Clother Data", menuName = "Inventory/Clother Data")]
public class ClothingData : ItemData
{
    [Header("Clother Data")]
    public ClothingType clothingType = ClothingType.Any;
    public int damageReduction = 0;

    public override string GetItemTypeString() => clothingType.ToString();
}

public enum ClothingType
{
    Any,
    Hat,
    Mask,
    Top,
    Gloves,
    Shoes,
    Pants,
}
