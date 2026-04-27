using UnityEngine;
using UnityEngine.InputSystem;

namespace Mirror
{
    public class ToggleHotkey : MonoBehaviour
    {
        public InputActionReference debugAction;
        public GameObject ToToggle;

        void Update()
        {
            if (debugAction.action.IsPressed())
                ToToggle.SetActive(!ToToggle.activeSelf);
        }
    }
}
