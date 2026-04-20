using UnityEngine;

public class InventoryUi : MonoBehaviour
{
    private Inventory inventory;
    private InventoryCell[] inventoryCells;

    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;

        inventoryCells = GetComponentsInChildren<InventoryCell>(true);

        // Listeners for new items
        inventory.onItemChanged.AddListener(AddItem);

        // Add all icons
        for (int i = 0; i < inventory.items.Count; i++)
        {
            AddItem(inventory.items[i]);
        }
    }

    void OnDestroy()
    {
        if (inventory)
        {
            inventory.onItemChanged.RemoveListener(AddItem);
        }
    }

    private void AddItem(InventoryItem item)
    {
        if (GetInventoryCell(item.itemId) is InventoryCell cell)
        {
            cell.SetItem(item);
            return;
        }

        foreach (var loopCell in inventoryCells)
        {
            if (loopCell.currentItem == null)
            {
                loopCell.SetItem(item);
                return;
            }
        }
    }

    private InventoryCell GetInventoryCell(uint itemId)
    {
        foreach (var cell in inventoryCells)
        {
            if (cell.currentItem && cell.currentItem.id == itemId)
            {
                return cell;
            }
        }

        return null;
    }
}
