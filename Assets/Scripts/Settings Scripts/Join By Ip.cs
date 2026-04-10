using Mirror;
using TMPro;
using UnityEngine;

public class JoinByIp : MonoBehaviour
{
    public TMP_InputField ipInputField;

    public void Join()
    {
        string[] parts = ipInputField.text.Split(":");
        if (parts.Length != 2)
            return;
        if (parts[0].Split(".").Length != 4)
            return;

        NetworkManager.singleton.networkAddress = ipInputField.text;
        if (Transport.active is PortTransport portTransport)
        {
            // use TryParse in case someone tries to enter non-numeric characters
            if (ushort.TryParse(parts[1], out ushort uport))
                portTransport.Port = uport;
            else
                return;
        }
        NetworkManager.singleton.StartClient();
    }
}
