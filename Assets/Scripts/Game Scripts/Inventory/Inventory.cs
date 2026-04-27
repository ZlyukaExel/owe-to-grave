using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : NetworkBehaviour
{
    public int inventorySize = 64;
    public List<SlotDefinition> slotDefinitions = new();
    public int equipmentSlots { private set; get; }
    public readonly SyncList<InventoryItem> items = new();
    public UnityEvent<int, InventoryItem> onItemChanged;
    private Transform targetDirection;

    public override void OnStartServer()
    {
        equipmentSlots = slotDefinitions.Count;

        // Filling list with empty
        for (int i = 0; i < equipmentSlots + inventorySize; i++)
        {
            items.Add(default); // Instead of null
        }

        // Equipment slots are filled in inspector
        // Filling other slot definitions
        for (int i = 0; i < inventorySize; i++)
        {
            slotDefinitions.Add(new SlotDefinition());
        }
    }

    private int GetItemInventoryIndex(uint itemId, int quantity)
    {
        ItemData itemData = ItemDataManager.Instance.GetItem(itemId);

        // Increase if already exist
        for (int i = equipmentSlots; i < inventorySize; i++)
        {
            var item = items[i];
            var def = slotDefinitions[i];
            if (item.itemId == itemId && def.CanAcceptItem(itemData.type, quantity, item.quantity))
                return i;
        }

        // Get empty if no such item yet
        return GetEmptyIndex(itemId, quantity);
    }

    private int GetEmptyIndex(uint itemId, int quantity)
    {
        ItemData itemData = ItemDataManager.Instance.GetItem(itemId);

        // Increase if already exist
        for (int i = equipmentSlots; i < inventorySize; i++)
        {
            var item = items[i];
            var def = slotDefinitions[i];
            if (item.itemId == 0 && def.CanAcceptItem(itemData.type, quantity, item.quantity))
                return i;
        }

        return -1;
    }

    public bool HasSpace(uint itemId, int quantity) => GetEmptyIndex(itemId, quantity) != -1;

    [Command(requiresAuthority = false)]
    public void TakeIn(uint itemId, int quantity)
    {
        int itemIndex = GetItemInventoryIndex(itemId, quantity);
        if (itemIndex == -1)
            return;

        int itemQuantity = items[itemIndex].quantity;
        items[itemIndex] = new InventoryItem(itemId, itemQuantity + quantity);
    }

    [Command(requiresAuthority = false)]
    public void TakeIn(NetworkIdentity item)
    {
        if (item == null)
            return;

        int itemIndex = GetItemInventoryIndex(item.assetId, 1);
        if (itemIndex == -1) // Not enough space
            return;

        // Increase quantity and update
        int itemQuantity = items[itemIndex].quantity;
        items[itemIndex] = new InventoryItem(item.assetId, itemQuantity + 1);

        // Destroy object
        NetworkServer.Destroy(item.gameObject);
    }

    [Command(requiresAuthority = false)]
    public void TakeOut(int slotIndex, int quantity)
    {
        if (slotIndex < 0 || slotIndex >= items.Count)
            return;

        if (quantity <= 0)
            return;

        var slotItem = items[slotIndex];

        if (slotItem.itemId == 0)
            return;

        int actualQuantity = Mathf.Min(quantity, slotItem.quantity);

        if (slotItem.quantity <= actualQuantity)
        {
            items[slotIndex] = new InventoryItem(0, 0);
        }
        else
        {
            items[slotIndex] = new InventoryItem(
                slotItem.itemId,
                slotItem.quantity - actualQuantity
            );
        }

        SpawnItemPrefabs(slotItem.itemId, actualQuantity);
    }

    [Command(requiresAuthority = false)]
    public void UnstackItemFromSlot(int fromSlotIndex, int quantity)
    {
        if (fromSlotIndex < 0 || fromSlotIndex >= items.Count)
            return;
        if (quantity <= 0)
            return;

        var sourceItem = items[fromSlotIndex];
        if (sourceItem.itemId == 0 || sourceItem.quantity < quantity)
            return;

        int emptyIndex = GetEmptyIndex(sourceItem.itemId, quantity);
        if (emptyIndex == -1)
            return;

        // If unstack all, clear cell
        if (sourceItem.quantity == quantity)
        {
            items[fromSlotIndex] = new InventoryItem(0, 0);
        }
        // If unstack part, decrease quantity
        else
        {
            items[fromSlotIndex] = new InventoryItem(
                sourceItem.itemId,
                sourceItem.quantity - quantity
            );
        }

        // Moving items into an empty cell
        items[emptyIndex] = new InventoryItem(sourceItem.itemId, quantity);
    }

    [Command(requiresAuthority = false)]
    public void MoveItemBetweenSlots(int fromSlotIndex, int toSlotIndex)
    {
        if (
            fromSlotIndex < 0
            || fromSlotIndex >= items.Count
            || toSlotIndex < 0
            || toSlotIndex >= items.Count
        )
            return;

        if (fromSlotIndex == toSlotIndex)
            return;

        var fromItem = items[fromSlotIndex];
        var toItem = items[toSlotIndex];
        var toSlotDef = slotDefinitions[toSlotIndex];

        if (fromItem.itemId == 0)
            return;

        ItemData fromItemData = ItemDataManager.Instance.GetItem(fromItem.itemId);

        // Stack items
        if (toItem.itemId == fromItem.itemId && toItem.itemId != 0)
        {
            if (toSlotDef.CanAcceptItem(fromItemData.type, fromItem.quantity, toItem.quantity))
            {
                int newQuantity = toItem.quantity + fromItem.quantity;
                items[toSlotIndex] = new InventoryItem(toItem.itemId, newQuantity);
                items[fromSlotIndex] = new InventoryItem(0, 0);
            }
            return;
        }

        // Swap items
        if (toItem.itemId != 0)
        {
            ItemData toItemData = ItemDataManager.Instance.GetItem(toItem.itemId);
            var fromSlotDef = slotDefinitions[fromSlotIndex];

            if (
                toItemData != null
                && fromItemData != null
                && fromSlotDef.CanAcceptItem(toItemData.type, toItem.quantity, 0)
                && toSlotDef.CanAcceptItem(fromItemData.type, fromItem.quantity, 0)
            )
            {
                items[fromSlotIndex] = new InventoryItem(toItem.itemId, toItem.quantity);
                items[toSlotIndex] = new InventoryItem(fromItem.itemId, fromItem.quantity);
            }
            return;
        }

        // Move to empty
        if (
            fromItemData != null
            && toSlotDef.CanAcceptItem(fromItemData.type, fromItem.quantity, 0)
        )
        {
            items[toSlotIndex] = new InventoryItem(fromItem.itemId, fromItem.quantity);
            items[fromSlotIndex] = new InventoryItem(0, 0);
        }
    }

    private void SpawnItemPrefabs(uint itemId, int quantity)
    {
        var itemData = ItemDataManager.Instance.GetItem(itemId);
        if (itemData != null && itemData.prefab != null)
        {
            for (int i = 0; i < quantity; i++)
            {
                var spawned = Instantiate(
                    itemData.prefab,
                    targetDirection.position + targetDirection.forward * 1.5f,
                    targetDirection.rotation
                );
                NetworkServer.Spawn(spawned);
            }
        }
    }

    public int GetItemQuantity(uint itemId)
    {
        int total = 0;
        foreach (var item in items)
        {
            if (item.itemId == itemId && item.quantity > 0)
                total += item.quantity;
        }
        return total;
    }

    public override void OnStartClient()
    {
        items.OnChange += OnItemsChanged;
    }

    public void SetTarget(Transform target)
    {
        targetDirection = target;
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
                // If item was deleted
                if (items[itemIndex].itemId == 0)
                {
                    updatedItem = oldItem;
                    updatedItem.quantity = 0;
                }
                // If item was updated / added
                else
                    updatedItem = items[itemIndex];
                break;
            case SyncList<InventoryItem>.Operation.OP_REMOVEAT:
                updatedItem = oldItem;
                updatedItem.quantity = 0;
                break;
            default:
                return;
        }

        onItemChanged.Invoke(itemIndex, updatedItem);
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

    public override readonly string ToString() => $"Inventory item {itemId}, quantity = {quantity}";
}
