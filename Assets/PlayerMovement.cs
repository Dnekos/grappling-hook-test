using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

	[SerializeField] float acceleration = 1;

	[SerializeField] float maxSpeed;
	[SerializeField] float minSpeed;

	Vector2 inputDir;

	Rigidbody rb;

	// Start is called before the first frame update
	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		inputDir = context.ReadValue<Vector2>();
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		Vector3 move = transform.right * inputDir.x + transform.forward * inputDir.y;

		if (inputDir != Vector2.zero && rb.velocity.magnitude < maxSpeed)
		{
			rb.AddForce(move.normalized * acceleration * rb.mass);
		}

		//else if (inputDir != Vector2.zero && rb.velocity.magnitude > maxSpeed)
		//	rb.velocity = inputDir * maxSpeed;

		if (rb.velocity.magnitude < minSpeed)
		{
			rb.velocity = Vector2.zero;
		}
	}
}
