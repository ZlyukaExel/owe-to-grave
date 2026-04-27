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

        Transform chatTransform = GetComponent<PlayerLinks>().ui.Find("Chat Ui/Chat");
        chat = chatTransform.Find("Scroll View/Viewport/Text").GetComponent<TMP_Text>();
        inputField = chatTransform.Find("Input Field").GetComponent<TMP_InputField>();

        inputField.onEndEdit.AddListener(EditInput);
        OnMessage += AddLocalMessage;

        Application.logMessageReceived += HandleLog;
    }

    [ClientCallback]
    private void OnDestroy()
    {
        OnMessage -= AddLocalMessage;
        Application.logMessageReceived -= HandleLog;
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

        if (type == LogType.Error || type == LogType.Warning)
            AddLocalMessage(prefix + logString + "\n" + stackTrace + postfix);
        else if (type == LogType.Log)
            AddLocalMessage(prefix + logString + postfix);
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

    public void EditInput(string message)
    {
#if UNITY_STANDALONE
        if (!submitAction.action.triggered)
            return;
#endif
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (message.StartsWith("/"))
        {
            if (
                PlayerCommandHandler.localPlayerInstance != null
                && PlayerCommandHandler.localPlayerInstance.TryProcessCommand(message)
            )
            {
                inputField.text = string.Empty;
                return;
            }
        }
        else
        {
            SendToEveryone($"<color=green>[{player.entityName}]: </color>" + message);
        }

        inputField.text = string.Empty;
    }

    public void SendToEveryone(string message, bool includeOwner = true)
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
        OnMessage?.Invoke(message);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcSendToEveryoneExceptOwner(string message)
    {
        OnMessage?.Invoke(message);
    }

    [TargetRpc]
    public void AddMessage(string message)
    {
        Debug.Log(message);
    }
}
