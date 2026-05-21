public class DefaultState : State
{
    public override void UpdateState()
    {
        l.movement.MovementUpdate();
        l.carTrigger.CarTriggerUpdate();
    }

    public override void FixedUpdateState()
    {
        l.movement.MovementFixedUpdate();
    }
}
