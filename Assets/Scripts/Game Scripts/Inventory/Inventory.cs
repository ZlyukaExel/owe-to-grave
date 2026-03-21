using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    public List<InventoryItem> items = new();

    public void TakeIn(InventoryItem item)
    {
        items.Add(item);
    }

    public void TakeOut(InventoryItem item)
    {
        GameObject prefab = NetworkManager.singleton.spawnPrefabs[item.id];
        GameObject spawned = Instantiate(prefab, transform.position, transform.rotation);
        NetworkServer.Spawn(spawned);

        items.Remove(item);
    }
}
