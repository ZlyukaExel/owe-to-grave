using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerLinks))]
public class ChatManager : NetworkBehaviour
{
    private Player player;

    [HideInInspector]
    public TMP_Text chat;

    [HideInInspector]
    public TMP_InputField inputField;
    private static event Action<string> OnMessage;

    [SerializeField]
    private InputActionReference submitAction;

    private void Start()
    {
        player = GetComponent<Player>();

        Transform chat = GetComponent<PlayerLinks>().ui.Find("Chat Ui/Chat");
        this.chat = chat.Find("Scroll View/Viewport/Text").GetComponent<TMP_Text>();
        inputField = chat.Find("Input Field").GetComponent<TMP_InputField>();

        inputField.onEndEdit.AddListener(EditInput);
        OnMessage += AddLocalMessage;
        Application.logMessageReceived += HandleLog;
    }

    // On disconnecting
    [ClientCallback]
    private void OnDestroy()
    {
        OnMessage -= AddLocalMessage;
    }

    private void AddLocalMessage(string message)
    {
        if (!chat)
            return;
        if (chat.text.EndsWith(message))
            return;
        if (!string.IsNullOrEmpty(chat.text))
            chat.text += "\n";
        chat.text += message;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string color = GetLogTypeColor(type);
        string prefix = $"<align=center><color={color}><i>";
        string postfix = "</color></i></align>";

        if (logString.StartsWith("/"))
            HandleCommand(prefix, logString, postfix);
        else
        {
            if (type == LogType.Error || type == LogType.Warning)
                AddLocalMessage(prefix + logString + "/n" + stackTrace + postfix);
            else
                AddLocalMessage(prefix + logString + postfix);
        }
    }

    private string GetLogTypeColor(LogType type)
    {
        return type switch
        {
            LogType.Error => "red",
            LogType.Warning => "yellow",
            _ => "grey",
        };
    }

    private void HandleCommand(string prefix, string command, string postfix)
    {
        if (command.Contains(" "))
        {
            string[] parts = command.Split(" ", 2);

            switch (parts[0])
            {
                case "/a":
                {
                    SendToEveryone(prefix + parts[1] + postfix);
                    break;
                }
                case "/aeo":
                {
                    CmdSendToEveryone(prefix + parts[1] + postfix, false);
                    break;
                }
                case "/ignore":
                {
                    return;
                }
                default:
                {
                    Debug.Log($"Command {parts[0]} is unsupported");
                    break;
                }
            }
        }
        else
            Debug.Log("Command has no arguments");
    }

    public void EditInput(string message)
    {
#if UNITY_STANDALONE
        if (!submitAction.action.triggered)
            return;
#endif
        if (string.IsNullOrWhiteSpace(message))
            return;

        SendToEveryone($"<color=green>[{player.entityName}]: </color>" + message);
        inputField.text = string.Empty;
    }

    private void SendToEveryone(string message, bool includeOwner = true)
    {
        CmdSendToEveryone(message, includeOwner);
    }

    [Command(requiresAuthority = false)]
    private void CmdSendToEveryone(string message, bool includeOwner)
    {
        if (includeOwner)
            RpcSendToEveryone(message);
        else
            RpcSendToEveryoneExceptOwner(message);
    }

    [ClientRpc]
    private void RpcSendToEveryone(string message)
    {
        OnMessage?.Invoke($"{message}");
    }

    [ClientRpc(includeOwner = false)]
    private void RpcSendToEveryoneExceptOwner(string message)
    {
        OnMessage?.Invoke($"{message}");
    }

    [TargetRpc]
    public void AddMessage(string message)
    {
        Debug.Log(message);
    }
}
