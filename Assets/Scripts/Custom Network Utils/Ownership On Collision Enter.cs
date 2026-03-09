using Mirror;
using UnityEngine;

public class OwnershipOnCollisionEnter : NetworkBehaviour
{
    private int itemsLayerId;

    void Start()
    {
        itemsLayerId = LayerMask.NameToLayer("Items");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isLocalPlayer)
            return;

        // Change item owner so he calculates physics now
        if (
            collision.gameObject.layer == itemsLayerId
            && collision.transform.TryGetComponent(out NetworkIdentity networkIdentity)
        )
            CmdRequestOwnership(networkIdentity);
    }

    [Command]
    private void CmdRequestOwnership(NetworkIdentity itemNetId)
    {
        if (itemNetId != null && itemNetId.connectionToClient != connectionToClient)
        {
            itemNetId.RemoveClientAuthority();
            itemNetId.AssignClientAuthority(connectionToClient);
        }
    }
}
