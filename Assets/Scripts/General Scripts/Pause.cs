using UnityEngine;

public class Pause : MonoBehaviour
{
    void Update()
    {
        if (ServerInfo.Instance.players.Count <= 1)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    void OnDisable()
    {
        Time.timeScale = 1;
    }

    void OnDestroy()
    {
        Time.timeScale = 1;
    }
}