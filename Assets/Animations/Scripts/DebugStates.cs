using UnityEngine;

public class AnimatorNodeDebug : StateMachineBehaviour
{
    public string stateName;

    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex
    )
    {
        Debug.Log($"Вход в {stateName}");
    }
}
