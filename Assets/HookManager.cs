using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookManager : MonoBehaviour
{
	[SerializeField] Verlet thrower;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.isTrigger || thrower.chainState != Verlet.ChainState.Thrown)
			return;
		thrower.Stick();
	}
}
