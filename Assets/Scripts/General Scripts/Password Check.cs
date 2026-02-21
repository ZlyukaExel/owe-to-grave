using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Mirror.Discovery;

public class PasswordCheck : MonoBehaviour
{
    [HideInInspector] public string password;
    [HideInInspector] public UnityEvent action;
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private CustomNetworkDiscovery customNetworkDiscovery;

    public void OnPasswordEnter(string text)
    {
        if (string.IsNullOrWhiteSpace(text)

#if UNITY_STANDALONE
        || !Input.GetKeyDown(KeyCode.Return)
#endif

        )
            return;

        CheckPassword();
    }

    public void CheckPassword()
    {
        if (passwordField.text == password)
            action.Invoke();
        else
        {
            label.text = "Incorrect password!";
            label.color = Color.red;
        }
    }

    public void CancelConnect()
    {
        passwordField.text = "";
        passwordField.onEndEdit.RemoveAllListeners();
        label.text = "This server reqiers password";
        label.color = Color.white;
        gameObject.SetActive(false);
        customNetworkDiscovery.StartDiscovery();
    }
}
