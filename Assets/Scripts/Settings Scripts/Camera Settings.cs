using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CameraSettings : BaseSettingsPanel
{
    [Header("Camera UI")]
    [SerializeField]
    private Slider horSensSlider,
        vertSensSlider,
        horAdjSlider,
        vertAdjSlider;

    [SerializeField]
    private UiList autoRotationList;

    [Header("References")]
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

        horSensSlider.onValueChanged.AddListener(value => OnSettingChanged());
        vertSensSlider.onValueChanged.AddListener(value => OnSettingChanged());
        horAdjSlider.onValueChanged.AddListener(value => OnSettingChanged());
        vertAdjSlider.onValueChanged.AddListener(value => OnSettingChanged());

        if (autoRotationList != null)
            autoRotationList.onValueChanged.AddListener(OnSettingChanged);
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

        horSensSlider.value = PlayerPrefs.GetFloat("HorizontalSensivity", 20);
        vertSensSlider.value = PlayerPrefs.GetFloat("VerticalSensivity", 20);
        horAdjSlider.value = PlayerPrefs.GetFloat("HorizontalAdjustment", 10);
        vertAdjSlider.value = PlayerPrefs.GetFloat("VerticalAdjustment", 10);

        if (autoRotationList != null)
        {
            int autoRot = PlayerPrefs.GetInt("AutoRotationEnabled", 1);
            autoRotationList.SetValue(autoRot);
        }

        if (saveButton != null)
            saveButton.interactable = false;
        if (cancelButton != null)
            cancelButton.interactable = false;

        isInitializing = false;
    }

    public override void SaveSettings()
    {
        PlayerPrefs.SetFloat("HorizontalSensivity", horSensSlider.value);
        PlayerPrefs.SetFloat("VerticalSensivity", vertSensSlider.value);
        PlayerPrefs.SetFloat("HorizontalAdjustment", horAdjSlider.value);
        PlayerPrefs.SetFloat("VerticalAdjustment", vertAdjSlider.value);

        if (autoRotationList != null)
        {
            string val = autoRotationList.GetValue();
            PlayerPrefs.SetInt("AutoRotationEnabled", val == "Включена" ? 1 : 0);
        }

        CameraController camController = FindFirstObjectByType<CameraController>();
        if (camController != null)
        {
            camController.UpdateSettings();
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
