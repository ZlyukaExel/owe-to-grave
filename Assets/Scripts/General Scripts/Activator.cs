using UnityEngine;

public class Activator : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToDeactivate;

    void Awake()
    {
        if (!objectToDeactivate)
            objectToDeactivate = gameObject;
    }

    public void Activate()
    {
        objectToDeactivate.SetActive(true);
    }

    public void Deactivate()
    {
        objectToDeactivate.SetActive(false);
    }
}
