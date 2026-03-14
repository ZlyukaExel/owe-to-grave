using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ObjectDisable))]
public class Weapon : MonoBehaviour
{
    public WeaponProperties properties;

    [SerializeField]
    private Transform bulletSpawnTransform;
    public Animator animator;

    [SerializeField]
    private ParticleSystem particle;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private ObjectDisable hidden;
    private ObjectDisable current;
    public UnityEvent<Vector3> onShot;

    void Awake()
    {
        current = GetComponent<ObjectDisable>();
    }

    public void SetShooting(bool isShooting)
    {
        animator.SetBool("isShooting", isShooting);
    }

    public GameObject GetHidden() => hidden.gameObject;

    public ParticleSystem GetFlash() => particle;

    public void SpawnBullet()
    {
        onShot.Invoke(bulletSpawnTransform.position);
        audioSource.Play();
        particle.Play();
    }

    public void Activate() => Activate(true);

    public void Activate(bool primary)
    {
        hidden.SetActive(!primary);
        current.SetActive(primary);
    }

    public void DeactivateBoth()
    {
        hidden.SetActive(false);
        gameObject.SetActive(false);
    }
}

[System.Serializable]
public class WeaponProperties
{
    public float damage = 20,
        critMultiplier = 3,
        bulletSpeed = 10,
        spread = 1;
    public int piercing = 0;
}
