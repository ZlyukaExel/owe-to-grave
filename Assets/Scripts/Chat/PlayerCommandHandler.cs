using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(ChatManager))]
public class PlayerCommandHandler : NetworkBehaviour
{
    public static PlayerCommandHandler localPlayerInstance;
    private readonly Dictionary<string, Action<string[], string>> commands = new(
        StringComparer.OrdinalIgnoreCase
    );
    private ChatManager chatManager;

    public override void OnStartLocalPlayer()
    {
        localPlayerInstance = this;
        chatManager = GetComponent<ChatManager>();
        RegisterCommands();
    }

    private void RegisterCommands()
    {
        commands.Add("/a", CommandAll);
        commands.Add("/aoe", CommandAllExceptOwner);
        commands.Add("/ignore", CommandIgnore);
        commands.Add("/tp", CommandTeleport);
        commands.Add("/kill", CommandKill);
        commands.Add("/kick", CommandKick);
        commands.Add("/find", CommandFind);
        commands.Add("/damage", CommandDamage);
        commands.Add("/help", CommandHelp);
    }

    public bool TryProcessCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.StartsWith("/"))
            return false;

        string[] args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string commandName = args[0].ToLower();

        if (commands.ContainsKey(commandName))
        {
            commands[commandName].Invoke(args, input);
            return true;
        }

        return false;
    }

    private void CommandAll(string[] args, string rawInput)
    {
        if (args.Length < 2)
        {
            Debug.Log("Usage: /a [message]");
            return;
        }
        string message = rawInput.Substring(args[0].Length).Trim();
        chatManager.SendToEveryone(
            $"<color=green>[{GetComponent<Player>().entityName}]: </color>" + message,
            true
        );
    }

    private void CommandAllExceptOwner(string[] args, string rawInput)
    {
        if (args.Length < 2)
        {
            Debug.Log("Usage: /aoe [message]");
            return;
        }
        string message = rawInput.Substring(args[0].Length).Trim();
        chatManager.SendToEveryone(
            $"<color=green>[{GetComponent<Player>().entityName}]: </color>" + message,
            false
        );
    }

    private void CommandIgnore(string[] args, string rawInput) =>
        Debug.Log("<color=yellow>Command /ignore executed</color>");

    private void CommandHelp(string[] args, string rawInput) =>
        Debug.Log(
            "<color=yellow>Commands: /a, /aoe, /ignore, /tp [x y z], /kill [name], /damage [name] [amount], /kick [name], /find [name]</color>"
        );

    private void CommandFind(string[] args, string rawInput)
    {
        if (args.Length < 2)
        {
            Debug.Log("Usage: /find [object name]");
            return;
        }

        GameObject target = GameObject.Find(args[1]);
        if (target != null)
            Debug.Log(
                $"<color=green>Object '{target.name}' foumd on: {target.transform.position}</color>"
            );
        else
            Debug.Log($"<color=red>Объект '{args[1]}' no one.</color>");
    }

    private void CommandTeleport(string[] args, string rawInput)
    {
        if (args.Length < 4)
        {
            Debug.Log("Usage: /tp [x] [y] [z]");
            return;
        }
        if (
            float.TryParse(args[1], out float x)
            && float.TryParse(args[2], out float y)
            && float.TryParse(args[3], out float z)
        )
            CmdTeleport(new Vector3(x, y, z));
    }

    private void CommandKill(string[] args, string rawInput)
    {
        if (args.Length < 2)
        {
            Debug.Log("Usage: /kill [object name]");
            return;
        }
        CmdKill(args[1]);
    }

    private void CommandDamage(string[] args, string rawInput)
    {
        if (args.Length < 3)
        {
            Debug.Log("Usage: /damage [name] [amount]. Negative = heal.");
            return;
        }
        if (int.TryParse(args[2], out int amount))
            CmdDamage(args[1], amount);
        else
            Debug.Log("<color=red>Value must be a number!</color>");
    }

    private void CommandKick(string[] args, string rawInput)
    {
        if (args.Length < 2)
        {
            Debug.Log("Usage: /kick [player name]");
            return;
        }

        if (!isServer)
        {
            Debug.Log("<color=red>poshel tbl /kick!</color>");
            return;
        }

        CmdKick(args[1]);
    }

    [Command]
    private void CmdTeleport(Vector3 pos)
    {
        transform.position = pos;
    }

    [Command]
    private void CmdKill(string targetName)
    {
        ApplyDamageToNetworkEntity(targetName, 9999999);
    }

    [Command]
    private void CmdDamage(string targetName, int amount)
    {
        ApplyDamageToNetworkEntity(targetName, amount);
    }

    [Command(requiresAuthority = false)]
    private void CmdKick(string targetName)
    {
        if (!isServer)
            return;

        foreach (var player in ServerManager.Instance.players)
        {
            if (player.connectionToClient.nickname.Equals(targetName))
            {
                TargetOnKicked(player.connectionToClient);
                break;
            }
        }
    }

    [TargetRpc]
    public void TargetOnKicked(NetworkConnection target)
    {
        NetworkManager.singleton.StopHost();
    }

    private void ApplyDamageToNetworkEntity(string targetName, int amount)
    {
        foreach (NetworkIdentity identity in NetworkServer.spawned.Values)
        {
            if (identity.gameObject.name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
            {
                NetworkHitpoints health = identity.GetComponent<NetworkHitpoints>();

                if (health == null)
                {
                    HitPointsSet hpSet = identity.GetComponent<HitPointsSet>();
                    if (hpSet != null)
                        health = hpSet.GetHp();
                }

                if (health != null)
                {
                    if (amount > 0)
                    {
                        DamageInfo info = new DamageInfo(amount, DamageType.Insult, netIdentity);
                        health.Damage(info);
                    }
                    else if (amount < 0)
                    {
                        health.Heal(Mathf.Abs(amount));
                    }

                    Debug.Log($"Successfully applied {amount} damage to {targetName}");
                }
                else
                {
                    Debug.LogWarning($"Entity '{targetName}' missing NetworkHitpoints");
                }
                return;
            }
        }
        Debug.LogWarning($"Network entity '{targetName}' not found on server");
    }
}
