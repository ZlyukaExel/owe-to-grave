using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class DynamicSelector : MonoBehaviour, IMoveHandler
{
    private Selectable selectable;

    void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    public void OnMove(AxisEventData eventData)
    {
        Selectable next = GetInteractiveSelectable(selectable, eventData.moveDir);
        if (!next || !next.interactable)
            next = selectable;
        eventData.Use();
        next.Select();
    }

    private Selectable GetInteractiveSelectable(Selectable selectable, MoveDirection moveDir)
    {
        if (selectable == null)
            return selectable;

        while (true)
        {
            selectable = moveDir switch
            {
                MoveDirection.Down => selectable.FindSelectableOnDown(),
                MoveDirection.Up => selectable.FindSelectableOnUp(),
                MoveDirection.Left => selectable.FindSelectableOnLeft(),
                MoveDirection.Right => selectable.FindSelectableOnRight(),
                _ => null,
            };
            if (!selectable || selectable.interactable)
                return selectable;
        }
    }
}
