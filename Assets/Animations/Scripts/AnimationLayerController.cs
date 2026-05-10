using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationLayerController : MonoBehaviour
{
    private Animator animator;

    [SerializeField]
    private string layerName;
    private int layerId;

    void Start()
    {
        animator = GetComponent<Animator>();
        layerId = animator.GetLayerIndex(layerName);
    }

    public void SetWeight(float weight)
    {
        animator.SetLayerWeight(layerId, weight);
    }
}
