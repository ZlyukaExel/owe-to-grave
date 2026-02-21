using UnityEngine;

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
        if (!l.humanoid.isGrounded)
            return;
    }
}
