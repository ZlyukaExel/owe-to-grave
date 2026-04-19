using UnityEngine;
using UnityEngine.Events;

public class MenuOpener : MonoBehaviour
{
    private bool isOpened;
    private Animator animator;

    [SerializeField]
    private UnityEvent openAction,
        closeAction;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenMenu()
    {
        if (isOpened)
        {
            if (animator)
                animator.SetTrigger("Close");
            closeAction.Invoke();
        }
        else
        {
            if (animator)
                animator.SetTrigger("Open");
            openAction.Invoke();
        }

        isOpened = !isOpened;
    }

    public void OpenMenu(bool open)
    {
        if (open && isOpened)
            return;
        if (!open && !isOpened)
            return;
        OpenMenu();
    }
}
