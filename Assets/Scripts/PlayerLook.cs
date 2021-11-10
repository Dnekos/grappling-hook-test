using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
	[SerializeField] float Sensitivity = 100;
	[SerializeField] Transform head;

	float xRotation = 0f;
	Vector2 LookDir;
	// Start is called before the first frame update
    void Start()
    {
		Cursor.lockState = CursorLockMode.Locked;
    }

	public void OnLook(InputAction.CallbackContext context)
	{
		LookDir = context.ReadValue<Vector2>();
	}

    // Update is called once per frame
    void Update()
    {
		LookDir *= Sensitivity * Time.deltaTime;

		xRotation -= LookDir.y;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);
		head.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

		transform.Rotate(Vector3.up * LookDir.x);
	}
}
