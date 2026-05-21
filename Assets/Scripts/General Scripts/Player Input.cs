using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInput : InputManager
{
    private TouchField touchField;
    public event Action<float> OnZoom;
    public Joystick joystick { get; private set; }
    public static PlayerInput Instance { get; private set; }

    [SerializeField]
    private float joystickWeight = 2;

    [HideInInspector]
    public UnityEvent onMovement,
        onNoMovement;

    [SerializeField]
    private InputActionReference moveAction,
        jumpAction,
        lookAction;

    public override void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();

        foreach (var inputManagerAction in inputMabagerActions)
        {
            inputManagerAction.SubscribeToInputSystem();
        }
    }

    public void Start()
    {
        GetButtons();
    }

    public void Update()
    {
        MouseUpdate();
        MovementUpdate();
    }

    private void MouseUpdate()
    {
        mouseVector = lookAction.action.ReadValue<Vector2>();

        float zoomDistance = touchField.ZoomDelta;

        // Rotate camera only if not zooming
        if (zoomDistance == 0)
            mouseVector += touchField.TouchDist;

        mouseVector *= Time.unscaledDeltaTime;

        float scrollValue = Mouse.current.scroll.y.ReadValue();
        float mouseScroll = Mathf.Clamp(scrollValue, -0.3f, 0.3f);
        OnZoom?.Invoke(zoomDistance + mouseScroll);
    }

    private void MovementUpdate()
    {
        movementVector =
            Vector2.ClampMagnitude(moveAction.action.ReadValue<Vector2>(), 1)
            + joystick.input * joystickWeight;

        if (movementVector.magnitude <= 0.1f && !IsPressed(jumpAction.action))
            onNoMovement.Invoke();
        else
            onMovement.Invoke();
    }

    public void StopInput()
    {
        mouseVector = movementVector = Vector3.zero;
    }

    private void GetButtons()
    {
        Transform mobileUi = transform.Find("Game Ui/Mobile Ui");
        touchField = transform.Find("Touch Field").GetComponent<TouchField>();
        joystick = mobileUi.Find("Fixed Joystick").GetComponent<FixedJoystick>();
    }
}
