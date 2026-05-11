using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ChangingColorToggle : MonoBehaviour
{
    public Color normalOnColor,
        normalOffColor,
        selectedOnColor,
        selectedOffColor;
    private Toggle toggle;

    void OnEnable()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(ChangeColor);
        ChangeColor(toggle.isOn);
    }

    void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(ChangeColor);
    }

    public void ChangeColor(bool isOn)
    {
        var colors = toggle.colors;
        colors.normalColor = isOn ? normalOnColor : normalOffColor;
        colors.selectedColor = isOn ? selectedOnColor : selectedOffColor;
        toggle.colors = colors;
    }
}
