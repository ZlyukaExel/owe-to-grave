using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneCloser : MonoBehaviour
{
    [SerializeField]
    private InputActionReference cancelAction;

    void OnEnable()
    {
        cancelAction.action.performed += CloseScene;
    }

    void OnDisable()
    {
        cancelAction.action.performed -= CloseScene;
    }

    public void CloseScene(InputAction.CallbackContext ctx)
    {
        CloseScene();
    }

    public void CloseScene()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
    }
}
