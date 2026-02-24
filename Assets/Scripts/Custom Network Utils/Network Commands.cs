using Mirror;
using UnityEngine;

public class NetworkCommands : NetworkBehaviour
{
    [SerializeField]
    private GameObject bulletPrefab;

    public void RequestSpawnBullet(Vector3 position, Vector3 direction, WeaponProperties properties)
    {
        if (!isLocalPlayer)
            return;

        CmdSpawnBullet(position, direction, properties);
    }

    [Command]
    private void CmdSpawnBullet(Vector3 position, Vector3 direction, WeaponProperties properties)
    {
        // Spawning bullet on server
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.LookRotation(direction));
        // Ignore shooter
        bullet.GetComponent<Bullet>().Initiate(properties, netId);
        NetworkServer.Spawn(bullet, connectionToClient);
    }

    [Command]
    public void CmdDestroy(GameObject target)
    {
        NetworkServer.Destroy(target);
    }

    [Command]
    public void CmdRequestOwnership(NetworkIdentity itemNetId)
    {
        if (itemNetId != null && itemNetId.connectionToClient != connectionToClient)
        {
            itemNetId.RemoveClientAuthority();
            itemNetId.AssignClientAuthority(connectionToClient);
        }
    }

    [Command]
    public void CmdRequestChangeLayer(NetworkIdentity itemNetId, int layer)
    {
        itemNetId.ServerChangeLayer(layer);
        RpcChangeLayer(itemNetId, layer);
    }

    [ClientRpc]
    private void RpcChangeLayer(NetworkIdentity targetObject, int newLayer)
    {
        targetObject.gameObject.layer = newLayer;
    }
}
