using TMPro;
using UnityEngine;
using System;
using Mirror;

public class PingCounter : MonoBehaviour
{
    private TMP_Text textField;

    private void Start()
    {
        if (NetworkServer.active) { Destroy(this); return; }

        textField = GetComponent<TMP_Text>();
    }

    private void LateUpdate()
    {
        if (Time.frameCount % 3 == 0)
        {
            textField.text = $"{Math.Round(NetworkTime.rtt * 1000)} ms";
        }
    }
}