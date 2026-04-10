using UnityEngine;

public class NameTagTrigger : MonoBehaviour
{
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
        if (other.transform.root.TryGetComponent(out NameTag nameTag))
        {
            nameTag.nameTagObject.SetActive(isActive);
        }
    }
}
