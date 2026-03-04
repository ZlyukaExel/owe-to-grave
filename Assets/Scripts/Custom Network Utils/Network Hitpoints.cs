using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkHitpoints : NetworkBehaviour
{
    [SerializeField]
    private float maxHp = 100;
    public HitPointsSet hitPoints;

    [SyncVar(hook = nameof(OnHpChanged))]
    public float currentHp;
    private int deathsCounter = 0;
    private bool isVulnerable = true;
    private Slider hpSlider;
    private TMP_Text deathsCounterText;

    void Start()
    {
        if (isServer)
            currentHp = maxHp;
    }

    [Command(requiresAuthority = false)]
    public void Damage(float damage)
    {
        //print("Damage taken: " + damage);

        // Ignore if no damage or already dead
        if (!isVulnerable || damage <= 0 || currentHp <= 0)
            return;

        currentHp = Mathf.Max(0, currentHp - damage);
    }

    public void ChangeHitPoints(HitPointsSet hitPoints)
    {
        if (!isOwned)
            return;

        this.hitPoints?.SetHp(null);
        this.hitPoints = hitPoints;
        this.hitPoints.SetHp(netIdentity);
    }

    private void OnHpChanged(float oldVar, float newVar)
    {
        UpdateUi();

        // Death
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void UpdateUi()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHp / maxHp;
        }
    }

    [Command]
    private void CmdSetHp(float hp)
    {
        currentHp = hp;
    }

    private void Die()
    {
        if (isOwned)
        {
            CmdSetHp(maxHp);
            deathsCounter++;
            deathsCounterText.text = $"Deaths: {deathsCounter}";
            Debug.Log($"/a {gameObject.name} died");
        }
    }

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
    public string source;

    public DamageInfo(float damage, float critMultiplier, DamageType type, string source)
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
