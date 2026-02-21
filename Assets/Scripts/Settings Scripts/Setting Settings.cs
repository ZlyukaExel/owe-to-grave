using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;


public class PlayerPrefsSet : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    void Start()
    {
#if UNITY_ANDROID
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif

        if (!PlayerPrefs.HasKey("Nickname"))
            PlayerPrefs.SetString("Nickname", "Player");

        if (!PlayerPrefs.HasKey("Server Max Connections"))
            PlayerPrefs.SetInt("Server Max Connections", 8);

        //Graphics
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Graphics quality settings", 0));
        QualitySettings.shadowResolution = (ShadowResolution)PlayerPrefs.GetInt("Shadows quality settings", 0);
        Application.targetFrameRate = PlayerPrefs.GetInt("FPS lock", 30);

        //Language
        if (!PlayerPrefs.HasKey("Language"))
        {
            string lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            switch (lang)
            {
                case "ru": { PlayerPrefs.SetInt("Language", 1); break; }
                default: { PlayerPrefs.SetInt("Language", 0); break; }
            }
        }
        StartCoroutine(SetLanguage(PlayerPrefs.GetInt("Language")));

        //Volume
        audioMixer.SetFloat("MasterVolume", PlayerPrefs.GetFloat("MasterVolume", 20));
        audioMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", -55));
        audioMixer.SetFloat("SoundsVolume", PlayerPrefs.GetFloat("SoundsVolume", -55));
        audioMixer.SetFloat("VoicesVolume", PlayerPrefs.GetFloat("VoicesVolume", -55));
    }

    private IEnumerator SetLanguage(int lang)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[lang];
    }
}
