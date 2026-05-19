using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Mirror;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Flee",
    story: "Set [Input] in EscapeTarget direction",
    category: "Action",
    id: "73c98ab0f3d38ad863474f1739af3ea9"
)]
public partial class FleeAction : Action
{
    [SerializeReference]
    public BlackboardVariable<InputManager> Input;
    
    [SerializeReference]
    public BlackboardVariable<Marker> CurrentMarker;

    [SerializeReference]
    public BlackboardVariable<NPCController> NpcController;

    [SerializeReference]
    public BlackboardVariable<EnemyDetector> EnemyDetector;

    [SerializeReference]
    public BlackboardVariable<float> Speed = new(0.5f);

    [SerializeReference]
    public BlackboardVariable<CustomNavMeshAgent> CustomNavMeshAgent;
    private float timeElapsed = 0;
    private float DetectionRadius = 20f;
    private float MaxDetectionRadius = 100f;
    private float DetectionStep = 20f;
    private Marker TargetMarker;

    protected override Status OnStart()
    {
        if (!CurrentMarker.Value.transform)
            return Status.Failure;

        timeElapsed = 0;
        TargetMarker = CurrentMarker.Value;
        CustomNavMeshAgent.Value.SetTarget(CurrentMarker.Value.transform.position);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!TargetMarker.transform)
            return Status.Failure;

        if (timeElapsed > 0.2f)
        {
            timeElapsed = 0;
            NetworkIdentity Threat = EnemyDetector.Value.GetClosestEnemy();
            if (Threat == null)
            {
                return Status.Success;
            }
            else
            {
                Marker target = NpcController.Value.FindEscapeMarker(
                    Threat,
                    DetectionRadius,
                    MaxDetectionRadius,
                    DetectionStep,
                    TargetMarker
                );
                if (target) TargetMarker = target;
            }
            CustomNavMeshAgent.Value.SetTarget(TargetMarker.transform.position);
        }
        else timeElapsed += Time.deltaTime;

        // If destination reached
        // if (CustomNavMeshAgent.Value.ReachedDestination)
        // {
        //     return Status.Success;
        // }

        // Move to destination
        Vector3 direction = CustomNavMeshAgent.Value.GetDirection();
        Input.Value.SetMovementVector(
            new Vector2(direction.x, direction.z).normalized * Speed.Value
        );
        return Status.Running;
    }

    protected override void OnEnd()
    {
        Input.Value.SetMovementVector(Vector2.zero);
    }
}

