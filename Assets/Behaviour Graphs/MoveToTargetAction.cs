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
    public BlackboardVariable<CustomNavMeshAgent> CustomNavMeshAgent;

    protected override Status OnUpdate()
    {
        Vector3 direction = CustomNavMeshAgent.Value.GetDirection(Target.Value.position);

        if (direction.magnitude <= 0.1f)
            return Status.Success;

        Input.Value.SetMovementVector(new(direction.x, direction.z));
        return Status.Running;
    }

    protected override void OnEnd()
    {
        Input.Value.SetMovementVector(Vector2.zero);
    }
}
