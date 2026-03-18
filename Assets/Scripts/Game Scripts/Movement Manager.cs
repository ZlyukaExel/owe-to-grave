using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CombatState))]
[RequireComponent(typeof(Links))]
public class MovementManager : MonoBehaviour
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
    private InputManager inputManager;
    private InputManager aiInput;
    private InputAction jumpAction;

    void Awake()
    {
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void Start()
    {
        PlayerLinks l = GetComponent<PlayerLinks>();
        rb = GetComponent<Rigidbody>();
        combat = GetComponent<CombatState>();
        aiInput = GetComponent<InputManager>();
        animator = l.animator;
        cameraController = l.cameraController;

        inputManager = controller switch
        {
            MovementController.Player => PlayerInput.Instance,
            _ => aiInput,
        };

        InputManagerAction sprintAction = inputManager.GetAction(
            InputSystem.actions.FindAction("Sprint")
        );
        sprintAction.onDown.AddListener(OnSprintButtonDown);
        sprintAction.onUp.AddListener(OnSprintButtonUp);
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

            bool isMoving =
                new Vector2(inputManager.Vertical, inputManager.Horizontal).magnitude > 0.1f;
            if (!runButtonPressed && !isMoving)
            {
                noInputTimer += Time.deltaTime;
                if (noInputTimer > 0.3f)
                    isRunning = false;
            }
            else
                noInputTimer = 0;
        }

        animator.SetBool("isJumping", !isGrounded);

        if (inputManager.IsPressed(jumpAction))
            Jump();
    }

    public void MovementFixedUpdate()
    {
        rb.linearVelocity = new(0, rb.linearVelocity.y, 0);

        currentSpeed = Mathf.Round(rb.linearVelocity.magnitude);

        // Slide or move
        if (canMove)
        {
            // if (isSliding)
            //     Slide();
            // else
            ActualMovement();
        }
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    public void ActualMovement()
    {
        Vector3 direction = new(inputManager.Horizontal, 0, inputManager.Vertical); // TODO: null first frame

        Vector3 joystickWorldMovement = cameraController.transform.TransformDirection(direction);
        joystickWorldMovement.y = 0;

        float inputMagnitude = direction.magnitude;
        isMoving = inputMagnitude > 0.1f;
        if (isMoving)
        {
            // Input can be only 0 (idle), 0.5 (slow walk), 1 (normal walk), 2 (running)
            if (inputMagnitude > 0.66f)
                inputMagnitude = 1;
            else
                inputMagnitude = 0.5f;

            if (isRunning && !combat.isAimingOrShooting)
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
        if (combat.isAimingOrShooting)
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
                animator.SetFloat("VelZ", inputManager.SmoothedVertical);
                animator.SetFloat("VelX", inputManager.SmoothedHorizontal);
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

    public void OnSprintButtonDown()
    {
        runButtonPressedTime = noInputTimer = 0;
        runButtonPressed = isRunning = true;
    }

    public void OnSprintButtonUp()
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
