using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Mirror.Discovery
{
    [RequireComponent(typeof(NetworkDiscovery))]
    public class CustomNetworkDiscovery : MonoBehaviour
    {
        private readonly Dictionary<long, ServerResponse> discoveredServers = new();
        private readonly Dictionary<long, ConnectToServerButton> serverButtons = new();

        public NetworkDiscovery networkDiscovery { get; private set; }

        [SerializeField]
        private float discoveryInterval = 3;

        [SerializeField]
        private ConnectToServerButton buttonPrefab;

        [SerializeField]
        private Transform buttonsContainer;

        [SerializeField]
        private GameObject loadingBar;
        public GameObject passwordScreen;
        private Coroutine discoveryCoroutine;

        [SerializeField]
        private UnityEvent onAwaliableServersChange;

        private void Start()
        {
            networkDiscovery = GetComponent<NetworkDiscovery>();
        }

        public void StartDiscovery()
        {
            if (discoveryCoroutine != null)
                StopCoroutine(discoveryCoroutine);
            discoveryCoroutine = StartCoroutine(Discovery());
        }

        public void StopDiscovery()
        {
            if (discoveryCoroutine != null)
                StopCoroutine(discoveryCoroutine);
            networkDiscovery.StopDiscovery();
            ClearButtons();
        }

        private IEnumerator Discovery()
        {
            while (true)
            {
                discoveredServers.Clear();
                networkDiscovery.StartDiscovery();

                yield return new WaitForSeconds(1.0f);

                UpdateButtons();

                yield return new WaitForSeconds(discoveryInterval);
            }
        }

        private void UpdateButtons()
        {
            List<long> idsToRemove = new();
            foreach (var id in serverButtons.Keys)
            {
                if (!discoveredServers.ContainsKey(id))
                    idsToRemove.Add(id);
            }

            foreach (var id in idsToRemove)
            {
                if (serverButtons[id] != null)
                    Destroy(serverButtons[id].gameObject);
                serverButtons.Remove(id);
            }

            foreach (var server in discoveredServers.Values)
            {
                if (!serverButtons.TryGetValue(server.serverId, out ConnectToServerButton button))
                {
                    // Создаем только если такой кнопки еще нет
                    button = Instantiate(buttonPrefab, buttonsContainer);
                    serverButtons.Add(server.serverId, button);
                }

                button.info = server;
                button.customNetworkDiscovery = this;
                button.transform.GetComponentInChildren<TMP_Text>().text = server.serverName;

                var toggle = button.transform.GetComponentInChildren<CustomToggle>();
                if (toggle != null)
                    toggle.isOn = !string.IsNullOrEmpty(server.password);
            }

            if (loadingBar)
                loadingBar.SetActive(serverButtons.Count == 0);

            onAwaliableServersChange.Invoke();
        }

        public void ClearButtons()
        {
            foreach (var button in serverButtons.Values)
            {
                if (button)
                    Destroy(button.gameObject);
            }
            serverButtons.Clear();
            discoveredServers.Clear();
        }

        public void OnDiscoveredServer(ServerResponse info)
        {
            discoveredServers[info.serverId] = info;
        }
    }
}
