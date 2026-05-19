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

    // private Marker currentMarker,
    //     targetMarker;
    // private bool isFleeing = false;
    // private Vector3 fleeDirection;

    // void Start()
    // {
    //     // FindInitialMarker();
    //     // ChooseRandomTarget();
    // }

    // void Update()
    // {
    //     // if (targetMarker == null)
    //     // {
    //     //     ChooseRandomTarget();
    //     //     return;
    //     // }

    //     // transform.position = Vector3.MoveTowards(
    //     //     transform.position,
    //     //     targetMarker.transform.position,
    //     //     moveSpeed * Time.deltaTime
    //     // );

    //     // if (Vector3.Distance(transform.position, targetMarker.transform.position) < 0.1f)
    //     // {
    //     //     currentMarker = targetMarker;
    //     //     ChooseRandomTarget();
    //     // }

    //     // Collider[] threats = Physics.OverlapSphere(transform.position, detectionRadius);
    //     // foreach (var threat in threats)
    //     // {
    //     //     if (threat.CompareTag("Bullet"))
    //     //     {
    //     //         isFleeing = true;
    //     //         fleeDirection = (transform.position - threat.transform.position).normalized;
    //     //         FleeFormThreat();
    //     //         break;
    //     //     }
    //     // }

    //     // if (isFleeing)
    //     // {
    //     //     FleeFormThreat();
    //     // }
    // }

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

    // void FleeFormThreat()
    // {
    //     Marker escapeMarker = FindEscapeMarker(fleeDirection);
    //     if (escapeMarker != null)
    //     {
    //         targetMarker = escapeMarker;
    //         // isFleeing = false;
    //     }
    // }

    public Marker FindEscapeMarker(
        NetworkIdentity threat,
        float detectionRadius,
        float maxDetectionRadius,
        float detectionStep,
        Marker currentMarker
    )
    {
        Vector3 direction = (transform.position - threat.transform.position).normalized;
        // float radius = detectionRadius;
        // while (radius <= maxDetectionRadius)
        // {
        //     Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        //     foreach (var hit in hits)
        //     {
        //         if (hit.TryGetComponent(out Marker marker))
        //         {
        //             Vector3 toMarker = (marker.transform.position - transform.position).normalized;
        //             if (Vector3.Dot(toMarker, direction) > 0.5f)
        //                 return marker;
        //         }
        //     }
        //     radius += detectionStep;
        // }
        // return null;

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
