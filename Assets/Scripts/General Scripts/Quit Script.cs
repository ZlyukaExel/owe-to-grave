using UnityEngine;
using UnityEngine.InputSystem;

public class QuitScript : MonoBehaviour
{
#if UNITY_ANDROID
    private float backButtonPressTime = 0f;
    private readonly float backButtonMaxTime = 1f;

    [SerializeField]
    private GameObject confirmExitScreen;

    [SerializeField]
    private InputActionReference cancelAction;

    void Update()
    {
        if (confirmExitScreen && cancelAction.action.triggered)
        {
            if (Time.time - backButtonPressTime < backButtonMaxTime)
            {
                confirmExitScreen.SetActive(true);
            }
            else
            {
                backButtonPressTime = Time.time;
            }
        }
    }
#endif

    public void QuitTheGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
