using Mirror;
using UnityEngine;

public class NetworkDisable : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnStateChanged))]
    public bool isEnabled = true;

    [SerializeField]
    private MonoBehaviour[] ownerComponents;

    void Start()
    {
        if (!isEnabled)
            ResetDisabler();
    }

    [Command(requiresAuthority = false)]
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }

    private void OnStateChanged(bool oldVar, bool newVar)
    {
        ResetDisabler();
    }

    public void ResetDisabler()
    {
        // Collision
        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = isEnabled;
        }

        // Renderers
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = isEnabled;
        }

        // Reliable rigidbody
        if (TryGetComponent(out NetworkRigidbodyReliable reliableRigidbody))
        {
            reliableRigidbody.isKinematic = !isEnabled;
        }

        // Unreliable rigidbody
        if (TryGetComponent(out NetworkRigidbodyUnreliable unreliableRigidbody))
        {
            unreliableRigidbody.isKinematic = !isEnabled;
        }

        // Other components
        if (isOwned)
        {
            foreach (var component in ownerComponents)
            {
                component.enabled = isEnabled;
            }
        }

        // If enabling, reset all child ones
        if (isEnabled)
        {
            foreach (var disabler in GetComponentsInChildren<NetworkDisable>())
            {
                if (disabler == this)
                    continue;
                disabler.ResetDisabler();
            }
        }
    }
}
