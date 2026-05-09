using UnityEngine;

public class WindowManager : MonoBehaviour
{
    public enum WindowMode
    {
        Fullscreen,
        Windowed,
    }

    public enum ResolutionOption
    {
        Res_1280x720,
        Res_1366x768,
        Res_1600x900,
        Res_1920x1080,
        Res_2560x1440,
        Res_3840x2160,
    }

    [Header("Window settings")]
    public WindowMode currentMode = WindowMode.Windowed;
    public ResolutionOption currentResolution = ResolutionOption.Res_1920x1080;

    public static WindowManager Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        int width = 1920;
        int height = 1080;
        switch (currentResolution)
        {
            case ResolutionOption.Res_1280x720:
                width = 1280;
                height = 720;
                break;
            case ResolutionOption.Res_1366x768:
                width = 1366;
                height = 768;
                break;
            case ResolutionOption.Res_1600x900:
                width = 1600;
                height = 900;
                break;
            case ResolutionOption.Res_1920x1080:
                width = 1920;
                height = 1080;
                break;
            case ResolutionOption.Res_2560x1440:
                width = 2560;
                height = 1440;
                break;
            case ResolutionOption.Res_3840x2160:
                width = 3840;
                height = 2160;
                break;
        }

        bool isFullscreen = currentMode == WindowMode.Fullscreen;

        Screen.SetResolution(
            width,
            height,
            isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed
        );
    }

    public void SetResolutionFromUI(int resolutionIndex)
    {
        currentResolution = (ResolutionOption)resolutionIndex;
        ApplySettings();
    }

    public void SetFullscreenFromUI(bool isFull)
    {
        currentMode = isFull ? WindowMode.Fullscreen : WindowMode.Windowed;
        ApplySettings();
    }
}
