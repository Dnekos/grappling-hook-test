using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
	public bool isPlayer = false;
	public float Health = 1;
	[SerializeField] GameObject BloodParticle;
	[SerializeField,Tooltip("Force that the ragdoll falls at when spawned")] float DeathPush = 3;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Melee")
			Debug.Log("hit");
	}
	
	public void GetHit(float damage, bool shouldDestroy = true, Vector3 HitPoint = default(Vector3))
	{
		if (Health <= 0)
			return; // already dead, dont kill twice

		Health -= damage;

		if (HitPoint != default(Vector3)) // if the damage has a point of contact, make a bloodsplatter
			Instantiate(BloodParticle, HitPoint, transform.rotation);

		if (Health <= 0)
		{
			if (!isPlayer)
			{
				HookManager hook = GetComponentInChildren<HookManager>();
				if (hook != null)
					hook.Unstick(); // make sure we take off the hook
				if (shouldDestroy) // slicing weapons have their own ragdoll stuff
				{
					// create a 'ragdoll' version of the object
					Rigidbody ragdollrb = CreateRagdoll().GetComponent<Rigidbody>();
					ragdollrb.AddForceAtPosition((transform.position - HitPoint).normalized * DeathPush, HitPoint); // apply some force to at backwards so it falls

					// destroy the original
					Destroy(gameObject);
				}
			}
		}
		Debug.Log(gameObject + "got hit for " + damage + " damage");
	}

	/// <summary>
	/// returns a gameobject with 
	/// </summary>
	GameObject CreateRagdoll()
	{
		GameObject ragdoll = new GameObject();

		Sliceable originalSliceable = gameObject.GetComponent<Sliceable>();

		// add components to ragdoll
		MeshFilter filter = ragdoll.AddComponent<MeshFilter>();
		MeshRenderer render = ragdoll.AddComponent<MeshRenderer>();
		Sliceable sliceable = ragdoll.AddComponent<Sliceable>();
		Rigidbody rb = ragdoll.AddComponent<Rigidbody>();
		MeshCollider meshCollider = ragdoll.AddComponent<MeshCollider>();

		// set relevant variables to components
		filter.mesh = gameObject.GetComponent<MeshFilter>().mesh;
		
		render.materials = gameObject.GetComponentInChildren<MeshRenderer>().materials;

		sliceable.IsSolid = originalSliceable.IsSolid;
		sliceable.ReverseWireTriangles = originalSliceable.ReverseWireTriangles;
		sliceable.UseGravity = originalSliceable.UseGravity;

		ragdoll.transform.localScale = gameObject.transform.localScale;
		ragdoll.transform.rotation = gameObject.transform.rotation;
		ragdoll.transform.position = gameObject.transform.position;

		meshCollider.sharedMesh = filter.mesh;
		meshCollider.convex = true;

		rb.useGravity = sliceable.UseGravity;

		//meshGameObject.tag = originalObject.tag;
		ragdoll.layer = LayerMask.NameToLayer("Debris");

		ragdoll.name = "ragdoll";

		return ragdoll;

	}
}
