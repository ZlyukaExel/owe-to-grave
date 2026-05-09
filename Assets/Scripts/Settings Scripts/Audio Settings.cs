using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class AudioSettings : BaseSettingsPanel
{
    [Header("Audio UI")]
    [SerializeField]
    private Slider masterVolumeSlider,
        musicVolumeSlider,
        soundsVolumeSlider,
        voicesVolumeSlider;

    [Header("References")]
    [SerializeField]
    private AudioMixer audioMixer;

    [SerializeField]
    private GameObject settingsMenu,
        saveOrGoScreen;

    [SerializeField]
    private UnityEvent onExit;

    [SerializeField]
    private Selectable defaultSelectable;

    private bool isInitializing = false;

    protected override void Awake()
    {
        base.Awake();

        masterVolumeSlider.onValueChanged.AddListener(value => OnSettingChanged());
        musicVolumeSlider.onValueChanged.AddListener(value => OnSettingChanged());
        soundsVolumeSlider.onValueChanged.AddListener(value => OnSettingChanged());
        voicesVolumeSlider.onValueChanged.AddListener(value => OnSettingChanged());
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

        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 20);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", -55);
        soundsVolumeSlider.value = PlayerPrefs.GetFloat("SoundsVolume", -55);
        voicesVolumeSlider.value = PlayerPrefs.GetFloat("VoicesVolume", -55);

        if (saveButton != null)
            saveButton.interactable = false;
        if (cancelButton != null)
            cancelButton.interactable = false;

        isInitializing = false;
    }

    public override void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SoundsVolume", soundsVolumeSlider.value);
        PlayerPrefs.SetFloat("VoicesVolume", voicesVolumeSlider.value);

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
