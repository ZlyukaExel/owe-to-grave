using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DisabledImageAndAnimation : MonoBehaviour
{
    public bool moveToEnd = false;
    private Animator animator;

    [SerializeField]
    private string animationName;

    [SerializeField]
    private string animationSpeedVariable;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        animator.SetFloat(animationSpeedVariable, moveToEnd ? 1 : -1);
        // Settinng not complete so animation won't start again
        animator.Play(animationName, 0, moveToEnd ? 0.9f : 0.1f);
    }
}
