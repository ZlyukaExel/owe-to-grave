using Mirror;
using UnityEngine;

public class Wheel : NetworkBehaviour
{
    private Transform wheelModel;
    public WheelCollider wheelCollider { get; private set; }
    public bool steerable;
    public bool motorized;
    public bool isBackWheel { get; private set; }
    public bool isGrounded => wheelCollider.isGrounded;
    private Vector3 position;
    private Quaternion rotation;

    private void Start()
    {
        isBackWheel = transform.localPosition.z < 0;
        wheelCollider = GetComponent<WheelCollider>();
        wheelModel = transform.Find("StandartWheel");
    }

    void Update()
    {
        if (!isOwned)
            return;
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelModel.transform.SetPositionAndRotation(position, rotation);
    }
}
