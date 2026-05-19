using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public Vector2 movementVector = Vector2.zero,
        mouseVector = Vector2.zero;

    protected List<InputManagerAction> inputMabagerActions = new();

    protected readonly HashSet<InputAction> pressed = new();

    public bool IsPressed(InputAction inputAction) => pressed.Contains(inputAction);

    public virtual void Awake()
    {
        foreach (var action in InputSystem.actions)
        {
            if (action.type == InputActionType.Button)
                inputMabagerActions.Add(new InputManagerAction(action, pressed));
        }
    }

    public InputManagerAction GetAction(InputAction inputAction)
    {
        foreach (var inputManagerAction in inputMabagerActions)
        {
            if (inputManagerAction.inputAction == inputAction)
                return inputManagerAction;
        }

        Debug.LogWarning($"Action \"{inputAction}\" is not assinged");
        return null;
    }

    public void SetActionPressed(InputAction inputAction, bool pressed)
    {
        if (pressed)
            GetAction(inputAction).onDown.Invoke();
        else
            GetAction(inputAction).onUp.Invoke();
    }

    public void SetMovementVector(Vector2 movement)
    {
        movementVector = movement;
    }

    public void SetMouseVector(Vector2 mouse)
    {
        mouseVector = mouse;
    }
}

[Serializable]
public class InputManagerAction
{
    public InputAction inputAction;

    [HideInInspector]
    public UnityEvent onDown = new();

    [HideInInspector]
    public UnityEvent onUp = new();
    private bool isSubscribed = false;

    public InputManagerAction(InputAction inputAction, HashSet<InputAction> pressed)
    {
        this.inputAction = inputAction;
        onDown.AddListener(() => pressed.Add(inputAction));
        onUp.AddListener(() => pressed.Remove(inputAction));
    }

    public void SubscribeToInputSystem()
    {
        if (isSubscribed)
            return;

        inputAction.started += ctx => onDown?.Invoke();
        inputAction.canceled += ctx => onUp?.Invoke();
        isSubscribed = true;
    }

    public void UnsubscribeFromInputSystem()
    {
        if (!isSubscribed)
            return;

        inputAction.started -= ctx => onDown?.Invoke();
        inputAction.canceled -= ctx => onUp?.Invoke();
        isSubscribed = false;
    }
}
