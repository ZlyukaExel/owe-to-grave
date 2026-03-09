using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CombatState))]
[RequireComponent(typeof(Links))]
public class Movement : MonoBehaviour
{
    public MovementController controller = MovementController.Player;
    public bool canMove = true;
    public float defaultSpeed = 3,
        jumpForce = 4;
    public bool isRunning,
        runButtonPressed;
    public bool isMoving { get; private set; }
    public float speedModifier = 1;
    public float currentSpeed { get; private set; }
    private float inputMagnitude;
    private float runButtonPressedTime;
    private float holdButtonTime = 0.15f;
    private float noInputTimer;

    [Header("Ground ray")]
    public bool isGrounded;
    public Vector3 groundRayOffset = new(0, 0.25f, 0);
    public float groundRayLength = 0.25f;
    public LayerMask groundTriggerLayers;

    private Animator animator;
    private Rigidbody rb;
    private CombatState combat;
    private CameraController cameraController;

    private void Start()
    {
        InputManager.Instance.GetAction(KeyCode.LeftShift).onDown.AddListener(OnRunButtonDown);
        InputManager.Instance.GetAction(KeyCode.LeftShift).onUp.AddListener(OnRunButtonUp);

        Links l = GetComponent<Links>();
        rb = GetComponent<Rigidbody>();
        combat = GetComponent<CombatState>();
        animator = l.animator;
        cameraController = l.cameraController;
    }

    public void MovementUpdate()
    {
        isGrounded = Physics.Raycast(
            transform.position + groundRayOffset,
            -transform.up,
            groundRayLength,
            groundTriggerLayers.value
        );

        if (isRunning)
        {
            runButtonPressedTime += Time.deltaTime;

            if (!runButtonPressed && !InputManager.Instance.isMoving)
            {
                noInputTimer += Time.deltaTime;
                if (noInputTimer > 0.3f)
                    isRunning = false;
            }
            else
                noInputTimer = 0;
        }

        animator.SetBool("isJumping", !isGrounded);
    }

    public void MovementFixedUpdate()
    {
        rb.linearVelocity = new(0, rb.linearVelocity.y, 0);

        currentSpeed = Mathf.Round(rb.linearVelocity.magnitude);

        // Slide or move
        if (canMove)
        {
            // if (isSliding)
            // {
            //     Slide();
            // }
            // else
            // {
            JumpFixedUpdate();
            ActualMovement();
            // }
        }
    }

    private void JumpFixedUpdate()
    {
        if (InputManager.Instance.GetKey(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    private void ActualMovement()
    {
        bool isAiming = combat != null && combat.isAimingOrShooting;

        Vector3 direction = new(
            InputManager.Instance.Horizontal,
            0,
            InputManager.Instance.Vertical
        );
        Vector3 joystickWorldMovement = cameraController.transform.TransformDirection(direction);
        joystickWorldMovement.y = 0;

        inputMagnitude = direction.magnitude;
        isMoving = inputMagnitude > 0.1f;
        if (isMoving)
        {
            // Input can be only 0 (idle), 0.5 (slow walk), 1 (normal walk), 2 (running)
            if (inputMagnitude > 0.66f)
                inputMagnitude = 1;
            else
                inputMagnitude = 0.5f;

            if (isRunning && !isAiming)
                inputMagnitude = 2;

            // Move character
            if (currentSpeed < 10)
                rb.AddForce(
                    inputMagnitude
                        * defaultSpeed
                        * speedModifier
                        * joystickWorldMovement.normalized,
                    ForceMode.VelocityChange
                );
        }
        else
            inputMagnitude = 0;

        // Aiming state
        if (isAiming)
        {
            // Rotate character towards camera direction
            Vector3 targetRotation = new(0, cameraController.angleY, 0);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(targetRotation),
                combat.aimingWeight
            );

            if (animator != null)
            {
                animator.SetFloat("VelZ", InputManager.Instance.SmoothedVertical);
                animator.SetFloat("VelX", InputManager.Instance.SmoothedHorizontal);
            }
        }
        // Default state
        else
        {
            // Rotate character in movement direction
            if (inputMagnitude > 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(joystickWorldMovement);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    7 * Time.fixedDeltaTime
                );
            }

            if (animator != null)
            {
                animator.SetFloat("VelZ", inputMagnitude); // Not smooth change cuz you'll have to handle walking state
                animator.SetFloat("VelX", 0);
            }
        }
    }

    public void OnRunButtonDown()
    {
        runButtonPressedTime = noInputTimer = 0;
        runButtonPressed = isRunning = true;
    }

    public void OnRunButtonUp()
    {
        if (runButtonPressedTime > holdButtonTime)
            isRunning = false;
        runButtonPressed = false;
    }

    // public void StartSlide()
    // {
    //     if (currentSpeed > 5 && !isSliding && isGrounded)
    //     {
    //         isSliding = true;
    //         fixedSpeed = currentSpeed *= 1.2f;
    //         slidingTime = 0;
    //         l.cameraController.maxPitch = 10;
    //         animator.SetBool("isSliding", true);
    //     }
    // }

    // private void Slide()
    // {
    //     if (currentSpeed > minSlideSpeed)
    //     {
    //         slidingTime += 5 * Time.fixedDeltaTime;

    //         // Fixed time slide
    //         /*float targetSlidingSpeed = Mathf.Lerp(10, minSlideSpeed, slidingTime / slideTime);
    //         if (currentSpeed < targetSlidingSpeed)
    //             rb.AddForce(transform.forward * runSpeed * targetSlidingSpeed / 10, ForceMode.VelocityChange);*/

    //         // Slow down player
    //         float targetSlidingSpeed = fixedSpeed - slidingTime;
    //         rb.AddForce(
    //             defaultSpeed * targetSlidingSpeed * transform.forward / 10,
    //             ForceMode.VelocityChange
    //         );
    //     }
    //     else
    //     {
    //         isSliding = false;
    //         l.cameraController.maxPitch = 45;
    //         //if (!crouchButtonPressed)
    //         //    OnCrouchButtonDown();
    //         animator.SetBool("isSliding", false);
    //     }
    // }
}

public enum MovementController
{
    Player,
    Ai,
}
