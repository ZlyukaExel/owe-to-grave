using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GraphicsSettingsScript : MonoBehaviour
{
    [SerializeField]
    private UiList graphicsQualityText,
        shadowsQualityText,
        fpsLockText;

    [SerializeField]
    private Button confirmButton,
        cancelButton;

    [SerializeField]
    private GameObject saveOrGoScreen,
        settingsMenu;

    [SerializeField]
    private UnityEvent onExit;

    [SerializeField]
    private Selectable defaultSelectable;

    void Start()
    {
        ResetSettings();
    }

    public void ResetSettings()
    {
        graphicsQualityText.SetValue(PlayerPrefs.GetInt("Graphics quality settings", 0));
        shadowsQualityText.SetValue(PlayerPrefs.GetInt("Shadows quality settings", 0));
        fpsLockText.SetValue(PlayerPrefs.GetInt("FPS lock", 30).ToString());

        confirmButton.interactable = cancelButton.interactable = false;
    }

    public void SaveChanges()
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

        PlayerPrefs.SetInt("FPS lock", int.Parse(fpsLockText.GetValue()));
        Application.targetFrameRate = PlayerPrefs.GetInt("FPS lock", 30);

        confirmButton.interactable = cancelButton.interactable = false;
    }

    public void GoBack()
    {
        if (confirmButton.interactable)
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
        ResetSettings();
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
