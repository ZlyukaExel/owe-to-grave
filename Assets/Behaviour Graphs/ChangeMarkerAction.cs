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
    public BlackboardVariable<Marker> TargetMarker;
    NPCController npcController = GetComponent<NPCController>();

    protected override Status OnStart()
    {
        if (TargetMarker == null)
        {
            return Status.Failure;
        }
        TargetMarker = npcController.ChooseRandomTarget(TargetMarker);
        if (TargetMarker == null)
        {
            return Status.Failure;
        }
        Target = TargetMarker.transform;
    }

    protected override Status OnUpdate() => Status.Success;
}
