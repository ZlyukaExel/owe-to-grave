using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;

namespace Mirror.Discovery
{
    [RequireComponent(typeof(NetworkDiscovery))]
    public class CustomNetworkDiscovery : MonoBehaviour
    {
        private readonly Dictionary<long, ServerResponse> discoveredServers = new();
        public NetworkDiscovery networkDiscovery { get; private set; }
        [SerializeField] private float discoveryInterval = 3;
        [SerializeField] private ConnectToServerButton buttonPrefab;
        [SerializeField] private Transform buttonsContainer;
        [SerializeField] private GameObject loadingBar;
        public GameObject passwordScreen;
        private Coroutine discoveryCoroutine;

        private void Start()
        {
            networkDiscovery = GetComponent<NetworkDiscovery>();
        }

        public void StartDiscovery()
        {
            discoveryCoroutine = StartCoroutine(Discovery());
        }

        public void StopDiscovery()
        {
            StopCoroutine(discoveryCoroutine);
            networkDiscovery.StopDiscovery();
        }

        private IEnumerator Discovery()
        {
            while (true)
            {
                discoveredServers.Clear();
                networkDiscovery.StartDiscovery();

                yield return new WaitForSeconds(0.5f);

                UpdateButtons();

                yield return new WaitForSeconds(discoveryInterval - 0.5f);
            }
        }

        private void UpdateButtons()
        {
            ClearButtons();

            // Create new buttons
            foreach (var server in discoveredServers.Values)
            {
                ConnectToServerButton button = Instantiate(buttonPrefab, buttonsContainer);
                button.info = server;
                button.customNetworkDiscovery = this;
                button.transform.GetComponentInChildren<TMP_Text>().text = server.serverName;
                button.transform.GetComponentInChildren<CustomToggle>().isOn = !string.IsNullOrEmpty(server.password);
            }

            if (loadingBar)
            {
                if (discoveredServers.Count > 0)
                    loadingBar.SetActive(false);
                else
                    loadingBar.SetActive(true);
            }
        }

        public void ClearButtons()
        {
            if (!buttonsContainer) return;
            foreach (Transform child in buttonsContainer)
            {
                if (child)
                    Destroy(child.gameObject);
            }
        }

        public void OnDiscoveredServer(ServerResponse info)
        {
            discoveredServers[info.serverId] = info;
        }
    }
}
