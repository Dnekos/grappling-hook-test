using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookManager : MonoBehaviour
{
	[SerializeField] Verlet thrower;
	[SerializeField] float ThrowForce = 5;

	public Gun GrabbedWeapon;


	[HideInInspector] public Rigidbody rb;
	SphereCollider col;
	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		col = GetComponent<SphereCollider>();
		col.enabled = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (thrower.chainState != Verlet.ChainState.Thrown || collision.gameObject.tag != "GrabbablePoint")
			return;

		// freeze hook
		Stick(collision.transform);

		// set up chain
		thrower.Stick();

		// check for grabbable points for pull
		Grabbable grabbed = collision.gameObject.GetComponent<Grabbable>();
		if (grabbed != null)
		{
			thrower.grabbedObj = grabbed;
			grabbed.Grabbed = true;
		}
	}

	public void Stick(Transform obj)
	{
		rb.isKinematic = true;
		rb.velocity = Vector3.zero;
		col.enabled = false;

		transform.SetParent(obj);
	}
	public void Throw(Transform hand)
	{
		transform.SetParent(null);

		rb.isKinematic = false;
		col.enabled = true;

		rb.AddForce(hand.forward * ThrowForce, ForceMode.Impulse);
	}
}
