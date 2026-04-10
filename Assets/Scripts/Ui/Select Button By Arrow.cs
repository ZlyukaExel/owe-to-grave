using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SelectButtonByArrows : MonoBehaviour
{
    [SerializeField]
    private Selectable selectable;

    [SerializeField]
    private InputActionReference navigateAction,
        cancelAction;

    [SerializeField]
    private bool onVerticalInput = true,
        onHorizontalInput = true;

    public static SelectButtonByArrows Instance;

    void OnEnable()
    {
        if (Instance)
            Instance.enabled = false;
        Instance = this;

        navigateAction.action.performed += SelectButton;
        cancelAction.action.performed += DeselectAll;
    }

    void OnDisable()
    {
        if (Instance == this)
            Instance = null;

        navigateAction.action.performed -= SelectButton;
        cancelAction.action.performed -= DeselectAll;
    }

    private void SelectButton(InputAction.CallbackContext ctx)
    {
        // Execute only if no button selected
        if (EventSystem.current && EventSystem.current.currentSelectedGameObject != null)
            return;

        Vector2 inputVector = ctx.ReadValue<Vector2>();
        if (!(onHorizontalInput && inputVector.x != 0 || onVerticalInput && inputVector.y != 0))
            return;

        if (selectable)
        {
            selectable.Select();
        }
        else
        {
            selectable = GetComponentInChildren<Selectable>();
            if (selectable)
                selectable.Select();
        }
    }

    private void DeselectAll(InputAction.CallbackContext ctx)
    {
        if (EventSystem.current)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
