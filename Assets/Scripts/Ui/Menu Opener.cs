using UnityEngine;
using UnityEngine.Events;

public class MenuOpener : MonoBehaviour
{
    private bool isOpened;
    private Animator animator;
    public string openMenuState,
        closeMenuState;

    public UnityEvent openAction,
        closeAction;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenMenu()
    {
        OpenMenu(!isOpened);
    }

    public void OpenMenuImmediate(bool open)
    {
        OpenMenu(open, true);
    }

    public void OpenMenu(bool open)
    {
        OpenMenu(open, false);
    }

    public void OpenMenu(bool open, bool immediate)
    {
        if (open == isOpened)
            return;

        isOpened = open;

        if (animator)
        {
            if (immediate)
            {
                if (string.IsNullOrEmpty(openMenuState) || string.IsNullOrEmpty(closeMenuState))
                    Debug.LogError("Open or close menu state is not set for " + gameObject);

                string stateName = isOpened ? openMenuState : closeMenuState;
                animator.Play(stateName, 0, 1.0f);
            }
            else
            {
                string triggerName = isOpened ? "Open" : "Close";
                animator.SetTrigger(triggerName);
            }
        }

        UnityEvent currentAction = isOpened ? openAction : closeAction;
        currentAction?.Invoke();
    }
}
