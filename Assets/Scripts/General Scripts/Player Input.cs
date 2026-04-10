using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInput : InputManager
{
    private float ignoreInputCount;
    private TouchField touchField;
    public event Action<float> OnZoom;
    public Joystick joystick { get; private set; }
    public static PlayerInput Instance { get; private set; }

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

    public override void Update()
    {
        MouseUpdate();

        if (ignoreInputCount != 0)
            return;

        MovementUpdate();

        base.Update();
    }

    private void MouseUpdate()
    {
        Vector2 lookVector = lookAction.action.ReadValue<Vector2>();
        MouseHorizontal = lookVector.x;
        MouseVertical = lookVector.y;

        float zoomDistance = touchField.zoomDistance;

        // Rotate camera only if not zooming
        if (zoomDistance == 0)
        {
            MouseHorizontal += touchField.TouchDist.x;
            MouseVertical += touchField.TouchDist.y;
        }

        MouseHorizontal *= Time.unscaledDeltaTime;
        MouseVertical *= Time.unscaledDeltaTime;

        float scrollValue = Mouse.current.scroll.y.ReadValue();
        float mouseScroll = Mathf.Clamp(scrollValue, -0.3f, 0.3f);
        OnZoom?.Invoke(zoomDistance + mouseScroll);
    }

    private void MovementUpdate()
    {
        Vector2 moveVector = moveAction.action.ReadValue<Vector2>();
        Vertical = Mathf.Clamp(joystick.Vertical + moveVector.y, -1, 1);
        Horizontal = Mathf.Clamp(joystick.Horizontal + moveVector.x, -1, 1);

        if (Vertical == 0 && Horizontal == 0 && !IsPressed(jumpAction.action))
            onNoMovement.Invoke();
        else
            onMovement.Invoke();
    }

    public void StartInput()
    {
        ignoreInputCount = Math.Max(ignoreInputCount - 1, 0);
    }

    public void StopInput()
    {
        Vertical = SmoothedVertical = 0;
        Horizontal = SmoothedHorizontal = 0;
        ignoreInputCount++;
    }

    private void GetButtons()
    {
        Transform mobileUi = transform.Find("Game Ui/Mobile Ui");
        touchField = mobileUi.Find("Touch Field").GetComponent<TouchField>();
        joystick = mobileUi.Find("Fixed Joystick").GetComponent<FixedJoystick>();
    }
}
