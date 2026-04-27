using System;
using Mirror;
using UnityEngine;

public class ServerManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnPvpChanged))]
    public bool pvpEnabled = false;
    public readonly SyncList<NetworkIdentity> players = new();
    public static ServerManager Instance { get; private set; }

    public event Action<NetworkIdentity> OnPlayerAdded;
    public event Action<NetworkIdentity> OnPlayerRemoved;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    [Server]
    public void AddPlayer(NetworkIdentity networkIdentity)
    {
        if (networkIdentity == null || players.Contains(networkIdentity))
            return;

        // Greeting player
        networkIdentity
            .GetComponent<ChatManager>()
            .AddMessage("Welcome to the server, " + networkIdentity.connectionToClient.nickname);

        // Informing other players
        foreach (var player in players)
        {
            player
                .GetComponent<ChatManager>()
                .AddMessage(networkIdentity.connectionToClient.nickname + " has joined");
        }

        // Add player
        players.Add(networkIdentity);
        RpcOnAdded(networkIdentity);
    }

    [ClientRpc]
    private void RpcOnAdded(NetworkIdentity networkIdentity)
    {
        OnPlayerAdded?.Invoke(networkIdentity);
    }

    [Server]
    public void RemovePlayer(NetworkIdentity networkIdentity)
    {
        if (players.Contains(networkIdentity))
        {
            players.Remove(networkIdentity);
            RpcOnRemoved(networkIdentity);
        }
    }

    [ClientRpc]
    private void RpcOnRemoved(NetworkIdentity networkIdentity)
    {
        OnPlayerRemoved?.Invoke(networkIdentity);
    }

    [Server]
    public void SetPvpEnabled(bool enabled)
    {
        pvpEnabled = enabled;
    }

    private void OnPvpChanged(bool oldValue, bool newValue)
    {
        if (newValue)
            Debug.Log("PVP enabled!");
        else
            Debug.Log("PVP disabled!");
    }
}
