using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TMP_Dropdown))]
public class CustomDropdown : MonoBehaviour, ISubmitHandler
{
    private TMP_Dropdown dropdown;
    public UnityEvent onSelectionStart,
        onSelectionEnd;

    [SerializeField]
    private InputActionReference cancelAction;

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (!dropdown)
            return;

        onSelectionStart.Invoke();
        cancelAction.action.performed += ctx => OnSelectionEnd();
    }

    public void OnSelectionEnd()
    {
        if (!dropdown)
            return;

        onSelectionEnd.Invoke();
        cancelAction.action.performed -= ctx => OnSelectionEnd();
        dropdown.Hide();
    }
}
