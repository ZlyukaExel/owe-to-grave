using TMPro;
using UnityEngine;

public class CopyText : MonoBehaviour
{
    [SerializeField]
    private TMP_Text textField;

    public void Copy()
    {
        GUIUtility.systemCopyBuffer = textField.text;
    }
}
