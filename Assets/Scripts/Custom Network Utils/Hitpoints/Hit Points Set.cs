using Mirror;
using UnityEngine;

public class HitPointsSet : NetworkBehaviour
{
    [SyncVar]
    [SerializeField]
    private NetworkHitpoints hp;

    private void Start()
    {
        foreach (var hitPoint in GetComponentsInChildren<HitPoint>())
            hitPoint.SetHpSet(this);
    }

    [Command(requiresAuthority = false)]
    public void SetHp(NetworkIdentity networkIdentity)
    {
        if (networkIdentity == null)
            hp = null;
        else
            hp = networkIdentity.GetComponent<NetworkHitpoints>();
    }

    public NetworkHitpoints GetHp() => hp;

    public void Damage(DamageInfo damageInfo)
    {
        hp?.Damage(damageInfo);
    }
}
