using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float bulletSpeed = 10,
        spread = 1;
    public Transform bulletSpawnTransform;
    public NetworkSwapGameObjects swapObjects;
    public Animator animator;
}
