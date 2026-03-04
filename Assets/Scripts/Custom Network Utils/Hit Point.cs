using UnityEngine;

public class HitPoint : MonoBehaviour
{
    [SerializeField]
    private bool isCrit;
    private HitPointsSet hpSet = null;

    public void Damage(DamageInfo damageInfo)
    {
        hpSet?.Damage(isCrit ? damageInfo.damage * damageInfo.critMultiplier : damageInfo.damage);
    }

    public NetworkHitpoints GetHp()
    {
        return hpSet.GetHp();
    }

    public void SetHpSet(HitPointsSet hpSet)
    {
        this.hpSet = hpSet;
    }
}
