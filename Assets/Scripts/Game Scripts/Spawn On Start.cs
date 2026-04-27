using UnityEngine;

public class SpawnOnStart : MonoBehaviour
{
    public GameObject[] prefabs;
    void Start()
    {
        foreach (var prefab in prefabs)
        {
            Instantiate(prefab);
        }
    }
}
