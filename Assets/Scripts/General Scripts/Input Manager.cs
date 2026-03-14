using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    private float ignoreInputCount;
    public bool mouseMoved => (MouseHorizontal != 0) && (MouseVertical != 0);
    public bool isMoving => new Vector2(Vertical, Horizontal).magnitude > 0.1f;
    private TouchField touchField;
    public float Vertical { get; private set; }
    public float Horizontal { get; private set; }
    public float MouseHorizontal { get; private set; }
    public float MouseVertical { get; private set; }

    [SerializeField]
    private float smoothingSpeed = 3;
    public float SmoothedVertical { get; private set; }
    public float SmoothedHorizontal { get; private set; }

    [SerializeField]
    private InputAction[] inputActions;
    public UnityEvent onMovement;
    public UnityEvent onNoMovement;
    private readonly HashSet<KeyCode> pressed = new();
    public event Action<float> OnZoom;

    public bool GetKey(KeyCode keyCode) => pressed.Contains(keyCode);

    public Joystick joystick { get; private set; }
    public static InputManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        GetButtons();

        foreach (var action in inputActions)
        {
            SetListeners(action.triggers, action.keyCode);
            SetListeners(action.buttons, action.keyCode);
        }
    }

    void Update()
    {
        MouseHandle();

        if (ignoreInputCount != 0)
            return;

        KeyboardHandle();
    }

    private void KeyboardHandle()
    {
        CheckKeyboardKey(KeyCode.W);
        CheckKeyboardKey(KeyCode.A);
        CheckKeyboardKey(KeyCode.S);
        CheckKeyboardKey(KeyCode.D);

        foreach (var action in inputActions)
        {
            if (!action.mobileOnly)
                CheckKeyboardKey(action.keyCode);
        }

        // Buttons input
        float buttonsVertical = 0,
            buttonsHorizontal = 0;

        if (GetKey(KeyCode.W) && !GetKey(KeyCode.S))
            buttonsVertical = 1;
        else if (!GetKey(KeyCode.W) && GetKey(KeyCode.S))
            buttonsVertical = -1;

        if (GetKey(KeyCode.D) && !GetKey(KeyCode.A))
            buttonsHorizontal = 1;
        else if (!GetKey(KeyCode.D) && GetKey(KeyCode.A))
            buttonsHorizontal = -1;

        Vertical = Mathf.Clamp(joystick.Vertical + buttonsVertical, -1, 1);
        Horizontal = Mathf.Clamp(joystick.Horizontal + buttonsHorizontal, -1, 1);

        if (Vertical == 0 && Horizontal == 0 && !GetKey(KeyCode.Space))
            onNoMovement.Invoke();
        else
            onMovement.Invoke();

        // Smoothly changing input
        SmoothedVertical = Mathf.MoveTowards(
            SmoothedVertical,
            Vertical,
            Time.deltaTime * smoothingSpeed
        );
        SmoothedHorizontal = Mathf.MoveTowards(
            SmoothedHorizontal,
            Horizontal,
            Time.deltaTime * smoothingSpeed
        );
    }

    private void CheckKeyboardKey(KeyCode keyCode)
    {
        if (Input.GetKeyDown(keyCode))
        {
            OnButtonDown(keyCode);
        }

        if (Input.GetKeyUp(keyCode))
        {
            OnButtonUp(keyCode);
        }
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

    private float pinchInitialDistance;

    private float GetPinch()
    {
        float zoomDistance = 0;

        // Pinch if no input and two fingers
        if (pressed.Count == 0 && Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                float distance = Vector2.Distance(touch1.position, touch2.position);

                zoomDistance =
                    (distance < pinchInitialDistance ? -1 : 1) * 20 * Time.unscaledDeltaTime;

                pinchInitialDistance = distance;
            }
        }

        return zoomDistance;
    }

    public void ResetInput()
    {
        Vertical = Horizontal = 0;
    }

    public void StopInput()
    {
        ResetInput();
        ignoreInputCount++;
    }

    public void StartInput()
    {
        ignoreInputCount = Math.Max(ignoreInputCount - 1, 0);
    }

    private void GetButtons()
    {
        Transform gameUi = transform.Find("Game Ui");
        touchField = gameUi.GetComponent<TouchField>();
        joystick = gameUi.Find("Fixed Joystick").GetComponent<FixedJoystick>();
    }

    public void OnButtonDown(KeyCode keyCode)
    {
        pressed.Add(keyCode);
        foreach (var action in inputActions)
        {
            if (action.keyCode == keyCode)
            {
                action.onDown?.Invoke();
                break;
            }
        }
    }

    public void OnButtonUp(KeyCode keyCode)
    {
        foreach (var action in inputActions)
        {
            if (action.keyCode == keyCode)
            {
                action.onUp?.Invoke();
                break;
            }
        }
        pressed.Remove(keyCode);
    }

    private void SetListeners(EventTrigger[] triggers, KeyCode keyCode)
    {
        if (triggers == null)
            return;
        foreach (var trigger in triggers)
        {
            trigger.AddPointerUpAndDownListeners(
                () => OnButtonDown(keyCode),
                () => OnButtonUp(keyCode)
            );
        }
    }

    private void SetListeners(Button[] buttons, KeyCode keyCode)
    {
        if (buttons == null)
            return;
        foreach (var button in buttons)
        {
            button.onClick.AddListener(() => OnButtonUp(keyCode));
        }
    }

    public InputAction GetAction(KeyCode keyCode)
    {
        foreach (var inputAction in inputActions)
        {
            if (inputAction.keyCode == keyCode)
                return inputAction;
        }

        Debug.LogWarning($"KeyCode {keyCode} is not assinged");
        return null;
    }
}

[Serializable]
public class InputAction
{
    public KeyCode keyCode;
    public EventTrigger[] triggers;
    public Button[] buttons;

    [HideInInspector]
    public UnityEvent onDown;

    [HideInInspector]
    public UnityEvent onUp;

    // Remove later
    public bool mobileOnly;
}
