using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private WeaponProperties weapon;
    private NetworkIdentity shooter;

    [HideInInspector]
    public float lifeTime = 2;

    [HideInInspector]
    public int piercing = 0;

    private readonly HashSet<uint> ignorePlayers = new();

    [SerializeField]
    private bool isKinematic = false;

    [SerializeField]
    private LayerMask obstaclesLayers;

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

        // If PVP enabled, handle Players layer too
        if (ServerManager.Instance.pvpEnabled)
            obstaclesLayers |= 1 << LayerMask.NameToLayer("Player");

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
        float step = weapon.bulletSpeed * Time.deltaTime;
        foreach (
            var hit in Physics.RaycastAll(
                transform.position,
                transform.forward,
                step,
                obstaclesLayers
            )
        )
        {
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

    public void Initiate(WeaponProperties properties, NetworkIdentity shooter)
    {
        weapon = properties;
        ignorePlayers.Add(shooter.netId);
    }

    private void HandleHit(RaycastHit hit)
    {
        if (!isServer)
            return;

        // Deal damage
        if (hit.collider.GetComponent<HitPoint>() is HitPoint hp)
        {
            if (hp.GetHp() == null)
                return;

            // Single bullet can't hit twice
            uint playerId = hp.GetHp().netId;
            if (ignorePlayers.Contains(playerId))
                return;
            ignorePlayers.Add(playerId);

            DamageInfo damageInfo = new(
                weapon.damage,
                weapon.critMultiplier,
                DamageType.Bullet,
                shooter
            );
            hp.Damage(damageInfo);

            // Destroying bullet
            if (piercing <= 0)
            {
                // This was made so cuz players should be able to hide behind teamates
                RpcDisable();
                StartCoroutine(DelayedDestroy(1));
            }
            // Reducing piercing
            else
                piercing--;
        }
        // Destroy destroyable
        else if (hit.collider.GetComponent<NetworkActive>() is NetworkActive destroyable)
        {
            destroyable.SetActive(false);
        }
        // Create particles and destroy bullet
        else
        {
            // Destroy destroyables around the contact point
            foreach (Collider colliderHit in Physics.OverlapSphere(hit.point, 0.1f))
            {
                if (
                    colliderHit.CompareTag("Destroyable")
                    && colliderHit.TryGetComponent(out destroyable)
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
            SpawnParticleRpc(hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));

            // Waiting for particles to be created and then destroying bullet
            RpcDisable();
            StartCoroutine(DelayedDestroy(1));
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
            .AddForceAtPosition(
                5 * weapon.damage * direction,
                transform.position,
                ForceMode.Impulse
            );
    }

    private IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        NetworkServer.Destroy(gameObject);
    }
}
