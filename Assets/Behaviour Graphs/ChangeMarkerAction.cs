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
<<<<<<< HEAD
    public BlackboardVariable<Marker> TargetMarker;
=======
>>>>>>> fc0c03f43b6b70cb2f46163c595c1ca5ff973080
    NPCController npcController = GetComponent<NPCController>();

    protected override Status OnStart()
    {
<<<<<<< HEAD
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
=======
        Target = npcController.ChooseRandomTarget(Target);
        if (Target == null)
        {
            Target = npcController.FindInitialMarker(5.0, 20.0, 5.0);
        }
>>>>>>> fc0c03f43b6b70cb2f46163c595c1ca5ff973080
    }

    protected override Status OnUpdate() => Status.Success;
}
