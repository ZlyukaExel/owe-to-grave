using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemMapDisabler : MonoBehaviour
{
    [SerializeField]
    private string mapName;

    public void SetMapEnabled(bool enabled)
    {
        if (enabled)
            InputSystem.actions.FindActionMap(mapName).Enable();
        else
            InputSystem.actions.FindActionMap(mapName).Disable();
    }
}
