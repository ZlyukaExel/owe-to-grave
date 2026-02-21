using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectButtonByArrows : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (button)
            {
                if (EventSystem.current.currentSelectedGameObject == null)
                    button.Select();
            }
            else
            {
                Button button = GetComponentInChildren<Button>();
                if (button) button.Select();
            }

        }
    }
}