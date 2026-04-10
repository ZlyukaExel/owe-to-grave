using UnityEngine;

[RequireComponent(typeof(Links))]
[RequireComponent(typeof(NetworkHitpoints))]
public class FallDamage : MonoBehaviour
{
    private Links l;
    private NetworkHitpoints hp;

    void Start()
    {
        l = GetComponent<Links>();
        hp = GetComponent<NetworkHitpoints>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!l.movement.isGrounded)
            return;
    }
}
