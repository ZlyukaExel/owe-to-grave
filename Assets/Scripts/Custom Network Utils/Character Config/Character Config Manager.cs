using System;
using UnityEngine;

public class CharacterConfigManager : MonoBehaviour
{
    [SerializeField]
    private CharacterConfig currentConfig = new();

    [SerializeField]
    private GameObject head;

    [SerializeField]
    private Weapon[] weapons = new Weapon[0];

    [SerializeField]
    private Clothing[] pants = new Clothing[0];

    [SerializeField]
    private Clothing[] tops = new Clothing[0];

    [SerializeField]
    private Clothing[] shoes = new Clothing[0];

    [SerializeField]
    private Clothing[] gloves = new Clothing[0];

    [SerializeField]
    private Clothing[] hats = new Clothing[0];

    [SerializeField]
    private Clothing[] hair = new Clothing[0];

    [SerializeField]
    private Clothing[] masks = new Clothing[0];

    public void SetConfig(CharacterConfig config)
    {
        if (currentConfig.weaponId != config.weaponId)
        {
            Weapon currentWeapon = weapons[currentConfig.weaponId];
            weapons[config.weaponId].DeactivateBoth();
            weapons[currentConfig.weaponId].Activate(config.inCombat);

            weapons[config.weaponId].onShot = currentWeapon.onShot;
        }

        if (currentConfig.inCombat != config.inCombat)
            weapons[config.weaponId].Activate(config.inCombat);

        if (currentConfig.pantsId != config.pantsId)
        {
            SetActiveIfNotNull(pants[currentConfig.pantsId].gameObject, false);
            SetActiveIfNotNull(pants[config.pantsId].gameObject, true);
        }

        if (currentConfig.topId != config.topId)
        {
            SetActiveIfNotNull(tops[currentConfig.topId].gameObject, false);
            SetActiveIfNotNull(tops[config.topId].gameObject, true);
        }

        if (currentConfig.shoesId != config.shoesId)
        {
            SetActiveIfNotNull(shoes[currentConfig.shoesId].gameObject, false);
            SetActiveIfNotNull(shoes[config.shoesId].gameObject, true);
        }

        if (currentConfig.glovesId != config.glovesId)
        {
            SetActiveIfNotNull(gloves[currentConfig.glovesId].gameObject, false);
            SetActiveIfNotNull(gloves[config.glovesId].gameObject, true);
        }

        if (currentConfig.hatId != config.hatId)
        {
            SetActiveIfNotNull(hats[currentConfig.hatId].gameObject, false);
            SetActiveIfNotNull(hats[config.hatId].gameObject, true);
        }

        if (currentConfig.maskId != config.maskId)
        {
            SetActiveIfNotNull(masks[currentConfig.maskId].gameObject, false);
            SetActiveIfNotNull(masks[config.maskId].gameObject, true);
        }

        currentConfig = config;
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

    public GameObject GetHead() => head;

    public Weapon GetWeapon() => weapons[currentConfig.weaponId];

    public Clothing GetPants() => pants[currentConfig.pantsId];

    public Clothing GetTop() => tops[currentConfig.topId];

    public Clothing GetShoes() => shoes[currentConfig.shoesId];

    public Clothing GetGloves() => gloves[currentConfig.glovesId];

    public Clothing GetHat() => hats[currentConfig.hatId];

    public Clothing GetHair() => hair[currentConfig.hairId];

    public Clothing GetMask() => masks[currentConfig.maskId];

    public int GetClothingIdByItem(InventoryItem item)
    {
        if (item.itemId == 0)
            return -1;

        if (ItemDataManager.Instance.GetItem(item.itemId) is not ClothingData clothingData)
            return -1;

        var targetArray = clothingData.clothingType switch
        {
            ClothingType.Pants => pants,
            ClothingType.Top => tops,
            ClothingType.Shoes => shoes,
            ClothingType.Gloves => gloves,
            ClothingType.Hat => hats,
            ClothingType.Mask => masks,
            _ => null,
        };

        if (targetArray == null)
            return -1;

        return Array.FindIndex(targetArray, x => x?.data?.id == clothingData.id);
    }
}
