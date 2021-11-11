using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
	public enum EnemyState
	{
		Patrol,
		Chase,
		Unarmed
	}
	public EnemyState state = EnemyState.Patrol;
	[SerializeField] Transform EnemyBody;


	[Header("Patrol"), SerializeField] float SweepAngle = 30;
	[SerializeField] float SweepSpeed = 1;
	[SerializeField] float DetectRange = 50;
	float initialAngle;
	Transform Player;


	[Header("Guns"), SerializeField, Tooltip("How long the enemy doesnt react before going to find a gun")]
	float UnarmedStunPeriod = 2;
	float stunTimer;
	Gun HeldWeapon;
	Transform LookTarget;

	

	// Start is called before the first frame update
	void Start()
    {
		HeldWeapon = GetComponentInChildren<Gun>();
		initialAngle = transform.eulerAngles.y;

	}

    // Update is called once per frame
    void Update()
    {
		if (EnemyBody == null) // if the enemy has been killed, delete AI and leave gun
		{
			DetachGun();
			Destroy(gameObject);
			return;
		}

		transform.position = EnemyBody.position;
		transform.rotation = EnemyBody.rotation;


		if (stunTimer > 0)
		{
			stunTimer -= Time.deltaTime;
			EnemyBody.LookAt(LookTarget);
			EnemyBody.eulerAngles = new Vector3(0, EnemyBody.rotation.eulerAngles.y, 0);
			return;
		}


        switch (state)
		{
			case EnemyState.Chase:
				Chase();
				break;
			case EnemyState.Patrol:
				Patrol();
				break;
			case EnemyState.Unarmed:
				//Chase();
				break;
		}
		if (HeldWeapon != null)
		{
			//Debug.Log(transform.position);

			if (HeldWeapon.Grabbed)
			{
				state = EnemyState.Unarmed;

				// set up little stun
				stunTimer = UnarmedStunPeriod;
				LookTarget = HeldWeapon.transform;

				// turn on gun physics then dropp
				DetachGun();
			}
		}

    }
	void DetachGun()
	{
		HeldWeapon.rb.isKinematic = false;
		HeldWeapon.transform.SetParent(null);
		HeldWeapon = null;
	}

	void Chase()
	{
		EnemyBody.LookAt(Player.position);
		HeldWeapon.ShootProjectile();
	}

	void Patrol()
	{
		EnemyBody.eulerAngles = new Vector3(0, Mathf.Sin(Time.realtimeSinceStartup * SweepSpeed) * SweepAngle, 0);
		RaycastHit info;
		if (Physics.Raycast(new Ray(EnemyBody.position, EnemyBody.forward), out info, DetectRange, LayerMask.GetMask("Player")))
		{
			state = EnemyState.Chase;
			Player = info.transform;
		}
	}
}
