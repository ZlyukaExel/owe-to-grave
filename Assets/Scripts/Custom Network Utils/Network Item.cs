using Mirror;
using UnityEngine;

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
            if (collision.gameObject.TryGetComponent(out NetworkHitpoints hp))
            {
                if (Time.time - lastHitTime > hitCooldown)
                {
                    hp.Damage(velocity);

                    // Audio SFX
                    if (collision.relativeVelocity.magnitude > 3)
                        audioSource.Play();

                    lastHitTime = Time.time;
                }
            }
        }
    }
}
