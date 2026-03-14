using System;
using Mirror;
using TMPro;
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
    private int deathsCounter = 0;
    private bool isVulnerable = true;
    private Slider hpSlider;
    private TMP_Text deathsCounterText;
    private Entity player;

    [HideInInspector]
    public UnityEvent onDeath;

    private void Start()
    {
        player = GetComponent<Entity>();

        if (hitPoints)
            hitPoints.SetHp(netIdentity);
        else if (GetComponent<HitPointsSet>() is HitPointsSet hpSet)
            ChangeHitPoints(hpSet);

        if (isServer)
            currentHp = maxHp;
    }

    [Command(requiresAuthority = false)]
    public void Damage(float damage)
    {
        // TODO: pass damage info, not damage

        //print("Damage taken: " + damage);

        // Ignore if already dead
        if (!isVulnerable || currentHp <= 0)
            return;

        currentHp = Mathf.Max(0, currentHp - damage);

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
        if (player)
        {
            deathsCounter++;
            deathsCounterText.text = $"Deaths: {deathsCounter}";
        }
        onDeath.Invoke();
    }

    [TargetRpc]
    private void TargetDie() => Die();

    public void SetUi(Transform ui)
    {
        hpSlider = ui.Find("Hitpoints").GetComponent<Slider>();
        deathsCounterText = ui.Find("Deaths Counter").GetComponent<TMP_Text>();
    }
}

[Serializable]
public class DamageInfo
{
    public float damage = 20;
    public float critMultiplier = 1;
    public DamageType type;
    public NetworkIdentity source;

    public DamageInfo(float damage, float critMultiplier, DamageType type, NetworkIdentity source)
    {
        this.damage = damage;
        this.critMultiplier = critMultiplier;
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
