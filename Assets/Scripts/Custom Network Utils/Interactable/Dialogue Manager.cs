using DialogueEditor;
using Mirror;
using UnityEngine;

public class DialogueManager : InteractiveObject
{
    [SerializeField]
    private NPCConversation conversation;

    [SyncVar]
    private bool isInteractable = true;
    private Transform character;

    public override void Awake()
    {
        base.Awake();
        if (!conversation)
            Debug.LogError("Conversation is not set at object: " + gameObject.name);
    }

    // Begin conversation
    public override void OnInteractButtonUp(Transform character)
    {
        this.character = character;
        ConversationManager.Instance.StartConversation(conversation);
        CmdSetInteractable(false);
        ConversationManager.Instance.OnConversationEnded.AddListener(EndConversation);
        character.GetComponent<PlayerLinks>().interactableTrigger.SetCheckTrigger(false);
    }

    // End conversation
    public void EndConversation()
    {
        CmdSetInteractable(true);
        ConversationManager.Instance.OnConversationEnded.RemoveListener(EndConversation);
        character.GetComponent<PlayerLinks>().interactableTrigger.SetCheckTrigger(true);
        character = null;
    }

    [Command(requiresAuthority = false)]
    private void CmdSetInteractable(bool isInteractable)
    {
        this.isInteractable = isInteractable;
    }

    public override bool IsInteractable() => isInteractable;
}
