using Mirror;
using UnityEngine;

public class BulletSpawner : NetworkBehaviour
{
    private NetworkAudioSource audioSource;

    [SerializeField]
    private Humanoid humanoid;

    [SerializeField]
    private ParticleSystem particle;

    void Start()
    {
        audioSource = GetComponent<NetworkAudioSource>();
    }

    public void Shoot()
    {
        if (!isOwned)
            return;

        if (humanoid.state is Combat combat)
        {
            combat.SpawnBullet();
            audioSource.Play();
            particle.Play();
        }
    }
}
