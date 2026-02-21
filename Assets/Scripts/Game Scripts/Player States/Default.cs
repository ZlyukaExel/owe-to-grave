public class Default : State
{
    public Default(Links links)
        : base(links) { }

    public override void UpdateState()
    {
        l.movement.MovementUpdate();
        l.carTrigger.CarTriggerUpdate();
        l.itemGrabber.ItemGrabbingUpdate();
    }

    public override void FixedUpdateState()
    {
        l.movement.MovementFixedUpdate();
    }

    public override void ExitState()
    {
        l.itemGrabber.StopDragging();
    }
}
