using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    private TMP_Text textField;
    private float deltaTime = 0;

    void Start()
    {
        textField = GetComponent<TMP_Text>();
    }

    private void LateUpdate()
    {
        if (Time.frameCount % 3 == 0)
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            textField.text = $"{Mathf.Round(1 / deltaTime)} fps";
        }
    }
}
