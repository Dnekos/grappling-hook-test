using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
	//public Transform grabbed;
	[Header("Grabbable Properties"), SerializeField] protected float PullForce = 0f;
	[SerializeField] protected float lift = 3;

	public bool Grabbed = false;

	public enum PullTarget
	{
		Self,
		Player,
		Equip
	}
	public PullTarget PullPlayer = PullTarget.Self;

	[HideInInspector] public Rigidbody rb;

	protected virtual void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	public virtual void Pull(Transform puller, HookManager hook)
	{
		Vector3 dirToPlayer = (transform.position - puller.position).normalized;
		Vector3 pullDir = Vector3.Reflect(puller.forward, Vector3.Cross(dirToPlayer, Vector3.up)).normalized;
		rb.AddForce(-pullDir * PullForce + Vector3.up * lift, ForceMode.Impulse);
	}
}
