using UnityEngine;
using TMPro;

public class NickChanger : MonoBehaviour
{
    private TMP_InputField inputField;

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.text = PlayerPrefs.GetString("Nickname");
        inputField.onEndEdit.AddListener(ChangeNickname);
    }

    private void ChangeNickname(string newNickname)
    {
        if (string.IsNullOrWhiteSpace(newNickname)

#if UNITY_STANDALONE
        || !Input.GetKeyDown(KeyCode.Return)
#endif

        )
        {
            inputField.text = PlayerPrefs.GetString("Nickname");
            return;
        }
        PlayerPrefs.SetString("Nickname", newNickname);
    }
}