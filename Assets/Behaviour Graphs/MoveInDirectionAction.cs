using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Move in direction",
    story: "Agent sets [input] movement direction to [movement_direction]",
    category: "Action",
    id: "bb740e1c42bb76d918878a4bbab7a133"
)]
public partial class MoveInDirectionAction : Action
{
    [SerializeReference]
    public BlackboardVariable<InputManager> Input;

    [SerializeReference]
    public BlackboardVariable<Vector2> Movement_direction;

    protected override Status OnStart()
    {
        Input.Value.SetMovementVector(Movement_direction.Value);
        return Status.Success;
    }
}
