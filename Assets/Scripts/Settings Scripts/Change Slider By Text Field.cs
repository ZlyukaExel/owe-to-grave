using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSliderByTextField : MonoBehaviour
{
    public Slider slider;
    private TMP_InputField inputField;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(string newValue)
    {
        slider.value = int.Parse(newValue);
    }
}
