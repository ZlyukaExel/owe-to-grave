using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;

public class LanguageChanger : MonoBehaviour
{
    private TMP_Dropdown dropdown;

    [SerializeField]
    private UnityEvent onLanguageChanged;

    private void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.value = PlayerPrefs.GetInt("Language", 0);
    }

    public void ChangeLanguage(int locale)
    {
        StartCoroutine(SetLocale(locale));
    }

    private IEnumerator SetLocale(int locale)
    {
        yield return LocalizationSettings.InitializationOperation;
        if (
            LocalizationSettings.SelectedLocale
            == LocalizationSettings.AvailableLocales.Locales[locale]
        )
            yield break;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[locale];
        PlayerPrefs.SetInt("Language", locale);
        onLanguageChanged.Invoke();
        print("Language changed");
    }
}
