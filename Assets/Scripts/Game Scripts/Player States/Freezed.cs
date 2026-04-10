public class FreezedState : State
{
    public override void EnterState()
    {
        pLinks?.itemGrabber.StopDragging();
        base.EnterState();
    }
}
