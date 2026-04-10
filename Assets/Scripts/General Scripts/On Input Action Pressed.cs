using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class OnInputActionPressed : MonoBehaviour
{
    [SerializeField]
    private UnityEvent action;

    [SerializeField]
    private InputActionReference inputAction;
    private EventSystem eventSystem;

    void Awake()
    {
        if (!inputAction)
            print("Action for " + gameObject.name + " is not set!");
    }

    void Start()
    {
        eventSystem = EventSystem.current;
    }

    void OnEnable()
    {
        inputAction.action.performed += OnAction;
    }

    void OnDisable()
    {
        inputAction.action.performed -= OnAction;
    }

    private void OnAction(InputAction.CallbackContext ctx) => OnAction();

    public void OnAction()
    {
        if (eventSystem != EventSystem.current)
            return;
        StartCoroutine(InvokeAtTheEndOfFrame());
    }

    private IEnumerator InvokeAtTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        action.Invoke();
    }
}
