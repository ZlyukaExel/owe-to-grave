using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(NetworkHitpoints))]
public class Entity : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnNicknameChanged))]
    public string entityName;

    [SerializeField]
    private TMP_Text textTag;

    [SerializeField]
    private NetworkHitpoints hitpoints;

    // Register onStartLocalPlayer cuz PlayerPrefs needed
    public override void OnStartLocalPlayer()
    {
        hitpoints = GetComponent<NetworkHitpoints>();
        RegisterPlayer(PlayerPrefs.GetString("Nickname"));
    }

    public override void OnStopServer()
    {
        Debug.Log($"/a {entityName} has left");
        ServerInfo.Instance.RemovePlayer(netIdentity);
    }

    [Command]
    private void RegisterPlayer(string nickname)
    {
        ChangeNickname(nickname);
    }

    [Command(requiresAuthority = false)]
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
    }

    private void OnNicknameChanged(string oldName, string newName)
    {
        gameObject.name = newName;
        if (textTag)
            textTag.text = newName;

        if (isOwned)
            AddToServerInfo();
    }

    [Command]
    private void AddToServerInfo()
    {
        ServerInfo.Instance.AddPlayer(netIdentity);
    }
}
