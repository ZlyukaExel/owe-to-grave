using UnityEngine;

public class Jump
{
    public float jumpForce = 4;
    private readonly Links l;

    public Jump(Links links)
    {
        l = links;
    }

    public void JumpFixedUpdate()
    {
        if (
            InputManager.Instance.GetKey(KeyCode.Space)
            && l.humanoid.canMove
            && l.humanoid.isGrounded
        )
        {
            l.rb.linearVelocity = new Vector3(l.rb.linearVelocity.x, 0f, l.rb.linearVelocity.z);
            l.rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }
}
