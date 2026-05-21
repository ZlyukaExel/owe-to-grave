using Mirror;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Links))]
public abstract class State : NetworkBehaviour
{
    protected Links l;
    protected PlayerLinks pLinks => l as PlayerLinks;

    [HideInInspector]
    public UnityEvent onStateEnter,
        onStateExit;

    public virtual void Awake()
    {
        l = GetComponent<Links>();
    }

    public virtual void UpdateState() { }

    public virtual void FixedUpdateState() { }

    public virtual void EnterState()
    {
        onStateEnter.Invoke();
    }

    public virtual void ExitState()
    {
        onStateExit.Invoke();
    }
}
