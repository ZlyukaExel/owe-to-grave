using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ActionByNavigation : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private Vector2 navigationVector = new();
    public bool mustBeSelected = false;
    private bool isSelected = false;

    [SerializeField]
    private bool changeOnHorizontal = true,
        changeOnVertical = false;

    [SerializeField]
    private InputActionReference navigateAction;
    public UnityEvent onNavUp,
        onNavDown,
        onNavRight,
        onNavLeft;
    public UnityEvent<Vector2> onNavigate;

    [SerializeField]
    private float maxDelay = 0.3f,
        minDelay = 0,
        acceleration = 0.1f;

    private float currentDelay = 0.3f,
        timeElapsed = 0;

    void Update()
    {
        if (mustBeSelected && !isSelected || navigationVector == Vector2.zero)
            return;

        timeElapsed += Time.unscaledDeltaTime;
        if (timeElapsed > currentDelay)
        {
            if (changeOnHorizontal)
            {
                if (navigationVector.x > 0)
                    onNavRight.Invoke();
                else if (navigationVector.x < 0)
                    onNavLeft.Invoke();
            }

            if (changeOnVertical)
            {
                if (navigationVector.y > 0)
                    onNavUp.Invoke();
                else if (navigationVector.y < 0)
                    onNavDown.Invoke();
            }

            onNavigate.Invoke(navigationVector);

            timeElapsed = 0;
            currentDelay = Mathf.Max(minDelay, currentDelay - acceleration);
        }
    }

    void OnEnable()
    {
        navigateAction.action.performed += HandleAction;
    }

    void OnDisable()
    {
        navigateAction.action.performed -= HandleAction;
        navigationVector = new();
        isSelected = false;
    }

    private void HandleAction(InputAction.CallbackContext ctx)
    {
        navigationVector = ctx.ReadValue<Vector2>();
        timeElapsed = Mathf.Infinity;
        currentDelay = maxDelay;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
    }
}
