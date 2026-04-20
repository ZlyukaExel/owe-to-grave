using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "InitMarker",
    story: "Find Initial [Marker] in [maxDetectionRadius] with [detectionStep] and [detectionRadius]",
    category: "Action",
    id: "8eaf38610c8c9125f065aa16dc30baa8"
)]
public partial class InitMarkerAction : Action
{
    [SerializeReference] public BlackboardVariable<Marker> currentMarker;
    [SerializeReference] public BlackboardVariable<float> MaxDetectionRadius;
    [SerializeReference] public BlackboardVariable<float> DetectionStep;
    [SerializeReference] public BlackboardVariable<float> DetectionRadius;

    protected override Status OnStart()
    {
        if (currentMarker == null)
        {
            float radius = DetectionRadius;
            while (radius <= MaxDetectionRadius)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, radius);
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent(out Marker marker))
                    {
                        currentMarker = marker;
                        return Status.Running;
                    }
                }
                radius += DetectionStep;
            }
            Debug.LogWarning("No marker found for NPC!");
            return Status.Failed;
        }
        return Status.Running;
    }
}

