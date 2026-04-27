using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class InventoryCell : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemData currentItem;
    public int quantity;

    [SerializeField]
    private Image itemIcon;
    private TMP_Text quantityLabel;
    public int slotIndex { get; private set; } = -1;
    public Canvas canvas;
    public RectTransform rectTransform { get; private set; }
    private CanvasGroup canvasGroup;
    public Transform originalParent { get; private set; }
    private InventoryUi inventoryUi;
    public bool isKeyboardDragging;
    public GameObject fallbackImage;

    public void SetSlotIndex(InventoryUi inventoryUi, int index)
    {
        Init();
        this.inventoryUi = inventoryUi;
        slotIndex = index;
    }

    public void SetItem(ItemData item, int quantity)
    {
        bool isValid = item != null && item.id > 0 && quantity > 0;
        gameObject.SetActive(isValid);
        if (fallbackImage)
            fallbackImage.SetActive(!isValid);

        if (!isValid)
            return;

        currentItem = item;
        itemIcon.sprite = item.icon;
        this.quantity = quantity;
        if (quantityLabel)
            quantityLabel.text = quantity > 1 ? quantity.ToString() : string.Empty;
    }

    private void Init()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>(true);
        canvasGroup = GetComponent<CanvasGroup>();
        quantityLabel = GetComponentInChildren<TMP_Text>(true);
    }

    public void SetItem(uint itemId, int quantity)
    {
        if (itemId == 0)
        {
            gameObject.SetActive(false);
            if (fallbackImage)
                fallbackImage.SetActive(true);
            return;
        }

        ItemData item = ItemDataManager.Instance.GetItem(itemId);
        SetItem(item, quantity);
    }

    public void SetItem(InventoryItem item)
    {
        SetItem(item.itemId, item.quantity);
    }

    public void OnBeginDrag(PointerEventData eventData) => OnBeginDrag();

    public void OnBeginDrag()
    {
        if (currentItem == null)
            return;

        originalParent = transform.parent;
        if (!canvas)
            print($"No canvas at {gameObject.name}");
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

    public void OnEndDrag(PointerEventData eventData) => OnEndDrag();

    public void OnEndDrag()
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

    public void StartKeyboardDrag()
    {
        isKeyboardDragging = true;
        OnBeginDrag();
    }

    public void EndKeyboardDrag()
    {
        if (!isKeyboardDragging)
            return;

        isKeyboardDragging = false;
        OnEndDrag();
    }

    public void ResetPosition(Transform newParent)
    {
        originalParent = newParent;
        transform.SetParent(newParent);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    public void OnSelect()
    {
        inventoryUi.MoveCurrentCell(this);
    }
}
