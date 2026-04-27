using UnityEngine;

public class HitPoint : MonoBehaviour
{
    [SerializeField]
    private float damageMultiplier = 1;
    private HitPointsSet hpSet = null;

    public void Damage(DamageInfo damageInfo)
    {
        damageInfo.damage *= damageMultiplier;
        hpSet?.Damage(damageInfo);
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
