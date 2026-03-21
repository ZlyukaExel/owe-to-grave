using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushItems : NetworkBehaviour
{
    private int itemsLayerId;
    private Rigidbody rb;

    void Start()
    {
        itemsLayerId = LayerMask.NameToLayer("Items");
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (
            (isOwned || isServer && connectionToClient == null)
            && collision.gameObject.layer == itemsLayerId
            && collision.rigidbody.TryGetComponent(out NetworkItem networkItem)
        )
            networkItem.AddForce(rb.linearVelocity * 0.5f);
    }
}
