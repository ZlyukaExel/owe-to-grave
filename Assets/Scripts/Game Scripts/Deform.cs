using Mirror;
using UnityEngine;

public class Deform : NetworkBehaviour
{
    private Mesh mesh;
    private Vector3[] originalVertices;

    [SyncVar(hook = nameof(OnVerticesChanged))]
    private Vector3[] syncedVertices;
    public float minImpulse = 10;
    public float radiusDeformate = 0.5f;
    public float damageMultiplayer = 0.04f;
    public float maxDeformation = 0.3f;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        originalVertices = mesh.vertices;
        if (syncedVertices != null)
            OnVerticesChanged(originalVertices, syncedVertices);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (
            !(
                collision.transform.TryGetComponent(out StateManager _)
                || collision.transform.TryGetComponent(out Bullet _)
            )
        )
        {
            if (collision.impulse.magnitude > minImpulse)
            {
                bool isDeformated = false;
                Vector3[] vertices = mesh.vertices;
                for (int i = 0; i < mesh.vertexCount; i++)
                {
                    for (int j = 0; j < collision.contacts.Length; j++)
                    {
                        Vector3 contactPoint = transform.InverseTransformPoint(
                            collision.contacts[j].point
                        );
                        Vector3 velocity = transform.InverseTransformVector(
                            collision.relativeVelocity
                        );
                        float distance = Vector3.Distance(contactPoint, vertices[i]);
                        if (distance < radiusDeformate)
                        {
                            Vector3 deformate =
                                velocity * (radiusDeformate - distance) * damageMultiplayer;
                            Vector3 newPosition = vertices[i] + deformate;

                            Vector3 clampedPosition =
                                originalVertices[i]
                                + Vector3.ClampMagnitude(
                                    newPosition - originalVertices[i],
                                    maxDeformation
                                );
                            vertices[i] = clampedPosition;

                            isDeformated = true;
                        }
                    }
                }

                if (isDeformated)
                {
                    CmdUpdateVerticles(vertices);
                }
            }

            //bool destroyablesHit = false;
            foreach (
                Collider hit in Physics.OverlapSphere(collision.contacts[0].point, radiusDeformate)
            )
            {
                if (
                    hit.CompareTag("Destroyable")
                    && hit.TryGetComponent(out NetworkActive destroyable)
                )
                {
                    destroyable.SetActive(false);
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdUpdateVerticles(Vector3[] verticles)
    {
        syncedVertices = verticles;
    }

    private void OnVerticesChanged(Vector3[] oldVertices, Vector3[] newVertices)
    {
        mesh.vertices = newVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
