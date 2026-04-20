using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Find First Marker",
    story: "Find First Marker",
    category: "Action",
    id: "c018289c2674a8b313759a2803e2a6a1"
)]
public partial class FindFirstMarkerAction : Action
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

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

