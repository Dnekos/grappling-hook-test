using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// https://youtu.be/ft6s09cq7DM
public class DistanceJoint3D : MonoBehaviour
{
	public Rigidbody ConnectedRigidbody;
	public Rigidbody PreviousRigidbody;

	public bool DetermineDistanceOnStart;
	public float Distance;
	public float spring = 0.1f;
	public float damper = 5f;

	protected Rigidbody rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	// Start is called before the first frame update
	void Start()
    {
		if (ConnectedRigidbody == null)
			return;

		if (DetermineDistanceOnStart)
			Distance = Vector3.Distance(rb.position, ConnectedRigidbody.position);

    }
	private void FixedUpdate()
	{
		if (ConnectedRigidbody == null)
			return;

		Vector3 connection;
		if (PreviousRigidbody == null)
			connection = (rb.position - ConnectedRigidbody.position);
		else
			connection = (rb.position - ConnectedRigidbody.position) + (rb.position - PreviousRigidbody.position) * 0.5f;
		var distanceDiscrepency = Distance - connection.magnitude;

		//var backconnection = rb.position - ConnectedRigidbody.position;
		//var distanceDiscrepency = Distance - connection.magnitude;

		rb.position += distanceDiscrepency * connection.normalized;

		var velocityTarget = connection + (rb.velocity + Physics.gravity * spring);
		var projectOnConnection = Vector3.Project(velocityTarget, connection);
		rb.velocity = (velocityTarget - projectOnConnection) / (1+damper * Time.fixedDeltaTime);
	}
	
}
