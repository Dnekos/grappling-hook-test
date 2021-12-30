using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Grabbable
{

	[Header("Damage per Second"), SerializeField]
	protected float BulletDamage = 3;
	[SerializeField]
	protected float PelletAmount;
	[SerializeField]
	protected float FireRate = .5f;

	[Header("Trajectory"), SerializeField]
	protected float spread = 5f;
	[SerializeField]
	protected float bulletForce = 5f;

	/*[Header("RocketSpecific"), SerializeField]
	protected bool launcher;
	[SerializeField]
	protected float splash;
	*/

	[Header("Player Specific values"),Tooltip("Where the gun sits when parented by the player")]
	public Vector3 HeldPos;
	[Header("Enemy Specific")]
	public Vector3 EnemyHeldPos;

	[Header("Clip"), SerializeField]
	protected float clipSize;


	[Header("Throwing"), SerializeField, Tooltip("How far the gun is thrown")] float ThrowForce;
	[SerializeField, Tooltip("How much the gun spins when thrown")] Vector3 AngularThrowForce;

	[SerializeField, Header("Projectile Components")]
	public Transform firePoint;
	[SerializeField]
	GameObject bulletPrefab;

	private bool canShoot = true;
	protected float RemainingAmmo;
	[HideInInspector]public BoxCollider col;


	protected override void Start()
	{
		base.Start();
		RemainingAmmo = clipSize;
		col = GetComponent<BoxCollider>();
	}

	#region Grabbing and held functions
	public override void Pull(Transform puller, HookManager hook)
	{
		// disable physics
		hook.transform.SetParent(null);
		transform.SetParent(hook.transform);
		rb.isKinematic = true;
		col.enabled = false;

		hook.GrabbedWeapon = this;
		hook.thrower.ReturnToHand();

	}

	/// <summary>
	/// become a child of and go to predetermined position of 'hand'
	/// </summary>
	public void Stick(Transform hand)
	{
		transform.SetParent(hand);
		transform.localPosition = HeldPos;
		transform.localRotation = Quaternion.Euler(Vector3.zero);
	}
	public void Throw()
	{
		// enable physics
		transform.SetParent(null);
		rb.isKinematic = false;
		col.enabled = true;

		rb.AddForce(transform.forward * ThrowForce, ForceMode.Impulse);
		rb.AddRelativeTorque(AngularThrowForce, ForceMode.Impulse);
	}

	/// <summary>
	/// small override to prevent the player from taking damage when throwing guns, while still letting the guns deal damage to enemies
	/// </summary>
	protected override void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag != "Player")
			base.OnCollisionEnter(collision);
	}
	#endregion

	IEnumerator CanShoot()
	{
		canShoot = false;
		yield return new WaitForSeconds(FireRate);

		canShoot = true;
	}


	/// <summary>
	/// Check if user can fire a projectile, then fire a projectile
	/// </summary>
	/// <returns>Wether or not a projectile was fired. Needed for SC to know when to send GunshotCheck</returns>
	public bool ShootProjectile(bool decrementAmmo = false)
	{
		// has issue where these are ran by non active objects (https://forum.unity.com/threads/playerinput-prefab-calls-action-events-when-using-player-input-manager.1120189/)
		if (!canShoot || !gameObject.activeInHierarchy || RemainingAmmo <= 0)
			return false;

		if (decrementAmmo)
			RemainingAmmo--;


		// create bullet
		for (int index = 0; index < PelletAmount; index++)
		{
			Bullet bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation).GetComponent<Bullet>();
			
			bullet.transform.eulerAngles += 
				new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread)); // apply spread
			bullet.Damage = BulletDamage;
			bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletForce, ForceMode.Impulse); // shoot the bullet
		}
			
		StartCoroutine(CanShoot());
		
		return true;
	}
}
