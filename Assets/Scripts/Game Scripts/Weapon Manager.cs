using UnityEngine;

[RequireComponent(typeof(Animator))]
public class WeaponManager : MonoBehaviour
{
    [HideInInspector]
    public bool inCombat = false;
    private int currentWeaponId = 0;
    private Animator animator;

    [SerializeField]
    private NetworkSwapGameObjects[] weapons;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SwitchWeapon(int weaponId)
    {
        weapons[currentWeaponId].DeactivateBoth();
        currentWeaponId = weaponId;
        animator.SetInteger("Weapon Id", weaponId);
        if (inCombat)
            ActivatePrimaryWeapon();
        else
            ActivateHiddenWeapon();
    }

    public void ActivateHiddenWeapon()
    {
        weapons[currentWeaponId].ActivateSecond();
    }

    public void ActivatePrimaryWeapon()
    {
        weapons[currentWeaponId].ActivateFirst();
    }
}
