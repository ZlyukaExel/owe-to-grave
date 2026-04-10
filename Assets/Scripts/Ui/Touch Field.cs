using UnityEngine;
using UnityEngine.InputSystem;

public class TouchField : MonoBehaviour
{
    [SerializeField]
    private RectTransform[] ignoreObjects;

    [SerializeField]
    private float minDistance = 0.5f;

    public Vector2 TouchDist { get; private set; }
    public float zoomDistance { get; private set; }
    public bool Pressed { get; private set; }

    private Vector2 pointerOld;
    private float lastPinchDistance;

    void Update()
    {
        HandlePointerInput();
        zoomDistance = GetZoom();
    }

    private void HandlePointerInput()
    {
        var pointer = Pointer.current;
        if (pointer == null)
            return;

        bool isCurrentlyPressed = pointer.press.isPressed;
        Vector2 currentPosition = pointer.position.ReadValue();

        if (isCurrentlyPressed)
        {
            if (!Pressed)
            {
                if (!IsTouchInIgnoredObject(currentPosition))
                {
                    Pressed = true;
                    pointerOld = currentPosition;
                }
            }
            else
            {
                Vector2 delta = (currentPosition - pointerOld) / 2f;
                TouchDist = (delta.magnitude > minDistance) ? delta : Vector2.zero;
                pointerOld = currentPosition;
            }
        }
        else
        {
            Pressed = false;
            TouchDist = Vector2.zero;
        }
    }

    private float GetZoom()
    {
        var ts = Touchscreen.current;
        if (ts == null || ts.touches.Count < 2)
        {
            lastPinchDistance = 0;
            return 0;
        }

        var t1 = ts.touches[0];
        var t2 = ts.touches[1];

        if (t1.isInProgress && t2.isInProgress)
        {
            Vector2 pos1 = t1.position.ReadValue();
            Vector2 pos2 = t2.position.ReadValue();
            float currentDistance = Vector2.Distance(pos1, pos2);

            if (lastPinchDistance <= 0)
                lastPinchDistance = currentDistance;

            float deltaDistance = currentDistance - lastPinchDistance;
            lastPinchDistance = currentDistance;

            return deltaDistance * Time.unscaledDeltaTime * 10f;
        }

        lastPinchDistance = 0;
        return 0;
    }

    private bool IsTouchInIgnoredObject(Vector2 screenPosition)
    {
        foreach (var rect in ignoreObjects)
        {
            if (
                rect != null
                && rect.gameObject.activeInHierarchy
                && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition)
            )
                return true;
        }
        return false;
    }

    private void OnGUI()
    {
        if (Pressed)
        {
            float size = 30f;
            Rect rect = new(
                pointerOld.x - size / 2,
                Screen.height - pointerOld.y - size / 2,
                size,
                size
            );
            GUI.color = Color.red;
            GUI.Box(rect, "");
        }
    }
}
