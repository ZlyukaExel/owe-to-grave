using UnityEngine;

public class ForceExitCombat : StateMachineBehaviour
{
    public int weaponId = 0;

    public override void OnStateUpdate(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex
    )
    {
        if (!animator.GetBool("inCombat") || weaponId != animator.GetInteger("weaponId"))
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
        // Debug.Log("Entering combat state");
        animator.ResetTrigger("ForceExit");
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Debug.Log("Exitting combat state");
        animator.ResetTrigger("ForceExit");
    }
}
