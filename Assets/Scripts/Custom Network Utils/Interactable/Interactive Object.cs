using Mirror;
using UnityEngine;

public abstract class InteractiveObject : NetworkBehaviour
{
    public abstract string InteractionText();
    public abstract bool IsInteractable();
    public abstract void Interact(Transform character);
}
