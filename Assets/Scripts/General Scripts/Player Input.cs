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
    private float pinchInitialDistance;

    [HideInInspector]
    public UnityEvent onMovement,
        onNoMovement;
    private InputAction moveAction,
        jumpAction;

    public override void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

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
        MouseHandle();

        if (ignoreInputCount != 0)
            return;

        Vector2 moveVector = moveAction.ReadValue<Vector2>();
        Vertical = Mathf.Clamp(joystick.Vertical + moveVector.y, -1, 1);
        Horizontal = Mathf.Clamp(joystick.Horizontal + moveVector.x, -1, 1);

        if (Vertical == 0 && Horizontal == 0 && !IsPressed(jumpAction))
            onNoMovement.Invoke();
        else
            onMovement.Invoke();

        base.Update();
    }

    private void MouseHandle()
    {
        // #if UNITY_STANDALONE
        //         MouseHorizontal = Input.GetAxis("Mouse X");
        //         MouseVertical = Input.GetAxis("Mouse Y");
        // #endif

        float zoomDistance = GetPinch();

        // Simple swipe if not zooming
        if (zoomDistance == 0)
        {
            MouseHorizontal = touchField.TouchDist.x;
            MouseVertical = touchField.TouchDist.y;

            MouseHorizontal *= Time.unscaledDeltaTime;
            MouseVertical *= Time.unscaledDeltaTime;
        }

        float mouseScroll = Mathf.Clamp(Input.GetAxis("Mouse ScrollWheel"), -0.1f, 0.1f);
        zoomDistance += mouseScroll;
        OnZoom?.Invoke(zoomDistance);
    }

    private float GetPinch()
    {
        float zoomDistance = 0;

        // Pinch if no input and two fingers
        if (pressed.Count == 0 && Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (
                touch1.phase == UnityEngine.TouchPhase.Moved
                && touch2.phase == UnityEngine.TouchPhase.Moved
            )
            {
                float distance = Vector2.Distance(touch1.position, touch2.position);

                zoomDistance =
                    (distance < pinchInitialDistance ? -1 : 1) * 20 * Time.unscaledDeltaTime;

                pinchInitialDistance = distance;
            }
        }

        return zoomDistance;
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
        Transform gameUi = transform.Find("Game Ui");
        touchField = gameUi.GetComponent<TouchField>();
        joystick = gameUi.Find("Fixed Joystick").GetComponent<FixedJoystick>();
    }
}
