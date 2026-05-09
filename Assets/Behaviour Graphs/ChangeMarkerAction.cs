using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "ChangeMarker",
    story: "Set [Target] to random Marker",
    category: "Action",
    id: "ccc2c38876308038049f1a513f4671ee"
)]
public partial class ChangeMarkerAction : Action
{
    [SerializeReference]
    public BlackboardVariable<Transform> Target;
    NPCController npcController = GetComponent<NPCController>();

    protected override Status OnStart()
    {
        Target = npcController.ChooseRandomTarget(Target);
        if (Target == null)
        {
            Target = npcController.FindInitialMarker(5.0, 20.0, 5.0);
        }
    }

    protected override Status OnUpdate() => Status.Success;
}
