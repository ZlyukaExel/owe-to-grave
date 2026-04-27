using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CameraSettings : MonoBehaviour
{
    [SerializeField]
    private Slider horSensSlider,
        vertSensSlider,
        horAdjSlider,
        vertAdjSlider;

    [SerializeField]
    private Toggle autoRotationToggle;

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

    public void ConfirmSettings()
    {
        PlayerPrefs.SetFloat("HorizontalSensivity", horSensSlider.value);
        PlayerPrefs.SetFloat("VerticalSensivity", vertSensSlider.value);
        PlayerPrefs.SetFloat("HorizontalAdjustment", horAdjSlider.value);
        PlayerPrefs.SetFloat("VerticalAdjustment", vertAdjSlider.value);
        PlayerPrefs.SetInt("AutoRotationEnabled", autoRotationToggle.isOn ? 1 : 0);

        FindFirstObjectByType<CameraController>().UpdateSettings();

        confirmButton.interactable = cancelButton.interactable = false;
    }

    public void ResetSettings()
    {
        horSensSlider.value = PlayerPrefs.GetFloat("HorizontalSensivity", 20);
        vertSensSlider.value = PlayerPrefs.GetFloat("VerticalSensivity", 20);
        horAdjSlider.value = PlayerPrefs.GetFloat("HorizontalAdjustment", 10);
        vertAdjSlider.value = PlayerPrefs.GetFloat("VerticalAdjustment", 10);
        if (PlayerPrefs.GetInt("AutoRotationEnabled", 1) == 1)
            autoRotationToggle.isOn = true;
        else
            autoRotationToggle.isOn = false;

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
