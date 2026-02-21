using Mirror;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextFieldIp : MonoBehaviour
{
    [Server]
    void Start()
    {
        string ip = NetworkManager.singleton.networkAddress;
        // print(Transport.active.ServerUri().Host);
        ushort port = 7780;
        if (Transport.active is PortTransport portTransport)
            port = portTransport.Port;
        GetComponent<TMP_Text>().text = ip + ":" + port;
    }
}
