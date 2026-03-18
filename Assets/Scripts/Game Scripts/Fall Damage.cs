using UnityEngine;

public class FallDamage : MonoBehaviour
{
    private PlayerLinks l;
    private NetworkHitpoints hp;

    void Start()
    {
        l = GetComponent<PlayerLinks>();
        hp = GetComponent<NetworkHitpoints>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!l.movement.isGrounded)
            return;
    }
}
