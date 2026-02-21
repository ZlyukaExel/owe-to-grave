using Mirror;
using UnityEngine;

public class NetworkSwapGameObjects : NetworkBehaviour
{
    [SerializeField]
    private NetworkDisable obj1,
        obj2;

    public void ActivateFirst()
    {
        if (!isOwned)
            return;

        obj1.SetEnabled(true);
        obj2.SetEnabled(false);
    }

    public void ActivateSecond()
    {
        if (!isOwned)
            return;

        obj1.SetEnabled(false);
        obj2.SetEnabled(true);
    }

    public void DeactivateBoth()
    {
        if (!isOwned)
            return;

        obj1.SetEnabled(false);
        obj2.SetEnabled(false);
    }
}
