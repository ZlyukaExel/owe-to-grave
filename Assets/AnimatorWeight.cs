using UnityEngine;

public class AnimatorWeight : StateMachineBehaviour
{
    [SerializeField]
    private string weightParamName;

    public override void OnStateUpdate(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex
    )
    {
        float weight = animator.GetFloat(weightParamName);
        animator.SetLayerWeight(layerIndex, weight);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log(animator.GetFloat(weightParamName));
    }
}
