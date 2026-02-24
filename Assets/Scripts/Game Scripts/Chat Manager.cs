using System;
using Mirror;
using TMPro;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    private Entity playerInfo;

    [HideInInspector]
    public TMP_Text chat;

    [HideInInspector]
    public TMP_InputField inputField;
    private static event Action<string> OnMessage;

    private void Start()
    {
        playerInfo = GetComponent<Entity>();

        Transform chat = GetComponent<Links>().ui.Find("Chat Ui/Chat");
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
        string message = $"<align=center><color={color}><i>{logString}";
        string postfix = "</color></i></align>";

        if (logString.StartsWith("/"))
            HandleCommand(logString, message + postfix);
        else
        {
            if (type == LogType.Error || type == LogType.Warning)
                AddLocalMessage(message + "/n" + stackTrace + postfix);
            else
                AddLocalMessage(message + postfix);
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

    private void HandleCommand(string command, string message)
    {
        if (command.Contains(" "))
        {
            string[] parts = command.Split(" ", 2);

            switch (parts[0])
            {
                case "/a":
                {
                    SendToEveryone(message);
                    break;
                }
                case "/aeo":
                {
                    CmdSendToEveryone(message, false);
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
        if (!Input.GetKeyDown(KeyCode.Return))
            return;
#endif
        if (string.IsNullOrWhiteSpace(message))
            return;

        SendToEveryone($"<color=green>[{playerInfo.entityName}]: </color>" + message);
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
