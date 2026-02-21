using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputWithButtons : MonoBehaviour
{
    public int minValue = 0, maxValue = 1;
    public Button increaseButton, decreaseButton;
    private TMP_InputField inputField;


    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        increaseButton.onClick.AddListener(IncreaseValue);
        decreaseButton.onClick.AddListener(DecreaseValue);
        inputField.onValueChanged.AddListener(ClampValue);
        CheckValues();
    }

    private void ClampValue(string value)
    {
        inputField.text = Math.Clamp(int.Parse(value), minValue, maxValue).ToString();
        CheckValues();
    }

    private void IncreaseValue()
    {
        int currentValue = int.Parse(inputField.text);
        currentValue++;
        inputField.text = currentValue.ToString();
    }

    private void DecreaseValue()
    {
        int currentValue = int.Parse(inputField.text);
        currentValue--;
        inputField.text = currentValue.ToString();
    }

    private void CheckValues()
    {
        int currentValue = int.Parse(inputField.text);
        decreaseButton.interactable = currentValue != minValue;
        increaseButton.interactable = currentValue != maxValue;
    }
}
