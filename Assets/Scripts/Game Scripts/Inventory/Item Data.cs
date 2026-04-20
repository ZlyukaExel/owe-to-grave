using Mirror;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public uint id;
    public string itemName;
    public ItemType type;

    [TextArea]
    public string description;
    public int price = 1;
    public Sprite icon;
    public GameObject prefab;

    private void OnValidate()
    {
        if (prefab)
            id = prefab.GetComponent<NetworkIdentity>().assetId;
    }
}

public enum ItemType
{
    Weapon,
    Clother,
    Misc,
}
