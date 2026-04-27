using UnityEngine;

public class Marker : MonoBehaviour
{
    public Marker[] neighbors;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (neighbors == null)
            return;
        Gizmos.DrawSphere(transform.position, 0.5f);
        foreach (var neighbor in neighbors)
        {
            if (neighbor != null)
            {
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
                Gizmos.DrawSphere(neighbor.transform.position, 0.5f);
            }
        }
    }
}
