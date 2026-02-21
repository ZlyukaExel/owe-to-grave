using Mirror;
using TMPro;
using UnityEngine;

public class PlayersListSettings : MonoBehaviour
{
    private ServerInfo serverInfo;
    public Transform listContainer;
    public GameObject playerElement;

    void Awake()
    {
        serverInfo = ServerInfo.Instance;
    }

    void OnEnable()
    {
        serverInfo.OnPlayerAdded += OnPlayerAdded;
        serverInfo.OnPlayerRemoved += OnPlayerRemoved;

        foreach (var player in serverInfo.players)
        {
            GameObject newElement = Instantiate(playerElement, listContainer);
            newElement.transform.GetComponentInChildren<TMP_Text>().text = player.name;
        }
    }

    void OnDisable()
    {
        serverInfo.OnPlayerAdded -= OnPlayerAdded;
        serverInfo.OnPlayerRemoved -= OnPlayerRemoved;

        foreach (Transform child in listContainer)
        {
            if (!child.TryGetComponent(out TMP_Text _))
                Destroy(child.gameObject);
        }
    }

    private void OnPlayerAdded(NetworkIdentity player)
    {
        foreach (Transform child in listContainer)
        {
            if (child.GetComponentInChildren<TMP_Text>().text.Equals(player.name))
                return;
        }

        GameObject newElement = Instantiate(playerElement, listContainer);
        newElement.transform.GetComponentInChildren<TMP_Text>().text = player.name;
    }

    private void OnPlayerRemoved(NetworkIdentity player)
    {
        foreach (Transform child in listContainer)
        {
            if (child.GetComponentInChildren<TMP_Text>().text.Equals(player.name))
                Destroy(child.gameObject);
        }
    }
}
