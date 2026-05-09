using UnityEngine;
using UnityEngine.UI;

public abstract class BaseSettingsPanel : MonoBehaviour
{
    [Header("Base UI Controls")]
    public Button saveButton;
    public Button cancelButton;

    protected virtual void Awake()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveSettings);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(ResetSettings);
    }

    protected virtual void OnDestroy()
    {
        if (saveButton != null)
            saveButton.onClick.RemoveListener(SaveSettings);
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(ResetSettings);
    }

    protected void MarkAsChanged()
    {
        if (saveButton != null)
            saveButton.interactable = true;
        if (cancelButton != null)
            cancelButton.interactable = true;
    }

    public abstract void LoadSettings();
    public abstract void SaveSettings();
    public abstract void ResetSettings();
}
