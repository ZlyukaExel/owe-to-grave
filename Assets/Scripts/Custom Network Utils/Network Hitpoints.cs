using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkHitpoints : NetworkBehaviour
{
    [SerializeField]
    private float maxHp = 100;
    public Collider[] critPoints;

    [SyncVar(hook = nameof(OnHpChanged))]
    public float currentHp;
    private int deathsCounter = 0;
    private Slider hpSlider;
    private TMP_Text deathsCounterText;

    void Start()
    {
        currentHp = maxHp;
    }

    [Command(requiresAuthority = false)]
    public void Damage(float damage)
    {
        //print("Damage taken: " + damage);

        // Ignore if no damage or already dead
        if (damage <= 0 || currentHp <= 0)
            return;

        currentHp -= damage;
    }

    private void OnHpChanged(float oldVar, float newVar)
    {
        if (isOwned)
        {
            // Death
            if (currentHp <= 0)
            {
                CmdSetHp(maxHp);
                deathsCounter++;
                deathsCounterText.text = $"Deaths: {deathsCounter}";
                Debug.Log($"/a {gameObject.name} died");
            }

            hpSlider.value = currentHp;
        }
    }

    [Command]
    private void CmdSetHp(float hp)
    {
        currentHp = hp;
    }

    public void SetUi(Transform canvas)
    {
        hpSlider = canvas.Find("Hitpoints").GetComponent<Slider>();
        deathsCounterText = canvas.Find("Deaths Counter").GetComponent<TMP_Text>();
    }
}
