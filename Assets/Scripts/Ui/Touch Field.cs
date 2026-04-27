using UnityEngine;
using UnityEngine.InputSystem;

public class TouchField : MonoBehaviour
{
    [SerializeField]
    private RectTransform[] ignoreObjects;

    [SerializeField]
    private float sensitivity = 1f;

    [SerializeField]
    private float minDistance = 0.1f;

    public Vector2 TouchDist { get; private set; }
    public float ZoomDelta { get; private set; }
    public bool Pressed { get; private set; }

    private Vector2 pointerOld;
    private float lastPinchDistance;
    private bool ignoreCurrentTouch;

    void Update()
    {
        var ts = Touchscreen.current;

        if (ts != null && ts.touches.Count >= 2)
        {
            HandlePinch(ts);
        }
        else
        {
            HandleSinglePointer();
            lastPinchDistance = 0;
            ZoomDelta = 0;
        }
    }

    private void HandleSinglePointer()
    {
        var pointer = Pointer.current;
        if (pointer == null)
            return;

        Vector2 currentPosition = pointer.position.ReadValue();
        bool isPressed = pointer.press.isPressed;

        if (isPressed)
        {
            // Pressed
            if (!Pressed)
            {
                if (IsTouchInIgnoredObject(currentPosition))
                {
                    ignoreCurrentTouch = true;
                }
                else
                {
                    ignoreCurrentTouch = false;
                    pointerOld = currentPosition;
                }
                Pressed = true;
            }

            // In allowed zone
            if (!ignoreCurrentTouch)
            {
                Vector2 delta = currentPosition - pointerOld;
                TouchDist =
                    (delta.sqrMagnitude > minDistance * minDistance)
                        ? delta * sensitivity
                        : Vector2.zero;
                pointerOld = currentPosition;
            }
        }
        else
        {
            Pressed = false;
            ignoreCurrentTouch = false;
            TouchDist = Vector2.zero;
        }
    }

    private void HandlePinch(Touchscreen ts)
    {
        var t1 = ts.touches[0];
        var t2 = ts.touches[1];

        if (t1.press.isPressed && t2.press.isPressed)
        {
            if (IsTouchInIgnoredObject(t1.position.ReadValue()))
                return;

            float currentDistance = Vector2.Distance(
                t1.position.ReadValue(),
                t2.position.ReadValue()
            );

            if (lastPinchDistance > 0)
            {
                ZoomDelta = (currentDistance - lastPinchDistance) * sensitivity;
            }

            lastPinchDistance = currentDistance;
            // No rotation in zoom
            TouchDist = Vector2.zero;
        }
    }

    private bool IsTouchInIgnoredObject(Vector2 screenPosition)
    {
        for (int i = 0; i < ignoreObjects.Length; i++)
        {
            var rect = ignoreObjects[i];
            if (
                rect != null
                && rect.gameObject.activeInHierarchy
                && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition)
            )
                return true;
        }
        return false;
    }
}
