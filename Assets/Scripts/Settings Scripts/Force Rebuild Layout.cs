using UnityEngine;
using UnityEngine.UI;

public class ForceRebuildLayout : MonoBehaviour
{
    [SerializeField]
    private RectTransform rectTransform;

    public void Rebuild()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}
