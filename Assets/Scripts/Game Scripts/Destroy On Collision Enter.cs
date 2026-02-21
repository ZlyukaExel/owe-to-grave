using Mirror;
using UnityEngine;

public class DestroyOnCollisionEnter : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Player")
            DestroyObject();
    }

    [Command(requiresAuthority = false)]
    private void DestroyObject()
    {
        NetworkServer.Destroy(gameObject);
    }
}
