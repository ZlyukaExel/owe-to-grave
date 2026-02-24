using UnityEngine;

public class HitPoint : MonoBehaviour
{
    [SerializeField]
    private NetworkHitpoints hp;

    [SerializeField]
    private bool isCrit;

    public void Damage(DamageInfo damageInfo)
    {
        hp?.Damage(isCrit ? damageInfo.damage * damageInfo.critMultiplier : damageInfo.damage);
    }

    public void SetHp(NetworkHitpoints hp)
    {
        this.hp = hp;
    }

    public NetworkHitpoints GetHp()
    {
        return hp;
    }
}
