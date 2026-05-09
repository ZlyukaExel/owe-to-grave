using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TMP_InputField))]
public class OnInputFieldSubmit : MonoBehaviour
{
    [SerializeField]
    private InputActionReference submitAction;

    public UnityEvent onSubmit = new();

    void Start()
    {
        GetComponent<TMP_InputField>().onEndEdit.AddListener(OnEndEdit);
    }

    private void OnEndEdit(string text)
    {
#if UNITY_STANDALONE
        if (!submitAction.action.triggered)
            return;
#endif

        onSubmit.Invoke();
    }
}
