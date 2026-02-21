using UnityEngine;

public class TouchField : MonoBehaviour
{
    [SerializeField]
    private RectTransform[] ignoreObjects;

    //[HideInInspector]
    public Vector2 TouchDist;

    [SerializeField]
    private float minDistance = 0.5f;
    private Vector2 PointerOld;

    [HideInInspector]
    public bool Pressed;
    private int activeTouchId = -1;

    void Update()
    {
        // Mobile input handle
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.touches[i];

                // If it's new touch and no active touch
                if (touch.phase == TouchPhase.Began && activeTouchId == -1)
                {
                    if (!IsTouchInIgnoredObject(touch.position))
                    {
                        activeTouchId = touch.fingerId; // Save touch's ID
                        Pressed = true;
                        PointerOld = touch.position;
                    }
                }

                // If it's active touch
                if (touch.fingerId == activeTouchId)
                {
                    if (
                        (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                        && Pressed
                    )
                    {
                        Vector2 delta = (touch.position - PointerOld) / 2;
                        if (delta.magnitude > minDistance)
                            TouchDist = delta;
                        else
                            TouchDist = Vector2.zero;

                        PointerOld = touch.position;
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        activeTouchId = -1; // Remove active touch ID
                        Pressed = false;
                        TouchDist = Vector2.zero;
                    }
                }
            }
        }
        // Mouse handle for Editor
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!IsTouchInIgnoredObject(Input.mousePosition))
                {
                    Pressed = true;
                    PointerOld = Input.mousePosition;
                }
            }
            else if (Input.GetMouseButton(0) && Pressed)
            {
                TouchDist = (Vector2)Input.mousePosition - PointerOld;
                PointerOld = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Pressed = false;
                TouchDist = Vector2.zero;
            }
        }

        // If no touch, touch dist = 0
        if (!Pressed)
        {
            TouchDist = Vector2.zero;
        }
    }

    private bool IsTouchInIgnoredObject(Vector2 screenPosition)
    {
        foreach (var rectTransform in ignoreObjects)
        {
            if (
                rectTransform != null
                && rectTransform.gameObject.activeInHierarchy
                && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition)
            )
            {
                return true;
            }
        }
        return false;
    }

    private void OnGUI()
    {
        if (Pressed && PointerOld != Vector2.zero)
        {
            // Draw rectangle
            float size = 30f;
            Rect rect = new(
                PointerOld.x - size / 2,
                Screen.height - PointerOld.y - size / 2,
                size,
                size
            );

            GUI.color = Color.red;
            GUI.Box(rect, "");
        }
    }
}
