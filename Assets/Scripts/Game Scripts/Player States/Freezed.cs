public class FreezedState : State
{
    public override void EnterState()
    {
        pLinks?.interactableTrigger.SetCheckTrigger(false);
        base.EnterState();
    }

    public override void ExitState()
    {
        pLinks?.interactableTrigger.SetCheckTrigger(true);
        base.ExitState();
    }
}
