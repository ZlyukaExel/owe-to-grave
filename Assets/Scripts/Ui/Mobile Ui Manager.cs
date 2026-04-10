using UnityEngine;

public class MobileUiManager : MonoBehaviour
{
    [SerializeField]
    private RectTransform[] mobileUiElements;

    [SerializeField]
    private RectTransform[] pcUiElements;

#if UNITY_EDITOR
    [SerializeField]
    private bool emulateMobile;
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
    void Awake()
    {
#if UNITY_EDITOR
        SetMobileUiVisible(emulateMobile);
#else
        SetMobileUiVisible(false);
#endif
    }
#endif

    public void SetMobileUiVisible(bool isVisible)
    {
        foreach (var uiElement in mobileUiElements)
        {
            uiElement.gameObject.SetActive(isVisible);
        }

        foreach (var uiElement in pcUiElements)
        {
            uiElement.gameObject.SetActive(!isVisible);
        }
    }
}
