using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    [Scene]
    public string sceneName;

    private EventSystem eventSystem;

    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene unloadedScene)
    {
        string targetName = System.IO.Path.GetFileNameWithoutExtension(sceneName);
        if (unloadedScene.name == targetName)
        {
            eventSystem.enabled = true;
        }
    }

    public void ChangeScene()
    {
        eventSystem = EventSystem.current;
        eventSystem.enabled = false;
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    public void LeaveServer()
    {
        Time.timeScale = 1;
        NetworkManager.singleton.StopHost();
    }
}
