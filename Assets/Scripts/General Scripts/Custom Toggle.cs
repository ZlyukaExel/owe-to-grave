using UnityEngine;
using UnityEngine.UI;

public class CustomToggle : MonoBehaviour
{
    [SerializeField] private bool _isOn;
    [SerializeField] private Image targetImage; 
    [SerializeField] private Sprite isOnSprite, isOffSprite; 

    public bool isOn
    {
        get => _isOn;
        set
        {
            if (_isOn != value)
            {
                _isOn = value;
                UpdateSprite();
            }
        }
    }

    private void UpdateSprite()
    {
        targetImage.sprite = _isOn ? isOnSprite : isOffSprite;
    }

    private void OnValidate()
    {
        if (targetImage != null)
            targetImage.sprite = _isOn ? isOnSprite : isOffSprite;
    }
}
