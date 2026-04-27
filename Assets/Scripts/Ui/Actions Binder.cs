using UnityEngine;
using UnityEngine.InputSystem;

public class ActionsBinder : MonoBehaviour
{
    [SerializeField]
    private InputActionReference[] actions;

    [SerializeField]
    private string[] bindings;

    public void Bind()
    {
        for (int i = 0; i < actions.Length; i++)
        {
            var action = actions[i];
            var bindingIndex = action.action.bindings.IndexOf(x => x.path == bindings[i]);
            if (bindingIndex != -1)
                action.action.RemoveBindingOverride(bindingIndex);
        }
    }

    public void UnBind()
    {
        for (int i = 0; i < actions.Length; i++)
        {
            var action = actions[i];
            var bindingIndex = action.action.bindings.IndexOf(x => x.path == bindings[i]);
            if (bindingIndex != -1)
                action.action.ApplyBindingOverride(bindingIndex, "");
        }
    }
}
