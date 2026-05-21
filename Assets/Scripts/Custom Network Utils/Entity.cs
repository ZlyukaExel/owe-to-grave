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

    [SyncVar]
    public NetworkIdentity currentNetworkIdentity;

    private void Awake()
    {
        hp = GetComponent<NetworkHitpoints>();
        hp.onDeath.AddListener(OnDeath);
        OnNicknameChanged("", entityName);
    }

    private void Start()
    {
        if (!(netIdentity.connectionToClient == null && isServer || isOwned))
            return;

        ResetCurrentNetworkIdentity();
    }

    [Command(requiresAuthority = false)]
    public void SetCurrentNetworkIdentity(NetworkIdentity newIdentity)
    {
        currentNetworkIdentity = newIdentity;
    }

    public void ResetCurrentNetworkIdentity()
    {
        SetCurrentNetworkIdentity(netIdentity);
    }

    private void OnNicknameChanged(string oldName, string newName)
    {
        gameObject.name = newName;
        if (textTag)
            textTag.text = newName;
    }

    public virtual void OnDeath() { }
}
