using UnityEngine;
using UnityEngine.EventSystems;

public class AnyDirectionSlider: MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector]
    public Vector2 TouchDist;
    [HideInInspector]
    public Vector2 PointerOld;
    [HideInInspector]
    protected int PointerId;
    [HideInInspector]
    public bool Pressed;

    public float x = 0;
    public float y = 0;
    private float t = 0;

    void Update()
    {
        if (Pressed)
        {
            t += Time.deltaTime;
            if (PointerId >= 0 && PointerId < Input.touches.Length)
            {
                TouchDist = Input.touches[PointerId].position - PointerOld;
                PointerOld = Input.touches[PointerId].position;
            }
            else
            {
                TouchDist = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - PointerOld;
                PointerOld = Input.mousePosition;
            }
            x += TouchDist.x;
            y += TouchDist.y;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Pressed = true;
        PointerId = eventData.pointerId;
        PointerOld = eventData.position;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        Pressed = false;
        if (t < 0.2f)
            x = y = 0;
        t = 0;
    }

    private void OnDisable()
    {
        x = y = 0;
    }
}