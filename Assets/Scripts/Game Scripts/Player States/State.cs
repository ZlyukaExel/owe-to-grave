public abstract class State
{
    protected Links l;

    public State(Links links)
    {
        l = links;
    }

    public virtual void UpdateState() { }

    public virtual void FixedUpdateState() { }

    public virtual void ExitState() { }
}
