using System;
using Mirror;
using UnityEngine;

public class ServerInfo : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnPvpChanged))]
    public bool isPvpOn = false;
    public readonly SyncList<NetworkIdentity> players = new();
    public static ServerInfo Instance { get; private set; }

    public event Action<NetworkIdentity> OnPlayerAdded;
    public event Action<NetworkIdentity> OnPlayerRemoved;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        // If pvp is off, bullets ignore players
        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Projectiles"),
            LayerMask.NameToLayer("Player"),
            !isPvpOn
        );
        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Projectiles"),
            LayerMask.NameToLayer("Ignore Raycast"),
            !isPvpOn
        ); // Local player's layer

        // Debug
        // foreach (var spawnable in NetworkServer.spawned)
        // {
        //     print("Object netId: " + spawnable.Key + ", netIdentity: " + spawnable.Value);
        // }
    }

    [Server]
    public void AddPlayer(NetworkIdentity networkIdentity)
    {
        if (networkIdentity == null)
            return;

        if (!players.Contains(networkIdentity))
        {
            players.Add(networkIdentity);
            RpcOnAdded(networkIdentity);
        }
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
    public void SetPvp(bool value)
    {
        isPvpOn = value;
    }

    private void OnPvpChanged(bool oldValue, bool newValue)
    {
        // If pvp is off, bullets ignore players
        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Projectiles"),
            LayerMask.NameToLayer("Player"),
            !newValue
        );
        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Projectiles"),
            LayerMask.NameToLayer("Ignore Raycast"),
            !newValue
        ); // It's local player's layer

        if (newValue)
            Debug.Log("PVP enabled!");
        else
            Debug.Log("PVP disabled!");
    }
}
