using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    [SyncVar]
    public int playerId = -1;

    [SyncVar(hook = nameof(OnNicknameChanged))]
    public string nickname;

    [SerializeField]
    private TMP_Text textTag;

    private bool nicknameSet = false;

    // Register onStartLocalPlayer cuz PlayerPrefs needed
    public override void OnStartLocalPlayer()
    {
        RegisterPlayer(PlayerPrefs.GetString("Nickname"));
    }

    public override void OnStopServer()
    {
        Debug.Log($"/a {nickname} has left");
        ServerInfo.Instance.RemovePlayer(netIdentity);
    }

    [Command]
    private void RegisterPlayer(string nickname)
    {
        playerId = connectionToClient.connectionId;
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
        this.nickname = uniqueNickname;
        connectionToClient.nickname = uniqueNickname;
    }

    private void OnNicknameChanged(string oldName, string newName)
    {
        gameObject.name = textTag.text = newName;

        // First nickname change prints welcomeMessage
        if (isOwned && !nicknameSet)
        {
            GetComponent<ChatManager>().PrintWelcomeMessage(newName);
            AddInServerInfo();
            nicknameSet = true;
        }
    }

    [Command]
    private void AddInServerInfo()
    {
        ServerInfo.Instance.AddPlayer(netIdentity);
    }
}
