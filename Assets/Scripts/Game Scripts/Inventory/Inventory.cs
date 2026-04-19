using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : NetworkBehaviour
{
    public readonly SyncList<uint> items = new();
    public UnityEvent<uint> onItemAdded,
        onItemRemoved;

    [Command(requiresAuthority = false)]
    public void TakeIn(uint itemId)
    {
        items.Add(itemId);
    }

    [Command(requiresAuthority = false)]
    public void TakeIn(NetworkIdentity item)
    {
        TakeIn(item.assetId);
        NetworkServer.Destroy(item.gameObject);
    }

    [Command(requiresAuthority = false)]
    public void TakeOut(uint itemId)
    {
        if (!items.Contains(itemId))
            return;

        ItemData itemData = ItemDataManager.Instance.GetItem(itemId);
        GameObject spawned = Instantiate(itemData.prefab, transform.position, transform.rotation);
        NetworkServer.Spawn(spawned);
        items.Remove(itemId);
    }

    public override void OnStartClient()
    {
        items.OnChange += OnItemsChanged;
    }

    private void OnItemsChanged(SyncList<uint>.Operation op, int itemIndex, uint itemId)
    {
        switch (op)
        {
            case SyncList<uint>.Operation.OP_ADD:
                onItemAdded.Invoke(itemId);
                break;

            case SyncList<uint>.Operation.OP_REMOVEAT:
                onItemRemoved.Invoke(itemId);
                break;
        }
    }
}
