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
        commands.Add("/addxp", CommandAddXp);
    }

    public bool ProcessCommands(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var commandLines = input.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in commandLines)
        {
            string trimmedLine = line.Trim();
            if (!ProcessCommand(trimmedLine))
                return false;
        }

        return true;
    }

    public bool ProcessCommand(string input)
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

        chatManager.AddLocalMessage(
            new ChatEntry("Server", $"Command {commandName} not found", "red")
        );
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
        chatManager.CmdSendToEveryone(
            new ChatEntry(GetComponent<Player>().entityName, message, "green"),
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
        chatManager.CmdSendToEveryone(
            new ChatEntry(GetComponent<Player>().entityName, message, "green"),
            false
        );
    }

    private void CommandIgnore(string[] args, string rawInput) { }

    private void CommandHelp(string[] args, string rawInput) =>
        Debug.Log(
            "<color=yellow>Commands: /a, /aoe, /ignore, /tp [x y z], /kill [name], /damage [name] [amount], /kick [name], /find [name]</color>, /addxp [stat] [amount]"
        );

    private void CommandFind(string[] args, string rawInput)
    {
        if (args.Length < 2)
        {
            Debug.Log("Usage: /find [entity name]");
            return;
        }

        GameObject target = GameObject.Find(args[1]);
        if (target != null)
            Debug.Log(
                $"<color=green>Entity '{target.name}' found in: {target.transform.position}</color>"
            );
        else
            Debug.Log($"<color=red>Entity '{args[1]}' not found</color>");
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
            Debug.Log("Usage: /kill [entity name]");
            return;
        }
        CmdKill(args[1]);
    }

    private void CommandDamage(string[] args, string rawInput)
    {
        if (args.Length < 3)
        {
            Debug.Log("Usage: /damage [name] [amount]");
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
                if (!identity.TryGetComponent<NetworkHitpoints>(out var health))
                {
                    if (identity.TryGetComponent<HitPointsSet>(out var hpSet))
                        health = hpSet.GetHp();
                }

                if (health != null)
                {
                    DamageInfo info = new(amount, DamageType.GodPower, netIdentity);
                    health.Damage(info);
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

    private void CommandAddXp(string[] args, string rawInput)
    {
        if (args.Length < 3)
        {
            Debug.Log("Usage: /addxp [stat_name] [amount]");
            return;
        }

        if (int.TryParse(args[2], out int amount))
        {
            var statsManager = GetComponent<NetworkCharacteristics>();

            if (statsManager != null)
            {
                statsManager.CmdAddSkillExperience(args[1], amount);
                Debug.Log(
                    $"<color=green>Requesting server to add {amount} XP to {args[1]}...</color>"
                );
            }
            else
            {
                Debug.Log("<color=red>NetworkPlayerManager component not found on player!</color>");
            }
        }
        else
        {
            Debug.Log("<color=red>XP amount must be a number!</color>");
        }
    }
}
