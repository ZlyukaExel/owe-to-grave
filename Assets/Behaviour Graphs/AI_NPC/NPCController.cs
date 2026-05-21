using Mirror;
using UnityEngine;

[RequireComponent(typeof(EnemyDetector))]
public class NPCController : MonoBehaviour
{
    [SerializeField]
    private float detectionRadius = 20f,
        maxDetectionRadius = 100f,
        detectionStep = 20f;

    private EnemyDetector enemyDetector;

    void Awake()
    {
        enemyDetector = GetComponent<EnemyDetector>();
    }

    public Marker FindInitialMarker()
    {
        float radius = detectionRadius;
        while (radius <= maxDetectionRadius)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out Marker marker))
                {
                    // Debug.LogWarning("NpcGoToMarker: " + marker);
                    return marker;
                }
            }
            radius += detectionStep;
        }
        Debug.LogWarning("ChangeMarkerAction: Маркеры не найдены!");
        return null;
    }

    public Marker ChooseRandomNeighbor(Marker currentMarker)
    {
        if (
            currentMarker == null
            || currentMarker.neighbors == null
            || currentMarker.neighbors.Length == 0
        )
            return null;

        Marker nextMarker = currentMarker.neighbors[
            Random.Range(0, currentMarker.neighbors.Length)
        ];

        return nextMarker;
    }

    public Marker FindEscapeMarker(NetworkIdentity threat, Marker currentMarker)
    {
        if (
            currentMarker == null
            || currentMarker.neighbors == null
            || currentMarker.neighbors.Length == 0
            || threat == null
        )
            return null;

        Marker bestMarker = null;
        float maxDistanceToThreat = -1f;
        Vector3 direction = (transform.position - threat.transform.position).normalized;

        Vector3 toCurrent = (currentMarker.transform.position - transform.position).normalized;
        if (Vector3.Dot(toCurrent, direction) > 0.5f)
        {
            maxDistanceToThreat = Vector3.Distance(
                threat.transform.position,
                currentMarker.transform.position
            );
            bestMarker = currentMarker;
        }

        foreach (var neighbor in currentMarker.neighbors)
        {
            if (neighbor == null)
                continue;

            Vector3 toMarker = (neighbor.transform.position - transform.position).normalized;
            if (Vector3.Dot(toMarker, direction) <= 0.5f)
                continue;

            float currentMarkerDistance = Vector3.Distance(
                threat.transform.position,
                neighbor.transform.position
            );
            if (currentMarkerDistance > maxDistanceToThreat)
            {
                maxDistanceToThreat = currentMarkerDistance;
                bestMarker = neighbor;
            }
        }

        return bestMarker;
    }

    public void OnBulletNear(DamageInfo damageInfo)
    {
        NetworkIdentity source = damageInfo.source;
        enemyDetector.AddEnemy(source);
    }
}
