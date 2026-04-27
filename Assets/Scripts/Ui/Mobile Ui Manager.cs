using UnityEngine;
using UnityEngine.Events;

public class MobileUiManager : MonoBehaviour
{
    public bool isMobile = false;

    [SerializeField]
    private UnityEvent onMobileActivated,
        onMobileDeactivated,
        onPcActivated,
        onPcDeactivated;

    void Awake()
    {
#if UNITY_EDITOR
        // Set in Inspector
#elif UNITY_ANDROID
        isMobile = true;
#elif UNITY_STANDALONE
        isMobile = false;
#endif

        UpdateUi();
    }

    public void UpdateUi()
    {
        SetMobileUiVisible(isMobile);
    }

    public void SetMobileUiVisible(bool isVisible)
    {
        isMobile = isVisible;
        if (isVisible)
        {
            onPcDeactivated.Invoke();
            onMobileActivated.Invoke();
        }
        else
        {
            onMobileDeactivated.Invoke();
            onPcActivated.Invoke();
        }
    }
}
