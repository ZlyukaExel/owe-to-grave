using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkAudioSource))]
public class NetworkItem : NetworkBehaviour
{
    private NetworkAudioSource audioSource;
    private Rigidbody rb;
    private float lastHitTime = -999,
        hitCooldown = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<NetworkAudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isOwned)
            return;

        //audioSource.CmdPlay();

        float velocity =
            rb.linearVelocity.magnitude * 5 /* rb.mass / 100*/
            - 10;

        if (velocity > 0)
        {
            // Damage players
            if (collision.collider.TryGetComponent(out HitPoint hp))
            {
                if (Time.time - lastHitTime > hitCooldown)
                {
                    DamageInfo damageInfo = new DamageInfo(velocity, 1, DamageType.Item, null);
                    hp.Damage(damageInfo);

                    // Audio SFX
                    if (collision.relativeVelocity.magnitude > 3)
                        audioSource.Play();

                    lastHitTime = Time.time;
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void SetVelocity(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    [Command(requiresAuthority = false)]
    public void AddForce(Vector3 velocity)
    {
        rb.AddForce(velocity);
    }
}
