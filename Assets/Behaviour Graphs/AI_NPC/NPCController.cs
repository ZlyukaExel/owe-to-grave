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
        ) return null;

        Marker nextMarker = currentMarker.neighbors[
            Random.Range(0, currentMarker.neighbors.Length)
        ];

        return nextMarker;
    }

    public Marker FindEscapeMarker(
        NetworkIdentity threat,
        float detectionRadius,
        float maxDetectionRadius,
        float detectionStep,
        Marker currentMarker
    )
    {
        Vector3 direction = (transform.position - threat.transform.position).normalized;
        if (
            currentMarker == null
            || currentMarker.neighbors == null
            || currentMarker.neighbors.Length == 0
            || threat == null
        ) return null;

        foreach (var neighbor in currentMarker.neighbors)
        {
            Vector3 toMarker = (neighbor.transform.position - transform.position).normalized;
            if (Vector3.Dot(toMarker, direction) > 0.5f)
                return neighbor;
        }
        return null;
    }

    public void OnBulletNear(DamageInfo damageInfo)
    {
        NetworkIdentity source = damageInfo.source;
        enemyDetector.AddEnemy(source);
    }
}
