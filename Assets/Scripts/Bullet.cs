using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

	public float Damage;
	[SerializeField] float Lifetime = 2;
	float countdown;
	[SerializeField] GameObject DustParticle;

	private void Update()
	{
		countdown += Time.deltaTime;
		if (countdown >= Lifetime)
			Destroy(gameObject);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.isTrigger)
			return;
		Debug.Log("hit " + collision.gameObject);

		HealthManager targetHealth = collision.gameObject.GetComponent<HealthManager>();
		ContactPoint mainpoint = collision.GetContact(0);
		if (targetHealth != null)
		{
			targetHealth.GetHit(Damage, HitPoint: mainpoint.point);
		}
		else
		{
			Instantiate(DustParticle, mainpoint.point + (0.2f * mainpoint.normal), Quaternion.LookRotation(mainpoint.normal));
		}
		Destroy(gameObject);
	}
}
