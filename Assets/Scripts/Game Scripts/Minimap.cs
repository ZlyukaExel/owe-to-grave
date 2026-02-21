using UnityEngine;

public class Minimap : MonoBehaviour
{
    [HideInInspector]
    public Transform target;
    private Transform playerMarker,
        minimapBorder;
    public Transform PlayerMarker
    {
        set
        {
            playerMarker = value;
            minimapBorder = value.parent.Find("Border");
        }
    }
    private Vector3 eu = new(90, 0, 0);
    public float angleY,
        cameraSize = 70;
    private Camera cameraComponent;

    void Start()
    {
        cameraComponent = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (!target)
            return;

        eu.z = -angleY;
        transform.eulerAngles = eu;
        transform.position = new Vector3(target.position.x, 100, target.position.z);

        playerMarker.eulerAngles = Vector3.forward * (angleY - target.eulerAngles.y);

        minimapBorder.eulerAngles = Vector3.forward * angleY;

        if (Mathf.Abs(cameraSize - cameraComponent.orthographicSize) > 0.01)
        {
            cameraComponent.orthographicSize = Mathf.Lerp(
                cameraComponent.orthographicSize,
                cameraSize,
                Time.unscaledDeltaTime
            );
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
