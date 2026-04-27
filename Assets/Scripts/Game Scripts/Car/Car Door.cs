using Mirror;
using UnityEngine;

[RequireComponent(typeof(HingeJoint))]
public class CarDoor : InteractiveObject
{
    [SyncVar(hook = nameof(OnStateChanged))]
    public bool isDestroyed;

    [SerializeField]
    private float maxAngle = 60;

    [SerializeField]
    private GameObject doorPrefab;
    private float timeOpened = 0,
        targetAngle = 0;
    private bool isOpened,
        isInteracting;
    private HingeJoint joint;
    private Vector3 defaultPosition;

    private void Start()
    {
        joint = GetComponent<HingeJoint>();
        defaultPosition = transform.localPosition;

        if (isDestroyed)
            OnStateChanged(true, false);
    }

    private void Update()
    {
        if (isInteracting)
        {
            float currentAngle = transform.localEulerAngles.y;
            float delta = Mathf.DeltaAngle(currentAngle, targetAngle * joint.axis.y);
            if (Mathf.Abs(delta) > 2)
            {
                transform.localRotation = Quaternion.RotateTowards(
                    transform.localRotation,
                    Quaternion.Euler(0, targetAngle * joint.axis.y, 0),
                    100 * Time.deltaTime
                );
            }
            else
            {
                SetJointLimit(targetAngle);
                if (targetAngle == 0)
                    isOpened = false;
                isInteracting = false;
            }
        }

        if (isOpened)
        {
            if (timeOpened < 5)
                timeOpened += Time.deltaTime;
            else if (transform.localEulerAngles.y < 2)
            {
                SetJointLimit(0);
                isOpened = false;
                targetAngle = 0;
            }
        }
    }

    public override bool IsInteractable() => true;

    public override void OnInteractButtonUp(Transform character)
    {
        CmdInteract();
    }

    [Command(requiresAuthority = false)]
    private void CmdInteract()
    {
        RpcInteract();
    }

    [ClientRpc]
    private void RpcInteract()
    {
        if (netIdentity.connectionToClient != connectionToClient)
            return;
        if (targetAngle == 0)
        {
            isOpened = true;
            targetAngle = maxAngle;
            SetJointLimit(maxAngle);
        }
        else
            targetAngle = 0;

        timeOpened = 0;
        isInteracting = true;
    }

    private void SetJointLimit(float newLimit)
    {
        JointLimits limits = joint.limits;
        limits.max = newLimit;
        joint.limits = limits;
    }

    private void OnCollisionStay(Collision collision)
    {
        timeOpened = 5;
        isInteracting = false;

        if (
            !(
                collision.transform.TryGetComponent(out StateManager _)
                || collision.transform.TryGetComponent(out Bullet _)
            )
        )
        {
            float force = collision.impulse.magnitude;
            if (force > 300 || Vector3.Distance(transform.localPosition, defaultPosition) > 0.3f)
                SetDestroyed(true);
            if (force > 100)
            {
                isOpened = true;
                timeOpened = 0;
                SetJointLimit(maxAngle);
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void SetDestroyed(bool isDestroyed)
    {
        this.isDestroyed = isDestroyed;
        GameObject door = Instantiate(doorPrefab, transform.position, transform.rotation);
        door.GetComponent<Rigidbody>().linearVelocity = GetComponent<Rigidbody>().linearVelocity;
        NetworkServer.Spawn(door);
    }

    private void OnStateChanged(bool oldVar, bool newVar)
    {
        gameObject.SetActive(!newVar);
    }
}
