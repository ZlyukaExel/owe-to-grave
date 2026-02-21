using Mirror;
using UnityEngine;

public abstract class InteractiveObject : NetworkBehaviour
{
    public abstract bool IsInteractable();
    public abstract void Interact(Transform character);
}
