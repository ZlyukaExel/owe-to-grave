using UnityEngine;
using UnityEngine.Events;

public class InvokeOnEnable : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onEnable;

    [SerializeField]
    private UnityEvent onDisable;

    void OnEnable()
    {
        onEnable.Invoke();
    }

    void OnDisable()
    {
        onDisable.Invoke();
    }
}
