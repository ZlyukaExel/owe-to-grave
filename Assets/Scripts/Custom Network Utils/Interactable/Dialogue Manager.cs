using DialogueEditor;
using UnityEngine;

public class DialogueManager : InteractiveObject
{
    [SerializeField]
    private NPCConversation conversation;

    void Awake()
    {
        if (!conversation)
            Debug.LogError("Conversation is not set at object: " + gameObject.name);
    }

    public override void Interact(Transform character)
    {
        ConversationManager.Instance.StartConversation(conversation);
    }

    public override string InteractionText() => "Speak";

    public override bool IsInteractable() => true;
}
