using UnityEngine;

public class NameTag : MonoBehaviour
{
    public GameObject nameTag;

    void OnTriggerEnter(Collider other)
    {
        SetNameTagActive(other, true);
    }

    void OnTriggerExit(Collider other)
    {
        SetNameTagActive(other, false);
    }

    private void SetNameTagActive(Collider other, bool isActive)
    {
        if (other.transform.root.GetComponentInChildren<NameTag>(true) is NameTag otherNameTagEnabler)
        {
            otherNameTagEnabler.nameTag.SetActive(isActive);
        }
    }
}