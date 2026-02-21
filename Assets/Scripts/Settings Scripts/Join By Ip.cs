using UnityEngine;
using TMPro;
using System.Net;
using Mirror.Discovery;
using Mirror;

public class JoinByIp : MonoBehaviour
{
    public TMP_InputField ipInputField;
    public TMP_InputField portInputField;

    public void Join()
    {
        if (!ipInputField.text.Contains(".")) return;
        if (ipInputField.text.Split(".").Length != 4) return;

        NetworkManager.singleton.networkAddress = ipInputField.text;
        if (Transport.active is PortTransport portTransport)
        {
            // use TryParse in case someone tries to enter non-numeric characters
            if (ushort.TryParse(portInputField.text, out ushort uport))
                portTransport.Port = uport;
        }
        NetworkManager.singleton.StartClient();
    }
}