using UnityEngine;

public class CapsuleMoves : MonoBehaviour
{
	public Transform mCamera;
	public float speed = 10;
	private Rigidbody rb;
	private float v;
	private float h;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	void Update()
	{
		v = Input.GetAxis("Vertical");
		h = Input.GetAxis("Horizontal");
	}
	
	void FixedUpdate()
	{
		float currentSpeed = Mathf.Round(rb.linearVelocity.magnitude);

		Vector3 direction = new Vector3(h, 0, v);
		Vector3 joystickWorldMovement = mCamera.TransformDirection(direction);
		joystickWorldMovement.y = 0.0f;

		float inputMagnitude = direction.magnitude;
		
		if (inputMagnitude > 0)
		{
			inputMagnitude = 1;

			//Character move 
			if (currentSpeed < 10)
				rb.AddForce(joystickWorldMovement.normalized * speed * inputMagnitude, ForceMode.VelocityChange);

			Quaternion targetRotation = Quaternion.LookRotation(joystickWorldMovement);

			//rotate character towards direction
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 7 * Time.fixedDeltaTime);
		}
	}
}
