using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : Selectable, IDropHandler, ISelectHandler
{
    public int slotIndex { private set; get; }
    private Inventory inventory;
    private InventoryCell cell;

    public void Initialize(Inventory inventory, int index)
    {
        this.inventory = inventory;
        slotIndex = index;
        cell = GetComponentInChildren<InventoryCell>(true);
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventoryCell draggedItem = eventData.pointerDrag?.GetComponent<InventoryCell>();

        if (draggedItem == null || draggedItem.currentItem == null)
            return;

        int fromIndex = draggedItem.slotIndex;
        int toIndex = slotIndex;

        if (fromIndex < 0 || toIndex < 0 || fromIndex == toIndex)
            return;

        inventory.MoveItemBetweenSlots(fromIndex, toIndex);

        Selectable oldSlot = draggedItem.originalParent?.GetComponent<Selectable>();
        oldSlot?.OnPointerExit(new PointerEventData(EventSystem.current));
        Select();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        cell.OnSelect();
    }
}

[System.Serializable]
public struct SlotDefinition
{
    public int maxQuantity;
    public ItemType allowedType;

    public SlotDefinition(int maxQuantity = 0, ItemType allowedType = ItemType.Any)
    {
        this.maxQuantity = maxQuantity;
        this.allowedType = allowedType;
    }

    public readonly bool CanAcceptItem(ItemType itemType, int quantity, int currentQuantity)
    {
        if (allowedType != ItemType.Any && itemType != allowedType)
            return false;

        if (maxQuantity > 0 && currentQuantity + quantity > maxQuantity)
            return false;

        return true;
    }
}
