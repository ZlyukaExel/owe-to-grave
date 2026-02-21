using Mirror;

public class NetworkActive : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnStateChanged))]
    public bool isActive = true;

    void Start()
    {
        gameObject.SetActive(isActive);
    }

    [Command(requiresAuthority = false)]
    public void SetActive(bool value)
    {
        isActive = value;
    }

    private void OnStateChanged(bool oldVar, bool newVar)
    {
        gameObject.SetActive(newVar);
    }
}
