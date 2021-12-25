using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class MeleeAttack : MonoBehaviour
{

	public bool TargetEnemy = true;
	public float Lifetime = 1;
	public float Damage = 1;

	[SerializeField] float _forceAppliedToCut = 10;
	bool cut = false;


	IEnumerator LifeTimer()
	{
		Debug.Log(Lifetime);
		yield return new WaitForSeconds(Lifetime);
		Destroy(gameObject);
	}
	
	// Start is called before the first frame update
	void Start()
    {
		StartCoroutine(LifeTimer());
	}

	private void OnTriggerEnter(Collider other)
	{
		HealthManager health = other.GetComponent<HealthManager>();
		if (other.isTrigger)
			return;

		if (health)
		{
			if ((TargetEnemy && !health.isPlayer) || (!TargetEnemy && health.isPlayer))
				health.GetHit(Damage, false);
			Debug.Log("did hit");
			if (health.Health <= 0)
				SliceObj(other);
		}
		else if (other.GetComponent<Sliceable>() != null)
			SliceObj(other);

	}

	void SliceObj(Collider other)
	{
			if (cut == true)
				return;
			cut = true;
			/*
			_triggerExitTipPosition = _tip.transform.position;

			//Create a triangle between the tip and base so that we can get the normal
			Vector3 side1 = _triggerExitTipPosition - _triggerEnterTipPosition;
			Vector3 side2 = _triggerExitTipPosition - _triggerEnterBasePosition;

			//Get the point perpendicular to the triangle above which is the normal
			//https://docs.unity3d.com/Manual/ComputingNormalPerpendicularVector.html
			Vector3 normal = Vector3.Cross(side1, side2).normalized;
			*/

			//Transform the normal so that it is aligned with the object we are slicing's transform.
			Vector3 transformedNormal = ((Vector3)(other.gameObject.transform.localToWorldMatrix.transpose * transform.up)).normalized;

			//Get the enter position relative to the object we're cutting's local transform
			Vector3 transformedStartingPoint = other.gameObject.transform.InverseTransformPoint(transform.position);

			Plane plane = new Plane();

			plane.SetNormalAndPosition(
					transformedNormal,
					transformedStartingPoint);
			//Debug.DrawRay(transform.position, transform.up,Color.red,2);
			//DrawPlane(transformedStartingPoint, transformedNormal);
			var direction = Vector3.Dot(Vector3.up, transformedNormal);

			//Flip the plane so that we always know which side the positive mesh is on
			if (direction < 0)
			{
				plane = plane.flipped;
			}

			GameObject[] slices = Slicer.Slice(plane, other.gameObject);
			Destroy(other.gameObject);

			Rigidbody rigidbody = slices[1].GetComponent<Rigidbody>();
			Vector3 newNormal = transform.up + transformedNormal * _forceAppliedToCut;
			rigidbody.AddForce(newNormal, ForceMode.Impulse);
	}
}
