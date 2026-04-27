using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CustomNavMeshAgent : MonoBehaviour
{
    private NavMeshAgent agent;

    void Awake() => agent = GetComponent<NavMeshAgent>();

    void Start()
    {
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    public Vector3 GetDirection(Vector3 targetPosition)
    {
        agent.SetDestination(targetPosition);
        agent.nextPosition = transform.position;
        Vector3 direction = agent.steeringTarget - transform.position;
        if (agent.remainingDistance < agent.stoppingDistance)
            return Vector3.zero;
        return direction.normalized;
    }
}
