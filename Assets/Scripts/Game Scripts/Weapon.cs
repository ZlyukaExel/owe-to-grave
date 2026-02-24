using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponProperties properties;
    public Transform bulletSpawnTransform;
    public NetworkSwapGameObjects swapObjects;
    public Animator animator;
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
