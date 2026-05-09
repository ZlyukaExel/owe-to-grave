using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomSelectable : MonoBehaviour, ISubmitHandler, ISelectHandler, IDeselectHandler
{
    public UnityEvent onSelect = new(),
        onDeselect = new(),
        onSubmit = new();

    public void OnSubmit(BaseEventData eventData)
    {
        onSubmit.Invoke();
    }

    public void OnSelect(BaseEventData eventData)
    {
        onSelect.Invoke();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        onDeselect.Invoke();
    }
}
