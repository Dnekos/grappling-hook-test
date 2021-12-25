using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
	//public Transform grabbed;
	[Header("Grabbable Properties"), SerializeField] protected float PullForce = 0f;
	[SerializeField] protected float lift = 3;
	[Tooltip("percent of Joint force applies to the player, rather than the grabbed object")] public float DragCoefficient = 1;
	
	public bool Grabbed = false;

	public enum PullTarget
	{
		Self,
		Player,
		Equip
	}
	public PullTarget target = PullTarget.Self;

	[HideInInspector] public Rigidbody rb;

	protected virtual void Start()
	{
		rb = GetComponent<Rigidbody>();
		if (rb == null)
			target = PullTarget.Player;
	}

	public virtual void Pull(Transform puller, HookManager hook)
	{
		Vector3 dirToPlayer = (transform.position - puller.position).normalized;

		if (target == PullTarget.Self)
		{
			Vector3 pullDir = Vector3.Reflect(puller.forward, Vector3.Cross(dirToPlayer, Vector3.up)).normalized;
			rb.AddForce(-pullDir * PullForce + Vector3.up * lift, ForceMode.Impulse);
		}
		else
		{
			puller.GetComponent<Rigidbody>().AddForce(dirToPlayer * PullForce + Vector3.up * lift, ForceMode.Impulse);
		}
	}
}
