using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookManager : MonoBehaviour
{
	[SerializeField] Verlet thrower;
	[SerializeField] PullManager player;

	public Gun GrabbedWeapon;

	private void OnCollisionEnter(Collision collision)
	{
		if (thrower.chainState != Verlet.ChainState.Thrown || collision.gameObject.tag != "GrabbablePoint")
			return;
		thrower.Stick();
		transform.SetParent(collision.transform);
		Grabbable grabbed = collision.gameObject.GetComponent<Grabbable>();
		if (grabbed != null)
		{
			player.grabbedObj = grabbed;
			grabbed.Grabbed = true;
		}
	}
}
