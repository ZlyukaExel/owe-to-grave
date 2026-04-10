using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 3f,
        detectionRadius = 5f,
        maxDetectionRadius = 20f,
        detectionStep = 5f;

    private Marker currentMarker,
        targetMarker;
    private bool isFleeing = false;
    private Vector3 fleeDirection;

    void Start()
    {
        FindInitialMarker();
        ChooseRandomTarget();
    }

    void Update()
    {
        if (targetMarker == null)
        {
            ChooseRandomTarget();
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetMarker.transform.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetMarker.transform.position) < 0.1f)
        {
            currentMarker = targetMarker;
            ChooseRandomTarget();
        }

        Collider[] threats = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var threat in threats)
        {
            if (threat.CompareTag("Bullet"))
            {
                isFleeing = true;
                fleeDirection = (transform.position - threat.transform.position).normalized;
                FleeFormThreat();
                break;
            }
        }

        if (isFleeing)
        {
            FleeFormThreat();
        }
    }

    void FindInitialMarker()
    {
        float radius = detectionRadius;
        while (radius <= maxDetectionRadius)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out Marker marker))
                {
                    currentMarker = marker;
                    return;
                }
            }
            radius += detectionStep;
        }
        Debug.LogWarning("No marker found for NPC!");
    }

    void ChooseRandomTarget()
    {
        if (currentMarker == null || currentMarker.neighbors.Length == 0)
            return;

        targetMarker = currentMarker.neighbors[Random.Range(0, currentMarker.neighbors.Length)];
    }

    void FleeFormThreat()
    {
        Marker escapeMarker = FindEscapeMarker(fleeDirection);
        if (escapeMarker != null)
        {
            targetMarker = escapeMarker;
            isFleeing = false;
        }
    }

    Marker FindEscapeMarker(Vector3 direction)
    {
        float radius = detectionRadius;
        while (radius <= maxDetectionRadius)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out Marker marker) && marker != currentMarker)
                {
                    Vector3 toMarker = (marker.transform.position - transform.position).normalized;
                    if (Vector3.Dot(toMarker, direction) > 0.5f)
                        return marker;
                }
            }
            radius += detectionStep;
        }
        return null;
    }
}
