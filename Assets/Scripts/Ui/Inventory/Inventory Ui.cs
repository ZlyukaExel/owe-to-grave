using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryUi : MonoBehaviour
{
    private Inventory inventory;

    [SerializeField]
    private GameObject cellPrefab;

    [SerializeField]
    private Transform cellContainer;

    [SerializeField]
    private InventoryCell[] inventoryCells;

    [SerializeField]
    private InputActionReference dropAction,
        unstackAction,
        dragAction;

    public bool isKeyboardDragging = false;
    public int dragSourceSlotIndex = -1;

    public void SetInventory(Inventory inventory)
    {
        if (this.inventory != null)
            this.inventory.onItemChanged.RemoveListener(UpdateItem);

        this.inventory = inventory;

        // Clear whole inventory cells in container
        // Except first slot (since it holds links)
        for (int i = cellContainer.childCount - 1; i > 0; i--)
        {
            var cellToDelete = cellContainer.GetChild(i);
            cellToDelete.SetParent(null);
            Destroy(cellToDelete.gameObject);
        }

        // Setup equipment and first slots
        InventorySlot[] fixedSlots = GetComponentsInChildren<InventorySlot>();
        if (fixedSlots.Length != inventory.equipmentSlots + 1)
            throw new System.Exception("Ui equipment slots don't match with Inventory");
        for (int i = 0; i < fixedSlots.Length; i++)
        {
            SetupSlot(fixedSlots[i].transform, i);
        }

        // Creating new inventory slots
        for (
            int i = inventory.equipmentSlots + 1;
            i < inventory.inventorySize + inventory.equipmentSlots;
            i++
        )
        {
            GameObject cellObj = Instantiate(cellPrefab, cellContainer);
            SetupSlot(cellObj.transform, i);
        }
        inventoryCells = GetComponentsInChildren<InventoryCell>(true);
        if (inventoryCells.Length != inventory.equipmentSlots + inventory.inventorySize)
            throw new System.Exception("Inventory size not matches with Ui");

        inventory.onItemChanged.AddListener(UpdateItem);

        // Filling slots with inventory
        for (int i = 0; i < inventory.inventorySize; i++)
        {
            UpdateItem(i, inventory.items[i]);
        }
    }

    private void SetupSlot(Transform slotTrans, int index)
    {
        var slot = slotTrans.GetComponent<InventorySlot>();
        slot.Initialize(inventory, index);

        var cell = slotTrans.GetComponentInChildren<InventoryCell>(true);
        cell.SetSlotIndex(this, index);
    }

    private void UpdateItem(int itemIndex, InventoryItem item)
    {
        if (inventoryCells == null || itemIndex >= inventoryCells.Length)
            return;

        inventoryCells[itemIndex].SetItem(item);
    }

    private void OnDropActionPressed(InputAction.CallbackContext ctx)
    {
        var selected = GetSelectedCell();
        if (selected != null && selected.currentItem != null && selected.slotIndex >= 0)
        {
            inventory.TakeOut(selected.slotIndex, 1);
        }
    }

    private void OnUnstackActionPressed(InputAction.CallbackContext ctx)
    {
        var selected = GetSelectedCell();
        if (selected == null || selected.currentItem == null)
            return;

        if (
            selected.quantity > 1
            && selected.slotIndex >= 0
            && inventory.HasSpace(selected.currentItem.id, 1)
        )
        {
            inventory.UnstackItemFromSlot(selected.slotIndex, 1);
        }
    }

    private void OnDragActionPressed(InputAction.CallbackContext ctx)
    {
        var selected = GetSelectedCell();
        if (selected == null || selected.currentItem == null)
            return;

        isKeyboardDragging = true;
        dragSourceSlotIndex = selected.slotIndex;

        selected.StartKeyboardDrag();
    }

    private void OnDragActionReleased(InputAction.CallbackContext ctx)
    {
        if (!isKeyboardDragging)
            return;

        isKeyboardDragging = false;

        GameObject selectedObj = EventSystem.current?.currentSelectedGameObject;
        InventorySlot targetSlot = selectedObj.GetComponent<InventorySlot>();

        if (targetSlot != null && targetSlot.slotIndex >= 0)
        {
            int targetIndex = targetSlot.slotIndex;

            if (dragSourceSlotIndex >= 0 && dragSourceSlotIndex != targetIndex)
            {
                inventory.MoveItemBetweenSlots(dragSourceSlotIndex, targetIndex);

                targetSlot.Select();
            }
        }

        if (
            dragSourceSlotIndex >= 0
            && inventoryCells != null
            && dragSourceSlotIndex < inventoryCells.Length
        )
        {
            inventoryCells[dragSourceSlotIndex].EndKeyboardDrag();
        }

        dragSourceSlotIndex = -1;
    }

    public InventoryCell GetSelectedCell()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        return selected != null ? selected.GetComponentInChildren<InventoryCell>() : null;
    }

    void OnEnable()
    {
        dropAction.action.performed += OnDropActionPressed;
        unstackAction.action.performed += OnUnstackActionPressed;
        dragAction.action.performed += OnDragActionPressed;
        dragAction.action.canceled += OnDragActionReleased;
    }

    void OnDisable()
    {
        dropAction.action.performed -= OnDropActionPressed;
        unstackAction.action.performed -= OnUnstackActionPressed;
        dragAction.action.performed -= OnDragActionPressed;
        dragAction.action.canceled -= OnDragActionReleased;
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.onItemChanged.RemoveListener(UpdateItem);
    }

    public void MoveCurrentCell(InventoryCell moveTo)
    {
        if (!isKeyboardDragging)
            return;

        InventoryCell currentCell = inventoryCells[dragSourceSlotIndex];

        // If move to start position, reset position
        if (currentCell.rectTransform == moveTo.rectTransform)
        {
            currentCell.rectTransform.position = moveTo.originalParent.position;
            Vector2 sourceSize = moveTo.rectTransform.rect.size;
            currentCell.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                sourceSize.x
            );
            currentCell.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                sourceSize.y
            );
        }
        // Move to other cell's position
        else
        {
            currentCell.rectTransform.position = moveTo.rectTransform.position;
            Vector2 sourceSize = moveTo.rectTransform.rect.size;
            currentCell.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                sourceSize.x
            );
            currentCell.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                sourceSize.y
            );
        }
    }
}
