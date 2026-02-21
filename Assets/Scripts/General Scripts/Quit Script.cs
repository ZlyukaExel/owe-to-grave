using UnityEngine;

public class QuitScript : MonoBehaviour
{
#if UNITY_ANDROID
    private float backButtonPressTime = 0f, backButtonMaxTime = 1f;
    public GameObject confirmExitScreen;

    void Update()
    {
        if (confirmExitScreen && Input.GetKeyDown(KeyCode.Escape))
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
