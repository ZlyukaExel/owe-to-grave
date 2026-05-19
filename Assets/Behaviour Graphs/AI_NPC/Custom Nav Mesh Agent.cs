using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CustomNavMeshAgent : MonoBehaviour
{
    public NavMeshAgent agent { private set; get; }

    void Awake() => agent = GetComponent<NavMeshAgent>();

    void Start()
    {
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    public void SetTarget(Vector3 targetPosition)
    {
        agent.SetDestination(targetPosition);
    }

    public bool ReachedDestination =>
        !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

    public Vector3 GetDirection()
    {
        agent.nextPosition = transform.position;

        // While pending path, return straight forward direction
        if (agent.pathPending)
        {
            return (agent.destination - transform.position).normalized;
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            // print(agent.gameObject.name + " got to the target");
            return Vector3.zero;
        }

        return agent.desiredVelocity;
    }
}
