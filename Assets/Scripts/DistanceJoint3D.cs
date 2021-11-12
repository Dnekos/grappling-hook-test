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
	public bool OnlyMaxDist;

	[Header("Tautness")]
	public bool Taut = false; // taut is used for pulls
	bool MaxDist = false; // maxdist is used for movement
	[SerializeField, Tooltip("Multiplier for margin of error in public tautness calculation, [0,1]. Lower values make Taut at shorter range.")] 
	float TautMargin = 1;

	[Header("Default Pull Values"),SerializeField] float DefaultLift = 5;
	[SerializeField] float DefaultForce = 14;


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
	private void Update()
	{
		if (ConnectedRigidbody == null )
			return;
		float distance = Vector3.Distance(rb.position, ConnectedRigidbody.position);
		Taut = distance >= Distance * TautMargin;
		MaxDist = distance >= Distance;
	}

	private void FixedUpdate()
	{
		if (ConnectedRigidbody == null || !MaxDist)
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
	
	public void PullPlayer()
	{
		Debug.Log("Default pull");
		Vector3 dirToPlayer = ConnectedRigidbody.position - transform.position;
		rb.AddForce(dirToPlayer * DefaultForce + Vector3.up * DefaultLift, ForceMode.Impulse);
	}
}
