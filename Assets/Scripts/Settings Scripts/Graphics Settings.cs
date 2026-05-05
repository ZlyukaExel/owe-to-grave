using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GraphicsSettings : BaseSettingsPanel
{
    [Header("Graphics UI")]
    [SerializeField]
    private UiList graphicsQualityText,
        shadowsQualityText,
        fpsLockText;

    [Header("Window UI")]
    [SerializeField]
    private TMP_Dropdown resolutionDropdown,
        windowModeDropdown;

    [Header("Navigation")]
    [SerializeField]
    private GameObject saveOrGoScreen,
        settingsMenu;

    [SerializeField]
    private UnityEvent onExit;

    [SerializeField]
    private Selectable defaultSelectable;

    private bool isInitializing = false;

    protected override void Awake()
    {
        base.Awake();
        graphicsQualityText.onValueChanged.AddListener(OnSettingChanged);
        shadowsQualityText.onValueChanged.AddListener(OnSettingChanged);
        fpsLockText.onValueChanged.AddListener(OnSettingChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(value => OnSettingChanged());
        if (windowModeDropdown != null)
            windowModeDropdown.onValueChanged.AddListener(value => OnSettingChanged());
    }

    private void Start()
    {
        LoadSettings();
    }

    private void OnSettingChanged()
    {
        if (!isInitializing)
            MarkAsChanged();
    }

    public override void LoadSettings()
    {
        isInitializing = true;

        graphicsQualityText.SetValue(PlayerPrefs.GetInt("Graphics quality settings", 0));
        shadowsQualityText.SetValue(PlayerPrefs.GetInt("Shadows quality settings", 0));
        fpsLockText.SetValue(PlayerPrefs.GetInt("FPS lock", 30).ToString());

        if (resolutionDropdown != null)
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 3);

        if (windowModeDropdown != null)
            windowModeDropdown.value = PlayerPrefs.GetInt("FullscreenMode", 1);

        if (saveButton != null)
            saveButton.interactable = false;
        if (cancelButton != null)
            cancelButton.interactable = false;

        isInitializing = false;
    }

    public override void SaveSettings()
    {
        if (Enum.TryParse(graphicsQualityText.GetValue(), out QualityEnum graphicsQuality))
        {
            PlayerPrefs.SetInt("Graphics quality settings", (int)graphicsQuality);
            QualitySettings.SetQualityLevel((int)graphicsQuality);
        }

        if (Enum.TryParse(shadowsQualityText.GetValue(), out QualityEnum shadowsQuality))
        {
            PlayerPrefs.SetInt("Shadows quality settings", (int)shadowsQuality);
            QualitySettings.shadowResolution = (ShadowResolution)shadowsQuality;
        }

        int fps = int.Parse(fpsLockText.GetValue());
        PlayerPrefs.SetInt("FPS lock", fps);
        Application.targetFrameRate = fps;

        if (resolutionDropdown != null && windowModeDropdown != null)
        {
            PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
            PlayerPrefs.SetInt("FullscreenMode", windowModeDropdown.value);

            if (WindowManager.Instance != null)
            {
                WindowManager.Instance.SetResolutionFromUI(resolutionDropdown.value);
                WindowManager.Instance.SetFullscreenFromUI(windowModeDropdown.value == 0);
            }
        }

        if (saveButton != null)
            saveButton.interactable = false;
    }

    public override void ResetSettings()
    {
        LoadSettings();
    }

    public void GoBack()
    {
        if (saveButton != null && saveButton.interactable)
        {
            saveOrGoScreen.SetActive(true);
            saveOrGoScreen.transform.Find("NoButton").GetComponent<Button>().Select();
        }
        else
        {
            onExit.Invoke();
            gameObject.SetActive(false);
            settingsMenu.SetActive(true);
        }
    }

    public void ExitMenu()
    {
        LoadSettings();
        onExit.Invoke();
        gameObject.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void SelectIfActive()
    {
        if (gameObject.activeInHierarchy)
            defaultSelectable.Select();
    }
}

public enum QualityEnum
{
    Low,
    Medium,
    High,
}
