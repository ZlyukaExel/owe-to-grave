using System;
using Mirror;
using UnityEngine.Events;

public class Inventory : NetworkBehaviour
{
    public readonly SyncList<InventoryItem> items = new();
    public UnityEvent<InventoryItem> onItemChanged;

    public void TakeIn(uint itemId) => TakeIn(itemId, 1);

    [Command(requiresAuthority = false)]
    public void TakeIn(uint itemId, int quantity)
    {
        if (quantity <= 0)
            return;

        // Increase if already exist
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemId.Equals(itemId))
            {
                var item = items[i];
                item.quantity += quantity;
                items[i] = item;
                return;
            }
        }

        // Add new if not exist
        items.Add(new InventoryItem { itemId = itemId, quantity = quantity });
    }

    [Command(requiresAuthority = false)]
    public void TakeIn(NetworkIdentity item)
    {
        TakeIn(item.assetId);
        NetworkServer.Destroy(item.gameObject);
    }

    [Command(requiresAuthority = false)]
    public void TakeOut(uint itemId, int quantity)
    {
        // Getting item
        int index = items.FindIndex(i => i.itemId == itemId);

        if (index == -1)
            return;

        // Just in case
        if (items[index].quantity < quantity)
            return;

        // Reduce quantity and spawn item
        var item = items[index];

        var itemData = ItemDataManager.Instance.GetItem(itemId);
        if (itemData.prefab)
        {
            var spawned = Instantiate(itemData.prefab, transform.position, transform.rotation); // TODO: severeal porefabs won't work
            NetworkServer.Spawn(spawned);
        }

        item.quantity -= quantity;
        if (item.quantity <= 0)
            items.RemoveAt(index);
        else
            items[index] = item;
    }

    public void TakeOut(uint itemId) => TakeOut(itemId, 1);

    public override void OnStartClient()
    {
        items.OnChange += OnItemsChanged;
    }

    private void OnItemsChanged(
        SyncList<InventoryItem>.Operation op,
        int itemIndex,
        InventoryItem oldItem
    )
    {
        InventoryItem updatedItem;

        switch (op)
        {
            case SyncList<InventoryItem>.Operation.OP_ADD:
            case SyncList<InventoryItem>.Operation.OP_INSERT:
            case SyncList<InventoryItem>.Operation.OP_SET:
                updatedItem = items[itemIndex];
                break;
            case SyncList<InventoryItem>.Operation.OP_REMOVEAT:
                updatedItem = oldItem;
                break;
            default:
                return;
        }

        onItemChanged.Invoke(updatedItem);
    }
}

[Serializable]
public struct InventoryItem : IEquatable<InventoryItem>
{
    public uint itemId;
    public int quantity;

    public InventoryItem(uint itemId, int quantity = 1)
    {
        this.itemId = itemId;
        this.quantity = quantity;
    }

    public readonly bool Equals(InventoryItem other) =>
        itemId == other.itemId && quantity == other.quantity;

    public override readonly bool Equals(object obj) => obj is InventoryItem other && Equals(other);

    public override readonly int GetHashCode() => itemId.GetHashCode();
}
