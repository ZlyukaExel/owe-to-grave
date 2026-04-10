using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class AnyDirectionSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool Pressed { get; private set; }
    public float x = 0,
        y = 0;
    private float startTime;

    void Update()
    {
        if (Pressed && Pointer.current != null)
        {
            Vector2 delta = Pointer.current.delta.ReadValue();
            x += delta.x;
            y += delta.y;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Pressed = true;
        startTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Pressed = false;
        if (Time.time - startTime < 0.2f)
            x = y = 0;
    }

    private void OnDisable()
    {
        x = y = Pressed ? x : 0;
        Pressed = false;
    }
}
