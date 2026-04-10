using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ConnectToServerButton : MonoBehaviour
{
    [HideInInspector]
    public ServerResponse info;

    [HideInInspector]
    public CustomNetworkDiscovery customNetworkDiscovery;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        customNetworkDiscovery.StopDiscovery();
        if (string.IsNullOrEmpty(info.password))
            Connect();
        else
        {
            customNetworkDiscovery.passwordScreen.SetActive(true);
            customNetworkDiscovery.passwordScreen.GetComponent<PasswordCheck>().password =
                info.password;
            customNetworkDiscovery.passwordScreen.GetComponent<PasswordCheck>().action =
                new UnityEvent();
            customNetworkDiscovery
                .passwordScreen.GetComponent<PasswordCheck>()
                .action.AddListener(Connect);
        }
    }

    private void Connect()
    {
        NetworkManager.singleton.StartClient(info.uri);
    }
}
