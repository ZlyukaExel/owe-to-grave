using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterConfigManager))]
[RequireComponent(typeof(NetworkCharacterConfig))]
public class Inventory : NetworkBehaviour
{
    public int inventorySize = 64;
    public List<SlotDefinition> slotDefinitions = new();
    public int equipmentSlots { private set; get; }
    public readonly SyncList<InventoryItem> items = new();
    public UnityEvent<int, InventoryItem> onItemChanged;
    private Transform targetDirection;
    private CharacterConfigManager characterConfig;
    public List<InventoryItem> initialItems = new();

    public override void OnStartServer()
    {
        characterConfig = GetComponent<CharacterConfigManager>();

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

        // Filling initial inventory
        for (int i = 0; i < initialItems.Count; i++)
        {
            items[i] = initialItems[i];
            OnEquipmentChanged(items[i], i);
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

            if (item.itemId == itemId)
            {
                if (def.GetAcceptableQuantity(itemData, quantity, item.quantity) == quantity)
                    return i;
            }
        }

        // Get empty if no such item yet
        return GetEmptyIndex(itemId, quantity);
    }

    public int GetEmptyIndex(uint itemId, int quantity)
    {
        ItemData itemData = ItemDataManager.Instance.GetItem(itemId);

        // Increase if already exist
        for (int i = equipmentSlots; i < inventorySize; i++)
        {
            var item = items[i];
            var def = slotDefinitions[i];

            if (item.itemId == 0)
            {
                if (def.GetAcceptableQuantity(itemData, quantity, 0) == quantity)
                    return i;
            }
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
        {
            Debug.Log("Not enough space in the inventory");
            return;
        }

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
            int amountToAdd = toSlotDef.GetAcceptableQuantity(
                fromItemData,
                fromItem.quantity,
                toItem.quantity
            );

            if (amountToAdd > 0)
            {
                if (amountToAdd == fromItem.quantity)
                    items[fromSlotIndex] = default;
                else
                    items[fromSlotIndex] = new InventoryItem(
                        fromItem.itemId,
                        fromItem.quantity - amountToAdd
                    );

                items[toSlotIndex] = new InventoryItem(
                    toItem.itemId,
                    toItem.quantity + amountToAdd
                );
            }
            return;
        }

        // Swap items
        if (toItem.itemId != 0)
        {
            ItemData toItemData = ItemDataManager.Instance.GetItem(toItem.itemId);
            var fromSlotDef = slotDefinitions[fromSlotIndex];

            if (toItemData != null && fromItemData != null)
            {
                int canTakeTo = toSlotDef.GetAcceptableQuantity(fromItemData, fromItem.quantity, 0);
                int canTakeFrom = fromSlotDef.GetAcceptableQuantity(toItemData, toItem.quantity, 0);

                if (canTakeTo == fromItem.quantity && canTakeFrom == toItem.quantity)
                {
                    items[fromSlotIndex] = new InventoryItem(toItem.itemId, toItem.quantity);
                    items[toSlotIndex] = new InventoryItem(fromItem.itemId, fromItem.quantity);
                }
            }
            return;
        }

        // Move to empty
        if (fromItemData != null)
        {
            int acceptableQuantity = toSlotDef.GetAcceptableQuantity(
                fromItemData,
                fromItem.quantity,
                0
            );

            if (acceptableQuantity > 0)
            {
                if (acceptableQuantity == fromItem.quantity)
                {
                    items[fromSlotIndex] = default;
                }
                else
                {
                    items[fromSlotIndex] = new InventoryItem(
                        fromItem.itemId,
                        fromItem.quantity - acceptableQuantity
                    );
                }

                items[toSlotIndex] = new InventoryItem(fromItem.itemId, acceptableQuantity);
            }
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

    public int EquipClothing(int slotIndex)
    {
        InventoryItem item = items[slotIndex];
        if (item.itemId == 0 || item.quantity < 1)
            return -1;

        ItemData itemData = ItemDataManager.Instance.GetItem(item.itemId);
        ClothingData clothingData = itemData as ClothingData;

        if (clothingData == null)
            return -1;

        switch (clothingData.clothingType)
        {
            case ClothingType.Hat:
                MoveItemBetweenSlots(slotIndex, 2);
                return 2;
            case ClothingType.Top:
                MoveItemBetweenSlots(slotIndex, 3);
                return 3;
            case ClothingType.Pants:
                MoveItemBetweenSlots(slotIndex, 4);
                return 4;
            case ClothingType.Mask:
                MoveItemBetweenSlots(slotIndex, 5);
                return 5;
            case ClothingType.Gloves:
                MoveItemBetweenSlots(slotIndex, 6);
                return 6;
            case ClothingType.Shoes:
                MoveItemBetweenSlots(slotIndex, 7);
                return 7;
            default:
                return -1;
        }
    }

    public int EquipWeapon(int slotIndex, bool primary = true)
    {
        InventoryItem item = items[slotIndex];
        if (item.itemId == 0 || item.quantity < 1)
            return -1;

        ItemData itemData = ItemDataManager.Instance.GetItem(item.itemId);
        if (itemData is not WeaponData)
            return -1;

        int moveToSlotIndex = primary ? 0 : 1;
        MoveItemBetweenSlots(slotIndex, moveToSlotIndex);
        return moveToSlotIndex;
    }

    public int Unequip(int slotIndex)
    {
        if (!(slotIndex >= 0 && slotIndex < equipmentSlots))
            return -1;

        InventoryItem item = items[slotIndex];
        if (item.itemId == 0 || item.quantity < 1 || !HasSpace(item.itemId, item.quantity))
            return -1;

        int movedTo = GetItemInventoryIndex(item.itemId, item.quantity);
        TakeIn(item.itemId, item.quantity);
        items[slotIndex] = default;
        return movedTo;
    }

    [Command(requiresAuthority = false)]
    public void SwapWeapons()
    {
        if (items.Count <= 1)
            return;

        (items[1], items[0]) = (items[0], items[1]);
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

        OnEquipmentChanged(updatedItem, itemIndex);
    }

    private void OnEquipmentChanged(InventoryItem item, int inventoryIndex)
    {
        if (inventoryIndex < 0 || inventoryIndex > 7)
            return;

        switch (inventoryIndex)
        {
            case 0:
                characterConfig.EquipWeapon(item, true);
                break;
            case 1:
                characterConfig.EquipWeapon(item, false);
                break;
            case 2:
                characterConfig.EquipClothing(item, ClothingType.Hat);
                break;
            case 3:
                characterConfig.EquipClothing(item, ClothingType.Top);
                break;
            case 4:
                characterConfig.EquipClothing(item, ClothingType.Pants);
                break;
            case 5:
                characterConfig.EquipClothing(item, ClothingType.Mask);
                break;
            case 6:
                characterConfig.EquipClothing(item, ClothingType.Gloves);
                break;
            case 7:
                characterConfig.EquipClothing(item, ClothingType.Shoes);
                break;
        }
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
