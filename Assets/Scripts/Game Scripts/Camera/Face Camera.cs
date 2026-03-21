using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform mainCamera;

    void LateUpdate()
    {
        if (mainCamera == null)
            mainCamera = Camera.main?.transform;
        else
            transform.rotation = mainCamera.rotation;
    }
}
