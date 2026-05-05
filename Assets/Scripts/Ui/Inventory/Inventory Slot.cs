using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : Selectable, IDropHandler
{
    public int slotIndex { private set; get; }
    private Inventory inventory;

    public void Initialize(Inventory inventory, int index)
    {
        this.inventory = inventory;
        slotIndex = index;
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventoryCell draggedItem = eventData.pointerDrag?.GetComponent<InventoryCell>();

        if (draggedItem == null || draggedItem.currentItem == null)
            return;

        int fromIndex = draggedItem.id;
        int toIndex = slotIndex;

        if (fromIndex < 0 || toIndex < 0 || fromIndex == toIndex)
            return;

        inventory.MoveItemBetweenSlots(fromIndex, toIndex);

        Selectable oldSlot = draggedItem.originalParent?.GetComponent<Selectable>();
        oldSlot?.OnPointerExit(new PointerEventData(EventSystem.current));
        Select();
    }
}

[System.Serializable]
public struct SlotDefinition
{
    public int maxQuantity;
    public ItemCategory allowedCategory;
    public ClothingType allowedClotherType;

    public SlotDefinition(
        int maxQuantity = 0,
        ItemCategory allowedCategory = ItemCategory.Any,
        ClothingType allowedClotherType = ClothingType.Any
    )
    {
        this.maxQuantity = maxQuantity;
        this.allowedCategory = allowedCategory;
        this.allowedClotherType = allowedClotherType;
    }

    public readonly bool CanAcceptItem(ItemData itemData, int quantity, int currentQuantity)
    {
        if (maxQuantity > 0 && currentQuantity + quantity > maxQuantity)
            return false;

        if (allowedCategory != ItemCategory.Any)
        {
            bool categoryMatches = allowedCategory switch
            {
                ItemCategory.Weapon => itemData is WeaponData,
                ItemCategory.Clother => itemData is ClothingData,
                ItemCategory.Misc => itemData is not WeaponData && itemData is not ClothingData,
                _ => true,
            };

            if (!categoryMatches)
                return false;
        }

        if (itemData is ClothingData clother && allowedClotherType != ClothingType.Any)
        {
            if (clother.clothingType != allowedClotherType)
                return false;
        }

        return true;
    }
}

public enum ItemCategory
{
    Any,
    Misc,
    Weapon,
    Clother,
}
