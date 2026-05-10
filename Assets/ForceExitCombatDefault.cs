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
            || animator.GetBool("inCombat")
            || animator.GetBool("isJumping")
        )
        {
            animator.SetTrigger("ForceExit");
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("ForceExit");
    }
}
