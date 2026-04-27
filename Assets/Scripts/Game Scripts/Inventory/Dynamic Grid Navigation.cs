using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(ActionByNavigation))]
public class DynamicGridNavigation : MonoBehaviour
{
    public Selectable topSelectable,
        bottomSelectable;
    private Selectable[] items;
    private int columns,
        currentIndex = 0;

    [SerializeField]
    private InputActionReference navigationAction;

    private bool isGridFocused = false;

    void Awake()
    {
        items = GetComponentsInChildren<Selectable>();
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].gameObject == gameObject)
                continue;

            var item = items[i];
            int indexForThisButton = i;
            var selectable = item.gameObject.AddComponent<CustomSelectable>();

            selectable.onSelect.AddListener(() =>
            {
                isGridFocused = true;
                currentIndex = indexForThisButton;
            });

            selectable.onDeselect.AddListener(() =>
            {
                isGridFocused = false;
            });
        }

        GetComponent<ActionByNavigation>().onNavigate.AddListener(OnNavigate);
    }

    void OnEnable()
    {
        columns = GetActualColumns();
    }

    public int GetActualColumns()
    {
        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        float width = rectTransform.rect.width;
        float cellWidth = grid.cellSize.x;
        float spacingX = grid.spacing.x;
        float paddingLeftRight = grid.padding.left + grid.padding.right;

        int columns = Mathf.FloorToInt(
            (width - paddingLeftRight + spacingX) / (cellWidth + spacingX)
        );

        return Mathf.Max(1, columns);
    }

    private void OnNavigate(Vector2 direction)
    {
        if (!isGridFocused)
            return;

        if (direction.x > 0)
            MoveSelection(1);
        else if (direction.x < 0)
            MoveSelection(-1);
        else if (direction.y > 0)
            MoveSelection(-columns);
        else if (direction.y < 0)
            MoveSelection(columns);
    }

    private void MoveSelection(int step)
    {
        int nextIndex = currentIndex + step;

        if (nextIndex < 0)
        {
            if (step == -columns && topSelectable != null)
            {
                topSelectable.Select();
            }
            return;
        }

        if (nextIndex >= items.Length)
        {
            if (step == columns)
            {
                if (bottomSelectable != null)
                {
                    bottomSelectable.Select();
                }
                else if (currentIndex < items.Length - 1)
                {
                    currentIndex = items.Length - 1;
                    items[currentIndex].Select();
                }
            }
            return;
        }

        currentIndex = nextIndex;
        items[currentIndex].Select();
    }
}
