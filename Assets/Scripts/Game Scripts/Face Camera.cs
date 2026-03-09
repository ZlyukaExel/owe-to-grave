using UnityEngine;

public class FaceCamera : MonoBehaviour // TODO: fix on start
{
    private Transform mainCamera;

    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
            transform.rotation = mainCamera.rotation;
    }
}
