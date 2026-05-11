using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class CategoryToggle : MonoBehaviour
{
    public ItemCategory category;
    private Toggle toggle;
    private InventoryUi inventoryUI;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        inventoryUI = GetComponentInParent<InventoryUi>();

        toggle.onValueChanged.AddListener(
            (isOn) =>
            {
                if (isOn)
                    inventoryUI.FilterBy(category);
            }
        );
    }
}
