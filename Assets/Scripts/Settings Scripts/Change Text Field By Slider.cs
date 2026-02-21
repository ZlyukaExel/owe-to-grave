using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTextBySlider : MonoBehaviour
{
    public TMP_InputField inputField;
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(float newValue)
    {
        inputField.text = newValue.ToString();
    }
}
