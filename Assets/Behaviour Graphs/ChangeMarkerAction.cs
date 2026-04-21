using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "ChangeMarker",
    story: "Set [Target] to random Marker",
    category: "Action",
    id: "ccc2c38876308038049f1a513f4671ee"
)]
public partial class ChangeMarkerAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Target;

    protected override Status OnStart()
    {
        if (Target == null) return Status.Failure;

        var markers = GameObject.FindGameObjectsWithTag("Marker");

        if (markers.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, markers.Length);
            Target.Value = markers[randomIndex].transform;
            return Status.Success;
        }

        Debug.LogWarning("ChangeMarkerAction: Маркеры не найдены!");
        return Status.Failure;
    }

    protected override Status OnUpdate() => Status.Success;
}

