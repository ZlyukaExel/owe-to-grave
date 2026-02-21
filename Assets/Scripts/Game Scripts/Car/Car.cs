using Mirror;
using TMPro;
using UnityEngine;

public class Car : NetworkBehaviour
{
    public bool isMoving => Mathf.Abs(speed) > 0.5f;

    [SyncVar]
    public float speed = 0;

    [SerializeField]
    private float maxSpeed = 20,
        torque = 2000,
        defaultBrakeTorque = 3,
        maxSteerAngle = 35,
        maxSteerAngleAtMaxSpeed = 10;
    private float speedFactor;
    private int wheelsGrounded;
    private float timeStuck;
    private Wheel[] wheels;
    private Rigidbody rb;
    private Transform driver;

    [Header("Miscellaneous")]
    public Seat[] seats;
    private Transform steeringWheel;
    private TMP_Text speedometer;
    private Links l;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<Wheel>();
        steeringWheel = transform.Find("SteeringWheel");
        CarDoor[] doors = transform.GetComponentsInChildren<CarDoor>();
        DestroyOnCollisionEnter[] destroyables =
            transform.GetComponentsInChildren<DestroyOnCollisionEnter>();
        foreach (CarDoor door in doors)
        {
            // Doors can't collide with each other
            foreach (CarDoor door2 in doors)
            {
                Physics.IgnoreCollision(
                    door.GetComponentInChildren<Collider>(),
                    door2.GetComponentInChildren<Collider>()
                );
            }

            // Doors can't destroy destroyables
            foreach (DestroyOnCollisionEnter destroyable in destroyables)
            {
                Physics.IgnoreCollision(
                    destroyable.GetComponent<Collider>(),
                    door.GetComponentInChildren<Collider>()
                );
            }
        }
    }

    private void LateUpdate()
    {
        // SyncVar on server
        if (isOwned)
            CmdSetSpeed(Vector3.Dot(transform.forward, rb.linearVelocity));

        if (driver == null)
            return;

        if (Time.frameCount % 3 == 0)
            speedometer.text = Mathf.Abs(Mathf.Round(speed * 4)).ToString();
    }

    private void FixedUpdate()
    {
        if (rb.isKinematic)
            return;
        if (!driver)
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.motorTorque = 0;
                wheel.wheelCollider.brakeTorque = defaultBrakeTorque;
            }

            return;
        }

        wheelsGrounded = 0;
        foreach (var wheel in wheels)
        {
            if (wheel.isGrounded)
                wheelsGrounded++;
        }

        // speed/maxSpeed
        speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(speed));

        // Slow down if close to max speed
        float currentTorque = Mathf.Lerp(torque, 0, speedFactor);

        // Less steering if close to max speed
        float currentSteerRange = Mathf.Lerp(maxSteerAngle, maxSteerAngleAtMaxSpeed, speedFactor);

        bool isAccelerating =
            (InputManager.Instance.Vertical > 0 && speed >= -0.5f)
            || (InputManager.Instance.Vertical < 0 && speed <= 0.5);

        foreach (var wheel in wheels)
        {
            WheelCollider col = wheel.wheelCollider;

            if (wheel.steerable)
                col.steerAngle = InputManager.Instance.SmoothedHorizontal * currentSteerRange;

            if (isAccelerating)
            {
                if (wheel.motorized)
                    col.motorTorque = InputManager.Instance.SmoothedVertical * currentTorque;
                col.brakeTorque = 0;
            }
            else
            {
                col.motorTorque = 0;
                col.brakeTorque = torque;
            }

            if (wheel.isBackWheel && InputManager.Instance.GetKey(KeyCode.Space))
                col.brakeTorque = torque;
        }

        steeringWheel.localEulerAngles =
            -currentSteerRange * 2 * InputManager.Instance.SmoothedHorizontal * Vector3.forward;

        if (wheelsGrounded == 0)
        {
            float rotateForce = 30;

            rb.angularVelocity +=
                rotateForce * InputManager.Instance.SmoothedVertical * transform.right / rb.mass;

            if (rb.linearVelocity.magnitude < 5 && rb.angularVelocity.magnitude < 1)
            {
                timeStuck += 10 * Time.fixedDeltaTime;
                if (timeStuck > 5 && !InputManager.Instance.GetKey(KeyCode.Space))
                    rotateForce = 500;
            }
            else
                timeStuck = 0;

            if (InputManager.Instance.GetKey(KeyCode.Space))
                rb.angularVelocity +=
                    InputManager.Instance.SmoothedHorizontal * rotateForce * transform.up / rb.mass;
            else
                rb.angularVelocity -=
                    InputManager.Instance.SmoothedHorizontal
                    * rotateForce
                    * transform.forward
                    / rb.mass;
        }
        else
            timeStuck = 0;
    }

    [Command]
    private void CmdSetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void EnterCar(Transform character)
    {
        if (!HasSeat())
            return;

        // If driver seat is occupied, searching for avaliable passenger seats
        if (!seats[0].IsInteractable())
        {
            if (seats.Length < 2)
                return;
            for (int i = 1; i < seats.Length; i++)
            {
                if (seats[i].IsInteractable())
                {
                    seats[i].Sit(character);
                    return;
                }
            }
        }
        else // New driver
        {
            driver = character;

            l = character.GetComponent<Links>();

            l.networkCommands.CmdRequestOwnership(netIdentity);

            // Change UI (can't use UI cuz keyboard ignores it then, remove on PC)
            Transform carUi = l.ui.Find("Car Ui");
            carUi.gameObject.SetActive(true);
            speedometer = carUi.Find("Speedometer").GetComponentInChildren<TMP_Text>();

            // Sit down
            seats[0].Sit(character);
        }
    }

    public void ExitCar()
    {
        if (!driver)
            return;

        // Change UI (can't use UI cuz keyboard ignores it then, remove on PC)
        l.ui.Find("Car Ui").gameObject.SetActive(false);

        //InputManager.Instance.ResetInput();

        foreach (var wheel in wheels)
        {
            if (wheel.wheelCollider is WheelCollider col)
            {
                col.motorTorque = col.brakeTorque = 0;
            }
        }

        driver = null;
        l = null;
    }

    public bool HasSeat()
    {
        foreach (var seat in seats)
        {
            if (seat.IsInteractable())
                return true;
        }
        return false;
    }
}
