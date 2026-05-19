using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "ChangeMarker",
    story: "Set [Target] to random Marker via [NpcController]",
    category: "Action",
    id: "ccc2c38876308038049f1a513f4671ee"
)]
public partial class ChangeMarkerAction : Action
{
    [SerializeReference]
    public BlackboardVariable<Marker> Target;

    [SerializeReference]
    public BlackboardVariable<NPCController> NpcController;

    protected override Status OnStart()
    {
        if (Target.Value == null)
        {
            Target.Value = NpcController.Value.FindInitialMarker();
        }
        else
        {
            Target.Value = NpcController.Value.ChooseRandomNeighbor(Target.Value);
        }

        return Status.Success;
    }
}
