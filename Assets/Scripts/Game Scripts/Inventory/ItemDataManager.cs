using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Instance { get; private set; }

    [SerializeField]
    private ItemData[] allItems;
    private Dictionary<uint, ItemData> itemDatabase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        allItems = Resources.LoadAll<ItemData>("Prefabs/Items");
        itemDatabase = allItems.ToDictionary(item => item.id);

        foreach (var item in allItems)
        {
            if (item.prefab != null)
            {
                NetworkClient.RegisterPrefab(item.prefab);
            }
        }
    }

    public ItemData GetItem(uint id)
    {
        if (itemDatabase.TryGetValue(id, out ItemData item))
        {
            return item;
        }

        Debug.LogWarning($"Item with ID {id} not found!");
        return null;
    }
}
