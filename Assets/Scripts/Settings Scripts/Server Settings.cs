using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerSettings : MonoBehaviour
{
    [SerializeField]
    private Toggle serverVisibleToggle;

    [SerializeField]
    private Toggle pvpToggle;

    [SerializeField]
    private TMP_InputField serverPassword;

    [SerializeField]
    private TMP_InputField maxPlayers;

    [SerializeField]
    private Button confirm,
        cancel;

    void Start()
    {
        // Settings work only on server
        if (!NetworkServer.active)
        {
            gameObject.SetActive(false);
            return;
        }

        ResetSettings();

        confirm.onClick.AddListener(ConfirmSettings);
        cancel.onClick.AddListener(ResetSettings);
    }

    private void SetMaxPlayers(string input)
    {
        int maxConnections = int.Parse(input);
        NetworkManager.singleton.maxConnections = maxConnections;
        PlayerPrefs.SetInt("Server Max Connections", maxConnections);
    }

    private void SetServerPassword(string input)
    {
        NetworkDiscovery.Instance.Password = input;
        PlayerPrefs.SetString("Server Password", input);
    }

    public void ResetSettings()
    {
        serverPassword.text = PlayerPrefs.GetString("Server Password");
        SetServerPassword(serverPassword.text);

        maxPlayers.text = PlayerPrefs.GetInt("Server Max Connections").ToString();
        SetMaxPlayers(maxPlayers.text);

        // Making server visible
        serverVisibleToggle.isOn = NetworkDiscovery.Instance.serverVisible;

        // If pvp is off, bullet ignores Player
        pvpToggle.isOn = ServerInfo.Instance.pvpEnabled;

        confirm.interactable = cancel.interactable = false;
    }

    private void ConfirmSettings()
    {
        SetServerPassword(serverPassword.text);
        SetMaxPlayers(maxPlayers.text);

        if (serverVisibleToggle.isOn)
        {
            if (!NetworkDiscovery.Instance.serverVisible)
                NetworkDiscovery.Instance.AdvertiseServer();
        }
        else
        {
            if (NetworkDiscovery.Instance.serverVisible)
                NetworkDiscovery.Instance.StopDiscovery();
        }

        // If pvp is off, bullet ignores Player
        ServerInfo.Instance.SetPvpEnabled(pvpToggle.isOn);

        confirm.interactable = cancel.interactable = false;
    }
}
