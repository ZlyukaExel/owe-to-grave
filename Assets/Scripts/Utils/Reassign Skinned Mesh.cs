using UnityEngine;

public class ReassignSkinnedMesh : MonoBehaviour
{
    public SkinnedMeshRenderer targetRenderer;

    [ContextMenu("Rebind Mesh Completely")]
    void RebindMesh()
    {
        SkinnedMeshRenderer shirtRenderer = GetComponent<SkinnedMeshRenderer>();

        shirtRenderer.bones = targetRenderer.bones;
        shirtRenderer.rootBone = targetRenderer.rootBone;

        DestroyImmediate(this);
    }
}
