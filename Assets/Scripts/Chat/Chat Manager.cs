using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public TMP_InputField chat;

    [HideInInspector]
    public TMP_InputField inputField;

    private static event Action<ChatEntry> OnMessage;

    [SerializeField]
    private InputActionReference submitAction;

    private readonly List<ChatEntry> history = new();
    private const int MaxHistoryCount = 100;
    private int historyIndex = -1;

    private void Start()
    {
        player = GetComponent<Player>();

        Transform chatTransform = GetComponent<PlayerLinks>().ui.Find("Chat Ui/Chat");
        chat = chatTransform.Find("Scroll View/Viewport/Text").GetComponent<TMP_InputField>();
        inputField = chatTransform.Find("Input Field").GetComponent<TMP_InputField>();
        inputField.onEndEdit.AddListener(EditInput);

        var navAction = inputField.GetComponent<ActionByNavigation>();
        navAction.onNavDown.AddListener(SelectNext);
        navAction.onNavUp.AddListener(SelectPrevious);

        OnMessage += AddLocalMessage;

        Application.logMessageReceived += HandleLog;
    }

    [ClientCallback]
    private void OnDestroy()
    {
        OnMessage -= AddLocalMessage;
        Application.logMessageReceived -= HandleLog;
    }

    public void AddLocalMessage(ChatEntry entry)
    {
        if (!chat)
            return;

        if (history.Count > 0)
        {
            var last = history[history.Count - 1];
            if (
                last.sender == entry.sender
                && last.content == entry.content
                && last.color == entry.color
            )
            {
                last.stackedTimes++;
                history[history.Count - 1] = last;
                UpdateChatUI();
                return;
            }
        }

        if (history.Count >= MaxHistoryCount)
            history.RemoveAt(0);

        history.Add(entry);
        UpdateChatUI();
    }

    private void UpdateChatUI()
    {
        chat.text = string.Concat(history.Select(FormatMessage));
    }

    private string FormatMessage(ChatEntry e)
    {
        string stackSuffix = e.stackedTimes > 1 ? $" ({e.stackedTimes})" : "";

        if (string.IsNullOrEmpty(e.sender))
        {
            return $"<color={e.color}>{e.content}{stackSuffix}</color>\n";
        }

        return $"<align=left><color={e.color}>[{e.sender}]:</color> {e.content}{stackSuffix}</align>\n";
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string color = GetLogTypeColor(type);
        string content =
            (type == LogType.Error || type == LogType.Warning)
                ? $"{logString}\n<size=80%>{stackTrace}</size>"
                : logString;

        AddLocalMessage(new ChatEntry("", content, color));
    }

    private string GetLogTypeColor(LogType type)
    {
        return type switch
        {
            LogType.Error => "red",
            LogType.Warning => "yellow",
            _ => "#C0C0C0",
        };
    }

    private void EditInput(string message)
    {
#if UNITY_STANDALONE
        if (!submitAction.action.triggered)
            return;
#endif
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (message.StartsWith("/"))
        {
            AddLocalMessage(new ChatEntry("Command", message, "blue"));
            if (PlayerCommandHandler.localPlayerInstance != null)
                PlayerCommandHandler.localPlayerInstance.ProcessCommands(message);
        }
        else
        {
            CmdSendToEveryone(new ChatEntry(player.entityName, message, "green"), true);
        }

        historyIndex = -1;

        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    private void SelectNext()
    {
        if (history.Count == 0 || historyIndex == -1)
            return;

        historyIndex++;

        if (historyIndex >= history.Count)
        {
            historyIndex = -1;
            inputField.text = string.Empty;
        }
        else
        {
            SetInputFromHistory();
        }
    }

    private void SelectPrevious()
    {
        if (history.Count == 0)
            return;

        if (historyIndex == 0)
            return;

        if (historyIndex == -1)
        {
            historyIndex = history.Count - 1;
        }
        else
        {
            historyIndex--;
        }

        SetInputFromHistory();
    }

    private void SetInputFromHistory()
    {
        inputField.text = history[historyIndex].content;
        StartCoroutine(CaretCor());
    }

    private IEnumerator CaretCor()
    {
        inputField.MoveTextStart(false);
        yield return new WaitForEndOfFrame();
        inputField.MoveTextEnd(false);
    }

    [Command(requiresAuthority = false)]
    public void CmdSendToEveryone(ChatEntry entry, bool includeOwner)
    {
        if (includeOwner)
            RpcSendToEveryone(entry);
        else
            RpcSendToEveryoneExceptOwner(entry);
    }

    [ClientRpc]
    private void RpcSendToEveryone(ChatEntry entry) => OnMessage?.Invoke(entry);

    [ClientRpc(includeOwner = false)]
    private void RpcSendToEveryoneExceptOwner(ChatEntry entry) => OnMessage?.Invoke(entry);

    [TargetRpc]
    public void AddMessage(ChatEntry entry) => OnMessage?.Invoke(entry);
}

[Serializable]
public struct ChatEntry
{
    public string sender;
    public string content;
    public string color;
    public int stackedTimes;

    public ChatEntry(string sender, string content, string color = "#C0C0C0")
    {
        this.sender = sender;
        this.content = content;
        this.color = color;
        stackedTimes = 1;
    }
}
