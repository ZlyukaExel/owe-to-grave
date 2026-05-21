using System;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class CharacterConfigManager : MonoBehaviour
{
    [SerializeField]
    private CharacterConfig currentConfig = new();

    [SerializeField]
    private GameObject head;

    public Weapon[] weapons = new Weapon[0];

    public Clothing[] pants = new Clothing[0];

    public Clothing[] tops = new Clothing[0];

    public Clothing[] shoes = new Clothing[0];

    public Clothing[] gloves = new Clothing[0];

    public Clothing[] hats = new Clothing[0];

    public Clothing[] hair = new Clothing[0];

    public Clothing[] masks = new Clothing[0];

    public UnityEvent<CharacterConfig> OnConfigChanged = new();

    private Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetConfig(CharacterConfig newConfig)
    {
        if (newConfig.Equals(currentConfig))
            return;

        if (isActive)
            SetObjectsActive(false);

        currentConfig = newConfig;

        if (isActive)
            SetObjectsActive(true);

        OnConfigChanged.Invoke(newConfig);
    }

    public CharacterConfig GetConfig() => currentConfig;

    public GameObject GetHead() => head;

    public Weapon GetPrimary() => weapons[currentConfig.primaryWeaponId];

    public Weapon GetSecondary() => weapons[currentConfig.secondaryWeaponId];

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

    public int GetWeaponId(WeaponData weaponData)
    {
        if (!weaponData)
            return -1;

        return Array.FindIndex(weapons, x => x?.data.id == weaponData.id);
    }

    public void EquipClothing(InventoryItem item, ClothingType clothingType)
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
    }

    public void EquipWeapon(InventoryItem item, bool primary)
    {
        int weaponId;
        if (item.quantity <= 0 || item.itemId == 0)
            weaponId = 0;
        else
            weaponId = GetWeaponId(ItemDataManager.Instance.GetItem(item.itemId) as WeaponData);
        if (weaponId == -1)
            return;

        CharacterConfig newConfig = currentConfig;
        if (primary)
        {
            newConfig.primaryWeaponId = weaponId;
            animator.SetInteger("weaponId", weaponId);
        }
        else
            newConfig.secondaryWeaponId = weaponId;

        SetConfig(newConfig); // To save changes locally
    }

    public bool isActive = true;

    public void SetActive(bool active)
    {
        if (TryGetComponent(out NavMeshObstacle obstacle))
            obstacle.enabled = active;

        if (TryGetComponent(out NavMeshAgent agent))
            agent.enabled = active;

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
        {
            GetPrimary()?.Activate(currentConfig.inCombat);
            GetSecondary()?.Activate(false);
        }
        else
        {
            GetPrimary()?.DeactivateBoth();
            GetSecondary()?.DeactivateBoth();
        }
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
