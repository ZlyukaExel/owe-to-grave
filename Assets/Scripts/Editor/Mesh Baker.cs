using UnityEngine;

public class MeshBaker : MonoBehaviour
{
    [ContextMenu("Bake to Static Mesh")]
    public void Bake()
    {
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        Mesh bakedMesh = new Mesh();

        skinnedMeshRenderer.BakeMesh(bakedMesh);

        GameObject staticObject = new GameObject(gameObject.name + "_Static");
        staticObject.transform.position = transform.position;
        staticObject.transform.rotation = transform.rotation;

        staticObject.AddComponent<MeshFilter>().sharedMesh = bakedMesh;
        staticObject.AddComponent<MeshRenderer>().sharedMaterials =
            skinnedMeshRenderer.sharedMaterials;

        Debug.Log("Меш успешно запечен!");
    }
}
