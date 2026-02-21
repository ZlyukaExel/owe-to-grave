using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [HideInInspector]
    public float damage = 20,
        speed = 1,
        lifeTime = 2,
        critMultiplier = 3;

    [SerializeField]
    private bool isKinematic = false;

    [SerializeField]
    private LayerMask obstaclesLayers;
    private readonly List<Collider> ignoreColliders = new();

    [SerializeField]
    private GameObject bulletHole,
        bulletParticle;

    private void Start()
    {
        if (!isServer)
        {
            enabled = false;
            return;
        }

        StartCoroutine(DelayedDestroy(lifeTime));
    }

    private void Update()
    {
        if (!isServer)
        {
            return;
        }

        if (!isKinematic)
            MoveBullet();
    }

    private void MoveBullet()
    {
        float step = speed * Time.deltaTime;
        foreach (
            var hit in Physics.RaycastAll(
                transform.position,
                transform.forward,
                step,
                obstaclesLayers
            )
        )
        {
            if (ignoreColliders.Contains(hit.collider))
                continue;

            // If hit
            HandleHit(hit);

            // Stop check if bullet destroyed
            if (isKinematic)
                break;
        }

        // Move bullet if not destroyed
        if (!isKinematic)
            transform.position += transform.forward * step;
    }

    public void Initiate(float speed, Collider[] ignoreColliders)
    {
        this.speed = speed;
        IgnoreColliders(ignoreColliders);
    }

    public void IgnoreColliders(Collider[] colliders)
    {
        ignoreColliders.AddRange(colliders);
    }

    private void HandleHit(RaycastHit hit)
    {
        if (!isServer)
            return;

        //print(hit.collider);

        switch (hit.collider.tag)
        {
            // Deal damage
            case "Player":
            {
                NetworkHitpoints hp = hit.transform.GetComponent<NetworkHitpoints>();

                if (hp.critPoints.Contains(hit.collider))
                    hp.Damage(damage * critMultiplier);
                else
                    hp.Damage(damage);

                break;
            }
            // Destroy destroyable
            case "Destroyable":
            {
                if (hit.collider.TryGetComponent(out NetworkActive destroyable))
                    destroyable.SetActive(false);
                break;
            }
            // Create particles and destroy bullet
            default:
            {
                // Destroy destroyables around the contact point
                foreach (Collider colliderHit in Physics.OverlapSphere(hit.point, 0.1f))
                {
                    if (
                        colliderHit.CompareTag("Destroyable")
                        && colliderHit.TryGetComponent(out NetworkActive destroyable)
                    )
                        destroyable.SetActive(false);
                }

                // Apply force to rigidbody
                if (
                    hit.collider.attachedRigidbody is Rigidbody rigidbody // Has rigidbody
                    && hit.transform.gameObject.layer == LayerMask.NameToLayer("Items") // Is item
                    && rigidbody.TryGetComponent(out NetworkIdentity networkIdentity) // Is network object
                )
                {
                    // Give autority if hasn't already
                    if (networkIdentity.connectionToClient != connectionToClient)
                    {
                        networkIdentity.RemoveClientAuthority();
                        networkIdentity.AssignClientAuthority(connectionToClient);
                    }

                    // Use TargetRpc cuz only owner can apply force
                    Vector3 forceDirection = -hit.normal;
                    forceDirection.y = 0;
                    RpcApplyForce(networkIdentity, forceDirection);
                }
                // Non-items create bullet holes
                else
                {
                    SpawnBulletHoleRpc(hit.point, Quaternion.LookRotation(hit.normal));
                }

                // Creating particle
                SpawnParticleRpc(
                    hit.point + hit.normal * 0.01f,
                    Quaternion.LookRotation(hit.normal)
                );

                // Waiting for particles to be created and then destroying bullet
                RpcDisable();
                StartCoroutine(DelayedDestroy(1));

                break;
            }
        }
    }

    [ClientRpc]
    private void SpawnParticleRpc(Vector3 position, Quaternion rotation)
    {
        Instantiate(bulletParticle, position, rotation);
    }

    [ClientRpc]
    private void SpawnBulletHoleRpc(Vector3 position, Quaternion rotation)
    {
        Instantiate(bulletHole, position, rotation);
    }

    [ClientRpc]
    private void RpcDisable()
    {
        GetComponent<MeshRenderer>().enabled = false;
        isKinematic = true;
    }

    [TargetRpc]
    private void RpcApplyForce(NetworkIdentity target, Vector3 direction)
    {
        target
            .GetComponent<Rigidbody>()
            .AddForceAtPosition(5 * damage * direction, transform.position, ForceMode.Impulse);
    }

    private IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        NetworkServer.Destroy(gameObject);
    }
}
