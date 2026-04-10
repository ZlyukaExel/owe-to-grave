using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TMP_InputField))]
public class NickChanger : MonoBehaviour
{
    private TMP_InputField inputField;

    [SerializeField]
    private InputActionReference submitAction;

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
            || !submitAction.action.triggered
#endif

        )
        {
            inputField.text = PlayerPrefs.GetString("Nickname");
            return;
        }
        PlayerPrefs.SetString("Nickname", newNickname);
    }
}
