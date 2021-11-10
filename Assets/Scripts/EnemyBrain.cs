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
	Gun HeldWeapon;

    // Start is called before the first frame update
    void Start()
    {
		HeldWeapon = GetComponentInChildren<Gun>();
		initialAngle = transform.eulerAngles.y;

	}

    // Update is called once per frame
    void Update()
    {
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
		if (HeldWeapon.Grabbed)
			state = EnemyState.Unarmed;
    }


	Transform Player;
	void Chase()
	{
		transform.LookAt(Player.position);
		HeldWeapon.ShootProjectile();
	}

	[SerializeField] float SweepAngle = 30;
	[SerializeField] float SweepSpeed = 1;
	[SerializeField] float DetectRange = 50;
	float initialAngle;
	void Patrol()
	{
		transform.eulerAngles = new Vector3(0, Mathf.Sin(Time.realtimeSinceStartup * SweepSpeed) * SweepAngle, 0);
		RaycastHit info;
		if (Physics.Raycast(new Ray(transform.position, transform.forward), out info, DetectRange, LayerMask.GetMask("Player")))
		{
			state = EnemyState.Chase;
			Player = info.transform;
		}
	}
}
