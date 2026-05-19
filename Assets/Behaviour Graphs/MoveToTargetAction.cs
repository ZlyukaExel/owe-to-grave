using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Move To Target",
    story: "Set [Input] in [Target] direction",
    category: "Action",
    id: "b66b245b0e85bbf1db76af19aa8d89e0"
)]
public partial class MoveToTargetAction : Action
{
    [SerializeReference]
    public BlackboardVariable<InputManager> Input;

    [SerializeReference]
    public BlackboardVariable<Transform> Target;

    [SerializeReference]
    public BlackboardVariable<float> Speed = new(0.5f);

    [SerializeReference]
    public BlackboardVariable<CustomNavMeshAgent> CustomNavMeshAgent;
    private float timeElapsed = 0;

    protected override Status OnStart()
    {
        if (!Target.Value)
            return Status.Failure;

        timeElapsed = 0;
        CustomNavMeshAgent.Value.SetTarget(Target.Value.position);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!Target.Value)
            return Status.Failure;

        if (timeElapsed > 0.2f)
        {
            timeElapsed = 0;
            CustomNavMeshAgent.Value.SetTarget(Target.Value.position);
        }
        else
            timeElapsed += Time.deltaTime;

        // If destination reached
        if (CustomNavMeshAgent.Value.ReachedDestination)
        {
            return Status.Success;
        }

        // Move to destination
        Vector3 direction = CustomNavMeshAgent.Value.GetDirection();
        Input.Value.SetMovementVector(
            new Vector2(direction.x, direction.z).normalized * Speed.Value
        );
        return Status.Running;
    }

    protected override void OnEnd()
    {
        Input.Value.SetMovementVector(Vector2.zero);
    }
}
