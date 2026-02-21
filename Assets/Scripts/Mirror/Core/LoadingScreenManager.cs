using UnityEngine;

[DisallowMultipleComponent]
public class LoadingScreenManager : MonoBehaviour
{
    private Animator animator;
    public AsyncOperation operation;

    void Awake()
    {
        if (FindObjectsByType<LoadingScreenManager>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 1)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        animator = GetComponent<Animator>();
    }

    public void CheckLoading()
    {
        if (operation != null && operation.isDone)
        {
            animator.Play("Fade In");
            operation = null;
        }
    }
}
