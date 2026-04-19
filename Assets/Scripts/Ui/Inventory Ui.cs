using UnityEngine;

public class InventoryUi : MonoBehaviour
{
    private Inventory inventory;

    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;
    }

    void OnEnable()
    {
        if (!inventory)
        {
            Debug.LogError("Inventory is not set to Inventory Ui");
            return;
        }

        // Add all icons
        foreach (var itemId in inventory.items)
        {
            AddIcon(itemId);
        }

        // Listeners for new items
        inventory.onItemAdded.AddListener(AddIcon);
        inventory.onItemAdded.AddListener(DeleteIcon);
    }

    void OnDisable()
    {
        if (!inventory)
        {
            Debug.LogError("Inventory is not set to Inventory Ui");
            return;
        }

        // Remove all icons
        foreach (var itemId in inventory.items)
        {
            DeleteIcon(itemId);
        }

        // Listeners for new items
        inventory.onItemAdded.RemoveListener(AddIcon);
        inventory.onItemAdded.RemoveListener(DeleteIcon);
    }

    private void AddIcon(uint itemId)
    {
        // TODO: add icons
        Debug.Log("Adding object: " + ItemDataManager.Instance.GetItem(itemId).itemName);
    }

    private void DeleteIcon(uint itemId)
    {
        // TODO: remove icons
        Debug.Log("Removing object: " + ItemDataManager.Instance.GetItem(itemId).itemName);
    }
}
