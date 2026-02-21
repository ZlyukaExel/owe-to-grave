using Mirror;

public static class NetworkUtils
{
    [Server]
    public static void ClearOwner(this NetworkIdentity targetObject)
    {
        if (targetObject.connectionToClient != null)
            targetObject.RemoveClientAuthority();
    }

    [Server]
    public static void ServerChangeLayer(this NetworkIdentity targetObject, int newLayer)
    {
        if (!targetObject) return;
        targetObject.gameObject.layer = newLayer;
    }
}