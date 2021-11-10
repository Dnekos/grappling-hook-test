using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

	[SerializeField] float acceleration = 1;

	[SerializeField] float maxSpeed;
	[SerializeField] float minSpeed;

	[Header("Ground Checking")]
	public bool isGrounded;
	[SerializeField] Transform groundCheck;
	[SerializeField] float groundDistance = 0.4f;
	[SerializeField] LayerMask groundMask;

	[Header("Jumping"), SerializeField]
	float jumpForce; //lmao goku game

	[Header("Hooked"), SerializeField, Tooltip("Speed multiplier for when suspended by the hook")]
	float SuspendedMultiplier = 1.3f;
	Verlet hook;

	Vector2 inputDir;
	Rigidbody rb;

	// Start is called before the first frame update
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		hook = GetComponentInChildren<Verlet>();
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		inputDir = context.ReadValue<Vector2>();
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		Debug.Log(context.performed);
		Debug.Log("groun " + isGrounded);
		if (context.performed && isGrounded)
			rb.AddForce(Vector3.up * jumpForce,ForceMode.Impulse);
	}

	private void Update()
	{
		isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask);
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		Vector3 move = transform.right * inputDir.x + transform.forward * inputDir.y;

		if (inputDir != Vector2.zero && rb.velocity.magnitude < maxSpeed)
		{
			rb.AddForce(move.normalized * acceleration * rb.mass * 
				((hook.chainState == Verlet.ChainState.Stuck && !isGrounded) ? SuspendedMultiplier : 1));
		}

		//else if (inputDir != Vector2.zero && rb.velocity.magnitude > maxSpeed)
		//	rb.velocity = inputDir * maxSpeed;

		if (rb.velocity.magnitude < minSpeed)
		{
			rb.velocity = Vector2.zero;
		}
	}
}
