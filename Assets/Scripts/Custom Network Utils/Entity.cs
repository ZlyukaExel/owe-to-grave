using Mirror;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(NetworkHitpoints))]
public class Entity : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnNicknameChanged))]
    public string entityName = "Entity";

    [SerializeField]
    protected TMP_Text textTag;

    private void Awake()
    {
        OnNicknameChanged("", entityName);
    }

    private void OnNicknameChanged(string oldName, string newName)
    {
        gameObject.name = newName;
        if (textTag)
            textTag.text = newName;
    }
}
