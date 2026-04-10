using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class AudioSettingsScript : MonoBehaviour
{
    [SerializeField]
    private Slider masterVolumeSlider,
        musicVolumeSlider,
        soundsVolumeSlider,
        voicesVolumeSlider;

    [SerializeField]
    private AudioMixer audioMixer;

    [SerializeField]
    private Button confirmButton,
        cancelButton;

    [SerializeField]
    private GameObject settingsMenu,
        saveOrGoScreen;

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
        // Slider changes volume instantly, so saving only PlayerPrefs
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SoundsVolume", soundsVolumeSlider.value);
        PlayerPrefs.SetFloat("VoicesVolume", voicesVolumeSlider.value);

        confirmButton.interactable = false;
        cancelButton.interactable = false;
    }

    public void ResetSettings()
    {
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 20);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", -55);
        soundsVolumeSlider.value = PlayerPrefs.GetFloat("SoundsVolume", -55);
        voicesVolumeSlider.value = PlayerPrefs.GetFloat("VoicesVolume", -55);

        confirmButton.interactable = false;
        cancelButton.interactable = false;
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
