using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class TimeScale : MonoBehaviour
{
    private Slider slider;

    void OnEnable()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnTimeScaleChanged);
    }

    private void OnTimeScaleChanged(float newValue)
    {
        Time.timeScale = newValue;
    }
}
