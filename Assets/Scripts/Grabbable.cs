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
	AutoReticle targeting;

	[Header("Thrown Damage"), SerializeField,Tooltip("Minimum speed to be able to damage an object")] float MinSpeedForDamage = 5;
	[SerializeField, Tooltip("Damage is calculated as velocity / divisor")] float DamageDivisor = 4;

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
		targeting = FindObjectOfType<AutoReticle>();
	}

	public virtual void Pull(Transform puller, HookManager hook)
	{

		Vector3 dirToPlayer = (transform.position - puller.position).normalized;

		if (target == PullTarget.Self)
		{
			if (targeting.enemy != null && targeting.enemy != transform) // if autotargeting is active and not directed at the self
			{
				// set up basic variables
				Vector3 enemyPos = targeting.enemy.position;
				float dist = Vector3.Distance(transform.position, enemyPos);
				float angle, modifiedPullForce = PullForce;

				do // determine angle, scaling up if needed
				{
					angle = 0.5f * Mathf.Acos((Physics.gravity.y * dist) / (modifiedPullForce * modifiedPullForce));
					modifiedPullForce += 0.01f;
				} while (float.IsNaN(angle));


				dirToPlayer = (enemyPos - transform.position).normalized;
				Debug.Log("Pos:" + enemyPos + " dist:" + dist + " angle:" + angle + " dirtoplayer:" + dirToPlayer + " pullforce:" + modifiedPullForce);
				Vector3 Force = Quaternion.AngleAxis(angle, Vector3.up) * dirToPlayer * modifiedPullForce * rb.mass + Vector3.up * lift;

				rb.AddForce(Force, ForceMode.Impulse);
				Debug.Log(Force);
				angle = 0.5f * Mathf.Asin((Physics.gravity.y * dist) / (PullForce * modifiedPullForce));
				Debug.Log(Quaternion.AngleAxis(angle, Vector3.up) * dirToPlayer * modifiedPullForce);
			}
			else // if there is no targeted enemy, pull in direction that the player is looking
			{
				Vector3 pullDir = Vector3.Reflect(puller.forward, Vector3.Cross(dirToPlayer, Vector3.up)).normalized;
				rb.AddForce(-pullDir * PullForce + Vector3.up * lift, ForceMode.Impulse);
			}

			hook.Unstick(); // disconnect hook when throwing it 
			hook.ReturnToThrower();
		}
		else // have the player pull towards the object
		{
			puller.GetComponent<Rigidbody>().AddForce(dirToPlayer * PullForce + Vector3.up * lift, ForceMode.Impulse);
		}
	}

	/// <summary>
	/// if the object is in a decent motion and collides with an object with health, they take some damage.
	/// </summary>
	protected virtual void OnCollisionEnter(Collision collision)
	{
		HealthManager targetHealth = collision.gameObject.GetComponent<HealthManager>();
		ContactPoint mainpoint = collision.GetContact(0);
		float speed = rb.velocity.magnitude;

		if (targetHealth != null)
			Debug.Log(speed + " "+ Mathf.Min(1, speed / DamageDivisor)+ " "+ GroundCheck());

		if (targetHealth != null && speed >= MinSpeedForDamage && !GroundCheck())
		{
			targetHealth.GetHit(Mathf.Min(1, speed / DamageDivisor), HitPoint: mainpoint.point);
		}
	}

	bool GroundCheck()
	{
		float dist = GetComponent<Collider>().bounds.extents.y * 1.05f; // 1.2 times the height (starting at the midpoint)
		if (Physics.Raycast(transform.position, Vector3.down, dist, LayerMask.GetMask("Ground")))
			return true;
		return false;
	}
}
