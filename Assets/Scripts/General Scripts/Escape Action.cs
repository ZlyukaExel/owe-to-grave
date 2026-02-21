using UnityEngine;
using UnityEngine.Events;

public class EscapeAction : MonoBehaviour
{
    [SerializeField] private UnityEvent action;
    [SerializeField] private KeyCode keyCode;

    private void Update()
    {
        if (Input.GetKeyDown(keyCode))
        {
            action.Invoke();
        }
    }
}
