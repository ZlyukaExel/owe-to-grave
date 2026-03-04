using UnityEngine;

public class CharacterConfigManager : MonoBehaviour
{
    [SerializeField]
    private CharacterConfig currentConfig = new();

    [SerializeField]
    private GameObject head;

    [SerializeField]
    private GameObject[] weapons = new GameObject[0];

    [SerializeField]
    private GameObject[] pants = new GameObject[0];

    [SerializeField]
    private GameObject[] tops = new GameObject[0];

    [SerializeField]
    private GameObject[] shoes = new GameObject[0];

    [SerializeField]
    private GameObject[] gloves = new GameObject[0];

    [SerializeField]
    private GameObject[] hats = new GameObject[0];

    [SerializeField]
    private GameObject[] hair = new GameObject[0];

    [SerializeField]
    private GameObject[] masks = new GameObject[0];

    public void SetConfig(CharacterConfig config)
    {
        if (currentConfig.weaponId != config.weaponId)
        {
            SetWeaponActive(weapons[config.weaponId], false, config.inCombat);
            SetWeaponActive(weapons[currentConfig.weaponId], true, config.inCombat);
        }

        if (currentConfig.weaponId != config.weaponId)
            SetWeaponActive(weapons[config.weaponId], true, config.inCombat);

        if (currentConfig.pantsId != config.pantsId)
        {
            SetActiveIfNotNull(pants[currentConfig.pantsId], false);
            SetActiveIfNotNull(pants[config.pantsId], true);
        }

        if (currentConfig.topId != config.topId)
        {
            SetActiveIfNotNull(tops[currentConfig.topId], false);
            SetActiveIfNotNull(tops[config.topId], true);
        }

        if (currentConfig.shoesId != config.shoesId)
        {
            SetActiveIfNotNull(shoes[currentConfig.shoesId], false);
            SetActiveIfNotNull(shoes[config.shoesId], true);
        }

        if (currentConfig.glovesId != config.glovesId)
        {
            SetActiveIfNotNull(gloves[currentConfig.glovesId], false);
            SetActiveIfNotNull(gloves[config.glovesId], true);
        }

        if (currentConfig.hatId != config.hatId)
        {
            SetActiveIfNotNull(hats[currentConfig.hatId], false);
            SetActiveIfNotNull(hats[config.hatId], true);
        }

        if (currentConfig.maskId != config.maskId)
        {
            SetActiveIfNotNull(masks[currentConfig.maskId], false);
            SetActiveIfNotNull(masks[config.maskId], true);
        }
    }

    public CharacterConfig GetConfig() => currentConfig;

    // private void DisableAll()
    // {
    //     foreach (var weapon in weapons)
    //         SetActiveIfNotNull(weapon, false);
    //     foreach (var pant in pants)
    //         SetActiveIfNotNull(pant, false);
    //     foreach (var top in tops)
    //         SetActiveIfNotNull(top, false);
    //     foreach (var shoe in shoes)
    //         SetActiveIfNotNull(shoe, false);
    //     foreach (var glove in gloves)
    //         SetActiveIfNotNull(glove, false);
    //     foreach (var hat in hats)
    //         SetActiveIfNotNull(hat, false);
    //     foreach (var mask in masks)
    //         SetActiveIfNotNull(mask, false);
    // }

    private void SetActiveIfNotNull(GameObject gameObject, bool isActive)
    {
        if (gameObject)
            gameObject.SetActive(isActive);
    }

    private void SetWeaponActive(GameObject gameObject, bool isActive, bool inCombat)
    {
        if (!gameObject)
            return;

        Weapon weapon = gameObject.GetComponent<Weapon>();

        if (isActive)
            weapon.Activate(inCombat);
        else
            weapon.DeactivateBoth();
    }

    public GameObject GetHead() => head;

    public GameObject GetWeapon() => weapons[currentConfig.weaponId];

    public GameObject GetPants() => pants[currentConfig.pantsId];

    public GameObject GetTop() => tops[currentConfig.topId];

    public GameObject GetShoes() => shoes[currentConfig.shoesId];

    public GameObject GetGloves() => gloves[currentConfig.glovesId];

    public GameObject GetHat() => hats[currentConfig.hatId];

    public GameObject GetHair() => hair[currentConfig.hairId];

    public GameObject GetMask() => masks[currentConfig.maskId];
}
