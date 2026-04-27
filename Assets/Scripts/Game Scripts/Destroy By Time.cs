using UnityEngine;

public class DestroyByTime : MonoBehaviour
{
    public float destroyTime;

    void Start()
    {
        if (destroyTime != 0)
            Destroy(gameObject, destroyTime);
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}
