using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    [Scene] public string sceneName;

    public void ChangeScene()
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    public void LeaveServer()
    {
        Time.timeScale = 1;
        NetworkManager.singleton.StopHost();
    }
}
