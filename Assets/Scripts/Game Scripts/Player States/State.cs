using Mirror;

[UnityEngine.RequireComponent(typeof(Links))]
public abstract class State : NetworkBehaviour
{
    protected Links l;

    public virtual void Awake()
    {
        l = GetComponent<Links>();
    }

    public virtual void UpdateState() { }

    public virtual void FixedUpdateState() { }

    public virtual void EnterState() { }

    public virtual void ExitState() { }
}
