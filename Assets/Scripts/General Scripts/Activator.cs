using UnityEngine;

public class Activator : MonoBehaviour
{
    public GameObject objectToDeactivate;

    public void Activate()
    {
        if (objectToDeactivate)
            objectToDeactivate.SetActive(true);
        else
            gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        if (objectToDeactivate)
            objectToDeactivate.SetActive(false);
        else
            gameObject.SetActive(false);
    }
}
