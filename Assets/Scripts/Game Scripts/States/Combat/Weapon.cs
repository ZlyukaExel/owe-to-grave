using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ObjectDisable))]
public class Weapon : MonoBehaviour
{
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

    public WeaponData data;
    public WeaponProperties properties => data.properties;

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
        current ??= GetComponent<ObjectDisable>();
        hidden.SetActive(!primary);
        current.SetActive(primary);
    }

    public void DeactivateBoth()
    {
        hidden.SetActive(false);
        GetComponent<ObjectDisable>().SetActive(false);
    }
}
