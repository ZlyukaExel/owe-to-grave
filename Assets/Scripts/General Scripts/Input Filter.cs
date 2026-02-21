using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

[RequireComponent(typeof(TMP_InputField))]
public class InputFilter : MonoBehaviour
{
    private TMP_InputField inputField;
    public string[] blackList;
    private string blackListSingleString;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(FilterRichText);
        blackListSingleString = string.Join("|", blackList);
    }

    private void FilterRichText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        text = Regex.Replace(text, blackListSingleString, "", RegexOptions.IgnoreCase);

        inputField.text = text;
    }
}