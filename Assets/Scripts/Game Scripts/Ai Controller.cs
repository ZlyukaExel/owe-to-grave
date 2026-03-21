using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InputManager))]
public class AiController : NetworkBehaviour
{
    public Vector2 movementVector = new();
    private InputManager input;
    private InputAction jumpAction;

    private void Start()
    {
        if (!isServer)
        {
            enabled = false;
            return;
        }

        input = GetComponent<InputManager>();
        jumpAction = InputSystem.actions.FindAction("Jump");

        // Example: setting jumping true
        // SetJumping(true);
    }

    private void Update()
    {
        input.SetMovementVector(movementVector);
    }

    public void SetJumping(bool isJumping)
    {
        input.SetActionPressed(jumpAction, isJumping);
    }
}
