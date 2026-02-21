using UnityEngine;

public class SwapGameObjects : MonoBehaviour
{
    public GameObject object1,
        object2;

    public void SwapObjects()
    {
        object1.SetActive(object2.activeSelf);
        object2.SetActive(!object2.activeSelf);
    }
}
