using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Change Marker",
    story: "Set [Target] to new Marker",
    category: "Action",
    id: "8ad4b260037899a2b385a7d2f5dd825d"
)]
public partial class ChangeMarkerAction : Action
{
    [SerializeReference]
    public BlackboardVariable<Marker> Target;

    [SerializeReference]
    public BlackboardVariable<Marker> currentMarker;

    protected override void OnEnd()
    {
        if (currentMarker == null || currentMarker.neighbors.Length == 0)
        {
            return Status.Failed;
        }
        Marker targetMarker = currentMarker.neighbors[Random.Range(0, currentMarker.neighbors.Length)];
        currentMarker = Target;
        Target = targetMarker;

        return Status.Success;
    }
}

