using System;
using Mirror;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

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
    private Marker targetMarker;

    protected override Status OnStart()
    {
        if (!CurrentMarker.Value.transform)
            return Status.Failure;

        timeElapsed = 0;
        targetMarker = CurrentMarker.Value;
        CustomNavMeshAgent.Value.SetTarget(CurrentMarker.Value.transform.position);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!targetMarker.transform)
            return Status.Failure;

        // If target reached
        if (CustomNavMeshAgent.Value.ReachedDestination || timeElapsed > 0.3f)
        {
            timeElapsed = 0;
            NetworkIdentity threat = EnemyDetector
                .Value.GetClosestEnemy()
                .entity.currentNetworkIdentity;

            if (threat == null)
                return Status.Success;
            else
            {
                Marker target = NpcController.Value.FindEscapeMarker(threat, targetMarker);
                if (target)
                {
                    // Change targetMarker only if reached destination
                    if (CustomNavMeshAgent.Value.ReachedDestination)
                        targetMarker = target;
                    CustomNavMeshAgent.Value.SetTarget(target.transform.position);
                }
            }
        }
        else
        {
            timeElapsed += Time.deltaTime;
        }

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
