using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class InteractiveObject : NetworkBehaviour
{
    [SerializeField]
    private GameObject interactiveLabel;
    public abstract bool IsInteractable();

    public virtual void OnInteractButtonDown(Transform character) { }

    public virtual void OnInteractButtonUp(Transform character) { }

    public virtual void Awake()
    {
        if (!interactiveLabel)
            Debug.LogError("Interactive label not set at object: " + gameObject);
    }

    public void ShowLabel(bool show)
    {
        interactiveLabel.SetActive(show);
    }
}
