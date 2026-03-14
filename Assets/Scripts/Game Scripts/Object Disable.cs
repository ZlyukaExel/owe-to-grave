using UnityEngine;

public class ObjectDisable : MonoBehaviour
{
    [SerializeField]
    private bool isEnabled;
    private Renderer[] renderers;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void SetActive(bool isEnabled)
    {
        this.isEnabled = isEnabled;
        UpdateState();
    }

    public void UpdateState()
    {
        foreach (var renderer in renderers)
        {
            renderer.enabled = isEnabled;
        }
    }
}
