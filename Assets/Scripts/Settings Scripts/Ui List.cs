using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UiList : MonoBehaviour
{
    [SerializeField]
    private string[] list;

    [SerializeField]
    private Button nextButton,
        previousButton;

    [SerializeField]
    private RectTransform content;
    public UnityEvent onValueChanged;

    [SerializeField]
    private TMP_Text text;

    void Awake()
    {
        if (!text)
            text = GetComponent<TMP_Text>();
        if (!text)
            Debug.LogWarning("No text component found or set");

        nextButton.onClick.AddListener(Next);
        previousButton.onClick.AddListener(Previous);
        onValueChanged.AddListener(CheckButtons);
    }

    public void SetValue(string value)
    {
        if (!list.Contains(value))
        {
            Debug.LogWarning($"List doesn't contain {value}");
            return;
        }
        if (text)
            text.text = value;
        onValueChanged.Invoke();
    }

    public void SetValue(int value)
    {
        if (value < 0 || value > list.Length - 1)
        {
            Debug.LogWarning($"Index {value} is out of bounds");
            return;
        }
        if (text)
            text.text = list[value];
        onValueChanged.Invoke();
    }

    public void Next()
    {
        int index = Array.IndexOf(list, text.text);
        if (index >= list.Length - 1)
            return;
        text.text = list[Array.IndexOf(list, text.text) + 1];
        onValueChanged.Invoke();
    }

    public void Previous()
    {
        int index = Array.IndexOf(list, text.text);
        if (index <= 0)
            return;
        text.text = list[index - 1];
        onValueChanged.Invoke();
    }

    private void CheckButtons()
    {
        int index = Array.IndexOf(list, text.text);
        previousButton.interactable = index > 0;
        nextButton.interactable = index < list.Length - 1;
        if (content)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }
    }

    public string GetValue()
    {
        return text.text;
    }
}
