using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
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
            animator.SetTrigger("Close");
            closeAction.Invoke();
        }
        else
        {
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
