using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : Grabbable
{
	[Header("Fire Speeds and clip"), SerializeField]
	protected float clipSize;
	[SerializeField]
	protected float FireRate = .5f;

	[Header("Damage"), SerializeField]
	protected float BulletDamage = 3;
	[SerializeField]
	protected float PelletAmount;

	[Header("Trajectory"), SerializeField]
	protected float spread = 5f;
	[SerializeField]
	protected float bulletForce = 5f;
	/*[Header("RocketSpecific"), SerializeField]
	protected bool launcher;
	[SerializeField]
	protected float splash;
	*/
	[SerializeField, Header("Projectile Components")]
	public Transform firePoint;
	[SerializeField]
	GameObject bulletPrefab;

	private bool canShoot = true;
	protected float currentClip;

	[Header("Held values")]
	public Vector3 HeldPos;

	public override void Pull(Transform puller, HookManager hook)
	{
		Debug.Log("Override");
		transform.SetParent(hook.transform);
		hook.GrabbedWeapon = this;
		FindObjectOfType<Verlet>().ReturnToHand();
		GetComponent<Rigidbody>().isKinematic = true;
	}


	IEnumerator CanShoot()
	{
		canShoot = false;
		yield return new WaitForSeconds(FireRate);

		canShoot = true;
	}

	virtual protected void Start()
	{
		currentClip = clipSize;
	}

	/// <summary>
	/// Check if user can fire a projectile, then fire a projectile
	/// </summary>
	/// <returns>Wether or not a projectile was fired. Needed for SC to know when to send GunshotCheck</returns>
	public bool ShootProjectile(bool decrementAmmo = false)
	{
		// has issue where these are ran by non active objects (https://forum.unity.com/threads/playerinput-prefab-calls-action-events-when-using-player-input-manager.1120189/)
		if (!canShoot || !gameObject.activeInHierarchy || currentClip <= 0)
			return false;

		if (decrementAmmo)
			currentClip--;


		// create bullet
		for (int index = 0; index < PelletAmount; index++)
		{
			//Quaternion newRot = (firePoint.lossyScale.y < 0) ? firePoint.rotation * Quaternion.Euler(0, 0, 180) : firePoint.rotation;
			Bullet bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation).GetComponent<Bullet>();
			bullet.transform.eulerAngles = new Vector3(bullet.transform.eulerAngles.x + Random.Range(-spread, spread), bullet.transform.eulerAngles.y + Random.Range(-spread, spread), bullet.transform.eulerAngles.z + Random.Range(-spread, spread));
			bullet.Damage = BulletDamage;
			//bullet.gameObject.SetActive(true);
			bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletForce,ForceMode.Impulse);

		}
			
		StartCoroutine(CanShoot());
		
		return true;
	}
}
