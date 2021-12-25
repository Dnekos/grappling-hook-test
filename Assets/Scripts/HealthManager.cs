using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
	public bool isPlayer = false;
	public float Health = 1;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Melee")
			Debug.Log("hit");
	}
	
	public void GetHit(float damage, bool shouldDestroy = true)
	{
		Health -= damage;
		if (Health <= 0)
		{
			if (!isPlayer)
			{
				HookManager hook = GetComponentInChildren<HookManager>();
				if (hook != null)
					hook.Unstick(); // make sure we take off the hook
				if (shouldDestroy)
					Destroy(gameObject);
			}
		}
		Debug.Log(gameObject + "got hit for " + damage + " damage");
	}
}
