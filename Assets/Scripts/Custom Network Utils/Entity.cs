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

    protected NetworkHitpoints hp;

    private void Awake()
    {
        hp = GetComponent<NetworkHitpoints>();
        hp.onDeath.AddListener(OnDeath);
        OnNicknameChanged("", entityName);
    }

    private void OnNicknameChanged(string oldName, string newName)
    {
        gameObject.name = newName;
        if (textTag)
            textTag.text = newName;
    }

    public virtual void OnDeath() { }
}
