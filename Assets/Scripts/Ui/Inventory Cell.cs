using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class InventoryCell : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemData currentItem;

    [SerializeField]
    private Image itemIcon;

    [SerializeField]
    private TMP_Text quantityLabel;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public Transform originalParent { get; private set; }

    public void SetItem(ItemData item, int quantity)
    {
        currentItem = item;

        gameObject.SetActive(item);

        if (item)
            itemIcon.sprite = item.icon;

        quantityLabel.text = quantity > 1 ? quantity.ToString() : "";
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetItem(uint itemId, int quantity)
    {
        ItemData item = ItemDataManager.Instance.GetItem(itemId);
        SetItem(item, quantity);
    }

    public void SetItem(InventoryItem item)
    {
        SetItem(item.itemId, item.quantity);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null)
            return;

        originalParent = transform.parent;
        transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentItem == null)
            return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentItem == null)
            return;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (transform.parent == canvas.transform)
        {
            ResetPosition(originalParent);
        }
    }

    public void ResetPosition(Transform newParent)
    {
        originalParent = newParent;
        transform.SetParent(newParent);
        transform.localPosition = Vector3.zero;
    }
}
