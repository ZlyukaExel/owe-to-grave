using UnityEngine;

public class Movement
{
    public bool isRunning,
        runButtonPressed;
    public bool isMoving { get; private set; }
    public float speedModifier = 1;
    public float currentSpeed { get; private set; }
    private float inputMagnitude;
    private float runButtonPressedTime;
    private float holdButtonTime = 0.15f;

    [Header("Slide")]
    public bool isSliding;
    public float minSlideSpeed;
    private float slidingTime;
    private float fixedSpeed;
    private readonly Links l;
    private readonly Jump jump;

    public Movement(Links links)
    {
        l = links;
        jump = new Jump(l);
        InputManager.Instance.GetAction(KeyCode.LeftShift).onDown.AddListener(OnRunButtonDown);
        InputManager.Instance.GetAction(KeyCode.LeftShift).onUp.AddListener(OnRunButtonUp);
    }

    private float noInputTimer;

    public void MovementUpdate()
    {
        l.humanoid.isGrounded = Physics.Raycast(
            l.transform.position + l.humanoid.groundRayOffset,
            -l.transform.up,
            l.humanoid.groundRayLength,
            l.humanoid.groundTriggerLayers.value
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

        l.animator.SetBool("isJumping", !l.humanoid.isGrounded);
    }

    public void MovementFixedUpdate()
    {
        l.rb.linearVelocity = new(0, l.rb.linearVelocity.y, 0);

        currentSpeed = Mathf.Round(l.rb.linearVelocity.magnitude);

        jump.JumpFixedUpdate();

        // Slide or move
        if (l.humanoid.canMove)
        {
            if (isSliding)
            {
                Slide();
            }
            else
            {
                ActualMovement();
            }
        }
    }

    private void ActualMovement()
    {
        Combat combat = l.humanoid.state as Combat;
        bool isAiming = combat != null && combat.isAimingOrShooting;

        Vector3 direction = new(
            InputManager.Instance.Horizontal,
            0,
            InputManager.Instance.Vertical
        );
        Vector3 joystickWorldMovement = l.playerCamera.TransformDirection(direction);
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
                l.rb.AddForce(
                    inputMagnitude
                        * l.humanoid.defaultSpeed
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
            Vector3 targetRotation = new(0, l.cameraController.angleY, 0);
            l.transform.rotation = Quaternion.Lerp(
                l.transform.rotation,
                Quaternion.Euler(targetRotation),
                combat.aimingWeight
            );

            if (l.animator != null)
            {
                l.animator.SetFloat("VelZ", InputManager.Instance.SmoothedVertical);
                l.animator.SetFloat("VelX", InputManager.Instance.SmoothedHorizontal);
            }
        }
        // Default state
        else
        {
            // Rotate character in movement direction
            if (inputMagnitude > 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(joystickWorldMovement);
                l.transform.rotation = Quaternion.Slerp(
                    l.transform.rotation,
                    targetRotation,
                    7 * Time.fixedDeltaTime
                );
            }

            if (l.animator != null)
            {
                l.animator.SetFloat("VelZ", inputMagnitude); // Not smooth change cuz you'll have to handle walking state
                l.animator.SetFloat("VelX", 0);
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

    public void StartSlide()
    {
        if (currentSpeed > 5 && !isSliding && l.humanoid.isGrounded)
        {
            isSliding = true;
            fixedSpeed = currentSpeed *= 1.2f;
            slidingTime = 0;
            l.cameraController.maxPitch = 10;
            l.animator.SetBool("isSliding", true);
        }
    }

    private void Slide()
    {
        if (currentSpeed > minSlideSpeed)
        {
            slidingTime += 5 * Time.fixedDeltaTime;

            // Fixed time slide
            /*float targetSlidingSpeed = Mathf.Lerp(10, minSlideSpeed, slidingTime / slideTime);
            if (currentSpeed < targetSlidingSpeed)
                rb.AddForce(transform.forward * runSpeed * targetSlidingSpeed / 10, ForceMode.VelocityChange);*/

            // Slow down player
            float targetSlidingSpeed = fixedSpeed - slidingTime;
            l.rb.AddForce(
                l.transform.forward * l.humanoid.defaultSpeed * targetSlidingSpeed / 10,
                ForceMode.VelocityChange
            );
        }
        else
        {
            isSliding = false;
            l.cameraController.maxPitch = 45;
            //if (!crouchButtonPressed)
            //    OnCrouchButtonDown();
            l.animator.SetBool("isSliding", false);
        }
    }
}
