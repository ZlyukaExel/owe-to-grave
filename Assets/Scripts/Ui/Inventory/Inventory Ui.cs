using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUi : MonoBehaviour
{
    private Inventory inventory;

    [SerializeField]
    private Transform cellContainer;
    private RectTransform popup;

    [SerializeField]
    private InventoryCell[] inventoryCells;

    [SerializeField]
    private InputActionReference dropAction,
        unstackAction,
        dragAction,
        equipPrimaryAction,
        equipSecondaryAction;

    public bool isKeyboardDragging = false;
    public int dragSourceSlotIndex = -1;

    public GameObject selectedCell;

    [Header("Filtering")]
    public ItemCategory filterCategory = ItemCategory.Any;
    public float activeFilterAlpha = 1,
        inactiveFilterAlpha = 0.6f;

    public void SetInventory(Inventory inventory)
    {
        if (this.inventory != null)
            this.inventory.onItemChanged.RemoveListener(UpdateItem);

        this.inventory = inventory;

        // Setup inventory
        inventoryCells = GetComponentsInChildren<InventoryCell>(true);
        if (inventoryCells.Length != inventory.equipmentSlots + inventory.inventorySize)
            throw new System.Exception(
                $"Inventory size not matches with Ui: {inventoryCells.Length}, {inventory.equipmentSlots + inventory.inventorySize}"
            );
        for (int i = 0; i < inventoryCells.Length; i++)
        {
            SetupSlot(inventoryCells[i].transform.parent, i);
        }

        inventory.onItemChanged.AddListener(UpdateItem);

        // Filling slots with inventory
        for (int i = 0; i < inventory.inventorySize; i++)
        {
            UpdateItem(i, inventory.items[i]);
        }

        popup = transform.parent.parent.Find("Popup").GetComponent<RectTransform>();
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
        if (selected != null && selected.currentItem != null && selected.id >= 0)
        {
            inventory.TakeOut(selected.id, 1);
        }
    }

    private void OnUnstackActionPressed(InputAction.CallbackContext ctx) =>
        OnUnstackActionPressed();

    public void OnUnstackActionPressed()
    {
        var selected = GetSelectedCell();
        if (selected == null || selected.currentItem == null)
            return;

        if (
            selected.quantity > 1
            && selected.id >= 0
            && inventory.HasSpace(selected.currentItem.id, 1)
        )
        {
            inventory.UnstackItemFromSlot(selected.id, 1);
        }
    }

    private void OnDragActionPressed(InputAction.CallbackContext ctx)
    {
        var selected = GetSelectedCell();
        if (selected == null || selected.currentItem == null)
            return;

        isKeyboardDragging = true;
        dragSourceSlotIndex = selected.id;

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

    private void OnEquipPrimaryActionPressed(InputAction.CallbackContext ctx)
    {
        OnEquipActionPressed();
    }

    private void OnEquipSecondaryActionPressed(InputAction.CallbackContext ctx)
    {
        OnEquipActionPressed(false);
    }

    private void OnEquipActionPressed(bool primary = true)
    {
        var selected = GetSelectedCell();
        if (selected == null || selected.currentItem == null || !selected.currentItem.isEquipable)
            return;

        int movedToIndex;

        // Unequip
        if (selected.id >= 0 && selected.id < inventory.equipmentSlots)
        {
            movedToIndex = inventory.Unequip(selected.id);
        }
        // Equip weapon
        else if (selected.currentItem is WeaponData)
        {
            movedToIndex = inventory.EquipWeapon(selected.id, primary);
        }
        // Equip clothing
        else
        {
            movedToIndex = inventory.EquipClothing(selected.id);
        }

        if (movedToIndex >= 0)
        {
            inventoryCells[movedToIndex].GetComponentInParent<InventorySlot>().Select();
        }
    }

    public InventoryCell GetSelectedCell() =>
        selectedCell ? selectedCell.GetComponentInChildren<InventoryCell>() : null;

    void OnEnable()
    {
        dropAction.action.performed += OnDropActionPressed;
        unstackAction.action.performed += OnUnstackActionPressed;
        dragAction.action.performed += OnDragActionPressed;
        dragAction.action.canceled += OnDragActionReleased;
        equipPrimaryAction.action.performed += OnEquipPrimaryActionPressed;
        equipSecondaryAction.action.performed += OnEquipSecondaryActionPressed;
    }

    void OnDisable()
    {
        dropAction.action.performed -= OnDropActionPressed;
        unstackAction.action.performed -= OnUnstackActionPressed;
        dragAction.action.performed -= OnDragActionPressed;
        dragAction.action.canceled -= OnDragActionReleased;
        equipPrimaryAction.action.performed -= OnEquipPrimaryActionPressed;
        equipSecondaryAction.action.performed -= OnEquipSecondaryActionPressed;
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.onItemChanged.RemoveListener(UpdateItem);
    }

    public void OnCellSelected(InventoryCell newCell)
    {
        StopDeselection();
        UpdatePopup();

        if (!isKeyboardDragging)
            return;

        InventoryCell currentCell = inventoryCells[dragSourceSlotIndex];

        // If move to start position, reset position
        if (currentCell.rectTransform == newCell.rectTransform)
        {
            currentCell.rectTransform.position = newCell.originalParent.position;
            Vector2 sourceSize = newCell.rectTransform.rect.size;
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
            currentCell.rectTransform.position = newCell.rectTransform.position;
            Vector2 sourceSize = newCell.rectTransform.rect.size;
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

    private void MovePopup(InventoryCell moveTo)
    {
        float popupWidth = popup.rect.width;
        float popupHeight = popup.rect.height;
        float cellWidth = moveTo.rectTransform.rect.width;
        float cellHeight = moveTo.rectTransform.rect.height;

        popup.position = moveTo.transform.position;
        if (moveTo.transform.position.x > Screen.width / 2)
            popup.anchoredPosition -= new Vector2(popupWidth / 2 + cellWidth / 2, 0);
        else
            popup.anchoredPosition += new Vector2(popupWidth / 2 + cellWidth / 2, 0);

        if (moveTo.transform.position.y > Screen.height / 2)
            popup.anchoredPosition += new Vector2(0, -popupHeight / 2 + cellHeight / 2);
        else
            popup.anchoredPosition -= new Vector2(0, -popupHeight / 2 + cellHeight / 2);
    }

    public void UpdatePopup()
    {
        if (isKeyboardDragging || !gameObject.activeInHierarchy)
            return;

        // If no cell selected
        InventoryCell cell = GetSelectedCell();
        if (!cell || !popup)
        {
            popup.gameObject.SetActive(false);
            return;
        }

        // If selected cell is empty
        ItemData itemData = cell.currentItem;
        popup.gameObject.SetActive(itemData);
        if (!itemData)
            return;

        popup.Find("Name").GetComponent<TMP_Text>().text = itemData.itemName;
        popup.Find("Description").GetComponent<TMP_Text>().text = itemData.description;
        popup.Find("Type/Value").GetComponent<TMP_Text>().text = itemData.GetItemTypeString();
        popup.Find("Price/Value").GetComponent<TMP_Text>().text = itemData.price + "$";
        popup.Find("Id/Value").GetComponent<TMP_Text>().text = itemData.id.ToString();

        popup.Find("Equip").gameObject.SetActive(false);
        popup.Find("Unequip").gameObject.SetActive(false);
        popup.Find("Primary").gameObject.SetActive(false);
        popup.Find("Secondary").gameObject.SetActive(false);
        popup.Find("Damage Reduction").gameObject.SetActive(false);
        popup.Find("Damage").gameObject.SetActive(false);

        bool isStacked = cell.quantity > 1;
        popup.Find("Unstack").gameObject.SetActive(isStacked);

        bool isEquiped = cell.id < inventory.equipmentSlots;

        // Clothing
        if (itemData is ClothingData clothingData)
        {
            popup.Find("Damage Reduction").gameObject.SetActive(true);
            popup.Find("Damage Reduction/Value").GetComponent<TMP_Text>().text =
                clothingData.damageReduction.ToString();

            switch (clothingData.clothingType)
            {
                case ClothingType.Hat:
                case ClothingType.Mask:
                case ClothingType.Top:
                case ClothingType.Gloves:
                case ClothingType.Pants:
                case ClothingType.Shoes:
                {
                    if (isEquiped)
                        popup.Find("Unequip").gameObject.SetActive(true);
                    else
                        popup.Find("Equip").gameObject.SetActive(true);
                    break;
                }
            }
        }
        // Weapon
        else if (itemData is WeaponData weaponData)
        {
            popup.Find("Damage").gameObject.SetActive(true);
            popup.Find("Damage/Value").GetComponent<TMP_Text>().text =
                weaponData.properties.damage.ToString();

            if (isEquiped)
                popup.Find("Unequip").gameObject.SetActive(true);
            else
            {
                popup.Find("Primary").gameObject.SetActive(true);
                popup.Find("Secondary").gameObject.SetActive(true);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(popup);
        LayoutRebuilder.ForceRebuildLayoutImmediate(popup);
        MovePopup(cell);
        FilterBy(filterCategory);
    }

    public void HidePopup() => popup.gameObject.SetActive(false);

    private Coroutine delayedDeselect;

    public void DelayedDeselect()
    {
        delayedDeselect = StartCoroutine(DelayedDeselectCoroutine());
    }

    private IEnumerator DelayedDeselectCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        selectedCell = null;
        HidePopup();
        delayedDeselect = null;
    }

    public void StopDeselection()
    {
        if (delayedDeselect == null)
            return;

        StopCoroutine(delayedDeselect);
    }

    public void SelectLastCell()
    {
        if (!selectedCell)
            return;

        selectedCell.GetComponent<InventorySlot>().Select();
    }

    public void FilterBy(ItemCategory filterCategory)
    {
        for (int i = 0; i < inventory.items.Count; i++)
        {
            bool categoryMatches;
            // Equipment slots are highlighted if slot category matches
            if (i < inventory.equipmentSlots)
            {
                ItemCategory allowedCategory = inventory.slotDefinitions[i].allowedCategory;
                categoryMatches =
                    filterCategory == ItemCategory.Any || allowedCategory == filterCategory;
            }
            // Inventory slots are highlighted only if item category in slot matches
            else
            {
                InventoryCell inventoryCell = inventoryCells[i];
                ItemData itemData = inventoryCells[i].currentItem;
                if (itemData && inventoryCell.quantity > 0)
                {
                    categoryMatches = filterCategory switch
                    {
                        ItemCategory.Weapon => itemData is WeaponData,
                        ItemCategory.Clother => itemData is ClothingData,
                        ItemCategory.Misc => itemData is not WeaponData
                            && itemData is not ClothingData,
                        _ => true,
                    };
                }
                else
                {
                    categoryMatches = filterCategory == ItemCategory.Any;
                }
            }

            inventoryCells[i].slot.GetComponent<CanvasGroup>().alpha = categoryMatches
                ? activeFilterAlpha
                : inactiveFilterAlpha;
        }

        this.filterCategory = filterCategory;
    }
}
