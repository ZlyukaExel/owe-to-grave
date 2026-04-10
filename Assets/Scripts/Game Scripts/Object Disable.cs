using UnityEngine;

public class ObjectDisable : MonoBehaviour
{
    [SerializeField]
    private bool isEnabled;
    private Renderer[] renderers;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetActive(bool isEnabled)
    {
        this.isEnabled = isEnabled;
        UpdateState();
    }

    public void UpdateState()
    {
        renderers ??= GetComponentsInChildren<Renderer>(true);

        foreach (var renderer in renderers)
        {
            renderer.enabled = isEnabled;
        }
    }
}
