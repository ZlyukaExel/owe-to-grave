using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NetworkHitpoints : NetworkBehaviour
{
    [SerializeField]
    private float maxHp = 100;

    [SerializeField]
    private HitPointsSet hitPoints;

    [SyncVar(hook = nameof(OnHpChanged))]
    public float currentHp;
    private bool isVulnerable = true;

    [SerializeField]
    private Slider hpSlider;
    private Player player;

    [HideInInspector]
    public UnityEvent onDeath;

    [HideInInspector]
    public UnityEvent<DamageInfo> onDamage;

    private void Start()
    {
        player = GetComponent<Player>();

        if (hitPoints)
            hitPoints.SetHp(netIdentity);
        else if (GetComponent<HitPointsSet>() is HitPointsSet hpSet)
            ChangeHitPoints(hpSet);

        if (isServer)
            currentHp = maxHp;
    }

    [Command(requiresAuthority = false)]
    public void Damage(DamageInfo damageInfo)
    {
        // TODO: pass crit

        // print("Damage taken: " + damageInfo.damage);

        // Ignore if already dead
        if (!isVulnerable || currentHp <= 0)
            return;

        currentHp = Mathf.Max(0, currentHp - damageInfo.damage);

        if (currentHp <= 0)
        {
            if (player)
                Debug.Log($"/a {player.entityName} died");

            // Items without autority die on Server
            if (connectionToClient == null)
                Die();
            else
                TargetDie();
        }
    }

    public void ChangeHitPoints(HitPointsSet hitPoints)
    {
        if (!(netIdentity.connectionToClient == null && isServer || isOwned))
            return;

        this.hitPoints?.SetHp(null);
        this.hitPoints = hitPoints;
        this.hitPoints.SetHp(netIdentity);
    }

    private void OnHpChanged(float oldVar, float newVar)
    {
        UpdateUi();
    }

    private void UpdateUi()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHp / maxHp;
        }
    }

    [Command]
    public void Respawn()
    {
        currentHp = maxHp;
    }

    private void Die()
    {
        onDeath.Invoke();
    }

    [TargetRpc]
    private void TargetDie() => Die();

    public void SetUi(Slider hpSlider)
    {
        this.hpSlider = hpSlider;
    }
}

[Serializable]
public class DamageInfo
{
    public float damage = 20;
    public DamageType type;
    public NetworkIdentity source;

    public DamageInfo() { }

    public DamageInfo(float damage, DamageType type, NetworkIdentity source)
    {
        this.damage = damage;
        this.type = type;
        this.source = source;
    }
}

public enum DamageType
{
    Bullet,
    Item,
    Fall,
}
