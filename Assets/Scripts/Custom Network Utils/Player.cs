using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

public class Player : Entity
{
    // Register onStartLocalPlayer cuz PlayerPrefs needed
    public override void OnStartLocalPlayer()
    {
        ChangeNickname(PlayerPrefs.GetString("Nickname"));
    }

    public override void OnStopServer()
    {
        Debug.Log($"/a {entityName} has left");
        ServerManager.Instance.RemovePlayer(netIdentity);
    }

    [Command]
    public void ChangeNickname(string nickname)
    {
        // Changing name of the server if host changed nickname
        if (isServer && NetworkDiscovery.Instance)
            NetworkDiscovery.Instance.ServerName = nickname;

        // Creating unique nickname
        string uniqueNickname = nickname;
        int suffix = 1;

        // Creating list of existing nicknames
        HashSet<string> existingNicknames = new();
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn != connectionToClient && conn.nickname != null)
            {
                existingNicknames.Add(conn.nickname);
            }
        }

        // Searching for a unique nickname
        while (existingNicknames.Contains(uniqueNickname))
        {
            uniqueNickname = $"{nickname} [{suffix}]";
            suffix++;
        }

        // Changing nickname on sever
        entityName = uniqueNickname;
        connectionToClient.nickname = uniqueNickname;

        ServerManager.Instance.AddPlayer(netIdentity);
    }
}
