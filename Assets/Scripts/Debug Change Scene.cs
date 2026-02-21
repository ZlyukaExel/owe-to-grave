using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugChangeScene : MonoBehaviour
{
    [Scene] public string scene;

    public void ChangeScene()
    {
        SceneManager.LoadScene(scene);
    }
}
