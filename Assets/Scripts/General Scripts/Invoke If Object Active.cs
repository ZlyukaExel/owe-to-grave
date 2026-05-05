using UnityEngine;
using UnityEngine.Events;

public class InvokeIfObjectActive : MonoBehaviour
{
    public GameObject objectMustBeActive;
    public bool mustBeActive = true,
        mustBeActiveSelf;
    public UnityEvent onInvoke = new();

    public void Invoke()
    {
        if (
            mustBeActive
            == (
                mustBeActiveSelf
                    ? objectMustBeActive.activeSelf
                    : objectMustBeActive.activeInHierarchy
            )
        )
            onInvoke.Invoke();
    }
}
