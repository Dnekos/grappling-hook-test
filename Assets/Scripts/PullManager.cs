using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PullManager : MonoBehaviour
{
	public Grabbable grabbedObj;
	[SerializeField] float lift = 10;

	DistanceJoint3D joint;

	[SerializeField] HookManager fundo;
	[SerializeField] Verlet thrower;


	[Header("DEBUG"), SerializeField]
	bool OnlyPullWhenTaut = false;
	// Start is called before the first frame update
	void Start()
	{
		joint = GetComponent<DistanceJoint3D>();
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void OnPull(InputAction.CallbackContext context)
	{
		if (!context.performed)
			return;

		if (thrower.HeldWeapon)
		{
			thrower.HeldWeapon.ShootProjectile(true);
		}
		else
		{
			if ((OnlyPullWhenTaut && !joint.Taut) || grabbedObj == null)
				return;
			Debug.Log("pulled");
			grabbedObj.Pull(transform, fundo);
		}
	}

}

