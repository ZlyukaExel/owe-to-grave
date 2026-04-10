using UnityEngine;

public class HideCursor : MonoBehaviour
{
    void OnEnable()
    {
        SetCursorVisible(false);
    }

    void OnDisable()
    {
        SetCursorVisible(true);
    }

    public void SetCursorVisible(bool isVisible)
    {
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
