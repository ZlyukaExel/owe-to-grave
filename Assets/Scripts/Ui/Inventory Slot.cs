using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        InventoryCell draggedItem = eventData.pointerDrag?.GetComponent<InventoryCell>();

        if (draggedItem != null)
        {
            InventoryCell existingItem = GetComponentInChildren<InventoryCell>();

            if (existingItem != null && existingItem != draggedItem)
            {
                existingItem.ResetPosition(draggedItem.originalParent);
            }

            draggedItem.ResetPosition(transform);
        }
    }
}
