using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

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

    public UnityEvent<CharacterConfig> OnConfigChanged = new();

    public void SetConfig(CharacterConfig newConfig)
    {
        if (isActive)
            SetObjectsActive(false);

        currentConfig = newConfig;

        if (isActive)
            SetObjectsActive(true);
    }

    public CharacterConfig GetConfig() => currentConfig;

    public GameObject GetHead() => head;

    public Weapon GetWeapon() => weapons[currentConfig.weaponId];

    public Clothing GetPants() => pants[currentConfig.pantsId];

    public Clothing GetTop() => tops[currentConfig.topId];

    public Clothing GetShoes() => shoes[currentConfig.shoesId];

    public Clothing GetGloves() => gloves[currentConfig.glovesId];

    public Clothing GetHat() => hats[currentConfig.hatId];

    public Clothing GetHair() => hair[currentConfig.hairId];

    public Clothing GetMask() => masks[currentConfig.maskId];

    public int GetClothingId(ClothingData clothingData)
    {
        if (!clothingData)
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

    public void Equip(InventoryItem item, ClothingType clothingType)
    {
        int clothingId;
        if (item.quantity <= 0 || item.itemId == 0)
            clothingId = 0;
        else
            clothingId = GetClothingId(
                ItemDataManager.Instance.GetItem(item.itemId) as ClothingData
            );
        if (clothingId == -1)
            return;

        CharacterConfig newConfig = currentConfig;

        switch (clothingType)
        {
            case ClothingType.Pants:
                newConfig.pantsId = clothingId;
                break;
            case ClothingType.Top:
                newConfig.topId = clothingId;
                break;
            case ClothingType.Shoes:
                newConfig.shoesId = clothingId;
                break;
            case ClothingType.Gloves:
                newConfig.glovesId = clothingId;
                break;
            case ClothingType.Hat:
                newConfig.hatId = clothingId;
                break;
            case ClothingType.Mask:
                newConfig.maskId = clothingId;
                break;
        }

        SetConfig(newConfig); // To save changes locally
        OnConfigChanged.Invoke(newConfig);
    }

    public bool isActive = true;

    public void SetActive(bool active)
    {
        isActive = active;
        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = active;
        }

        if (TryGetComponent(out NetworkRigidbodyReliable reliableRigidbody))
            reliableRigidbody.isKinematic = !active;

        if (TryGetComponent(out NetworkRigidbodyUnreliable unreliableRigidbody))
            unreliableRigidbody.isKinematic = !active;

        SetObjectsActive(active);
    }

    private void SetObjectsActive(bool active)
    {
        if (active)
            GetWeapon().Activate(currentConfig.inCombat);
        else
            GetWeapon().DeactivateBoth();
        GetHat()?.gameObject.SetActive(active);
        GetTop()?.gameObject.SetActive(active);
        GetPants()?.gameObject.SetActive(active);
        GetMask()?.gameObject.SetActive(active);
        GetGloves()?.gameObject.SetActive(active);
        GetShoes()?.gameObject.SetActive(active);
        GetHair()?.gameObject.SetActive(active);
        GetHead()?.gameObject.SetActive(active);
    }
}
