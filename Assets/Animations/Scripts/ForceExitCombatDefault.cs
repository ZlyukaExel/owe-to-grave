using UnityEngine;

public class ForceExitCombatDefault : StateMachineBehaviour
{
    public override void OnStateUpdate(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex
    )
    {
        if (
            animator.GetBool("isAttacking")
            || animator.GetBool("isAiming")
            || animator.GetBool("inCombat")
            || animator.GetBool("isJumping")
        )
        {
            animator.SetTrigger("ForceExit");
        }
    }

    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex
    )
    {
        // Debug.Log("Entering default state");
        animator.ResetTrigger("ForceExit");
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Debug.Log("Exitting default state");
        animator.ResetTrigger("ForceExit");
    }
}
