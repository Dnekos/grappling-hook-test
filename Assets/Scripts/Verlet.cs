using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Point
{
	public Vector3 position, prevPosition;
	public bool locked;
	public void SetBoth(Vector3 pos)
	{
		position = pos;
		prevPosition =  pos;
	}
	public Point(bool loc = false)
	{
		position = Vector3.zero;
		prevPosition = Vector3.zero;
		locked = loc;
	}
}

public class Stick
{
	public Point pointA, pointB;
	public float length;
	public Stick(Point a, Point b, float len = 0)
	{
		pointA = a;
		pointB = b;
		length = len;
	}
}



public class Verlet : MonoBehaviour
{
	[SerializeField] int numIterations = 10;
	public List<Point> points;
	[SerializeField] List<Stick> sticks;


    public void Simulate()
	{
		// logic provided by https://www.youtube.com/watch?v=PGk0rnyTa1U
		foreach (Point p in points)
		{
			if (!p.locked)
			{
				Vector3 positionBeforeUpdate = p.position;
				p.position += p.position - p.prevPosition;
				p.position += Physics.gravity * Time.deltaTime * Time.deltaTime;
				p.prevPosition = positionBeforeUpdate;
			}
		}
		for (int i = 0; i < numIterations; i++)
		{
			foreach (Stick stick in sticks)
			{
				Vector3 stickCenter = (stick.pointA.position + stick.pointB.position) * 0.5f;
				Vector3 stickDir = (stick.pointA.position - stick.pointB.position).normalized;
				if (!stick.pointA.locked)
					stick.pointA.position = stickCenter + stickDir * stick.length * 0.5f;
				if (!stick.pointB.locked)
					stick.pointB.position = stickCenter - stickDir * stick.length * 0.5f;

			}
		}
	}


	public enum ChainState
	{
		Away,
		Thrown,
		Stuck,
		Returning
	}
	public ChainState chainState = ChainState.Away;
	public float maxdistance;
	[SerializeField] LineRenderer chain;
	[SerializeField] Transform fundo;
	[SerializeField] float ThrowForce;
	//[SerializeField] Transform cam;
	Rigidbody FundoRB;
	SphereCollider FundoCol;
	[SerializeField] float returnSpeed = 2f;
	[SerializeField] float MinReturnDist = 0.05f;
	[SerializeField] DistanceJoint3D playerjoint;
	[SerializeField] float jointDistMultiplier;

	public Gun HeldWeapon;

	private void Start()
	{
		points = new List<Point>();
		sticks = new List<Stick>();

		points.Add(new Point(true));
		for (int i = 1; i < numIterations - 1; i++)
		{
			points.Add(new Point());
		}
		points.Add(new Point(true));
		for (int i = 0; i < numIterations - 1; i++)
		{
			sticks.Add(new Stick(points[i], points[i + 1]));
		}

		FundoRB = fundo.GetComponent<Rigidbody>();
		FundoCol = fundo.GetComponent<SphereCollider>();
		FundoCol.enabled = false;
	}

	private void FixedUpdate()
	{
		if (chainState == ChainState.Away)
		{
		//	fundo.position = transform.position;
		}
	}
	public void Update()
	{
		if (chainState != ChainState.Away)
		{
			if (chainState == ChainState.Stuck)
			{
				// if fundo is latched, simulate verlet
				points[0].SetBoth(transform.position);
				// pin last point onto fundo
				points[points.Count - 1].SetBoth(fundo.position);

				Simulate();
			}
			else // if in midair, keep it a straight line
			{
				for (int i = 0; i < points.Count; i++)
					points[i].SetBoth(Vector3.Lerp(transform.position, fundo.position, i / (float)points.Count));
			}

			// update line render
			chain.positionCount = points.Count;
			for (int i = 0; i < points.Count; i++)
				chain.SetPosition(i, points[i].position); 

			if (chainState == ChainState.Returning) // have it lerp back to hand
			{
				fundo.position = Vector3.Lerp(fundo.position, transform.position, Time.deltaTime * returnSpeed);
				if (Vector3.Distance(fundo.position, transform.position) <= MinReturnDist)
				{
					chainState = ChainState.Away;
					HookManager hook = fundo.GetComponent<HookManager>();
					if (hook.GrabbedWeapon)
					{
						HeldWeapon = hook.GrabbedWeapon;
						hook.GrabbedWeapon = null;
						HeldWeapon.transform.SetParent(transform);
						HeldWeapon.transform.localPosition = HeldWeapon.HeldPos;
						HeldWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
					}
				}
			}
		}
	}

	public void OnThrow(InputAction.CallbackContext context)
	{
		if (!gameObject.activeInHierarchy || !context.performed)
			return;

		// small state machine
		if (HeldWeapon)
			ThrowWeapon();
		else if (chainState == ChainState.Away)
			Throw();
		else if (chainState != ChainState.Returning)
			ReturnToHand();
	}
	public void ReturnToHand()
	{
		FundoRB.isKinematic = true;
		FundoCol.enabled = false;
		chainState = ChainState.Returning;
		playerjoint.ConnectedRigidbody = null;
		chain.positionCount = points.Count;

		fundo.SetParent(transform);

		//chain. = 1;
		chain.SetPosition(0, Vector3.down * 50); // just get the line out of the way

	}
	void ThrowWeapon()
	{
		HeldWeapon.transform.SetParent(null);
		HeldWeapon.GetComponent<Rigidbody>().isKinematic = false;
		HeldWeapon.GetComponent<Rigidbody>().AddForce(transform.forward * ThrowForce, ForceMode.Impulse);
		HeldWeapon = null;
	}
	public void Throw()
	{
		fundo.SetParent(null);

		chainState = ChainState.Thrown;
		FundoRB.isKinematic = false;
		FundoCol.enabled = true;

		FundoRB.AddForce(transform.forward * ThrowForce, ForceMode.Impulse);
	}
	public void Stick()
	{
		// freeze fundo
		FundoRB.isKinematic = true;
		FundoRB.velocity = Vector3.zero;
		chainState = Verlet.ChainState.Stuck;
		FundoCol.enabled = false;

		// set stick length to be equidistant
		float distance = Vector3.Distance(fundo.position, transform.position);
		float sticklength = distance / (float)sticks.Count;
		foreach (Stick stick in sticks)
		{
			stick.length = sticklength;
		}

		// attach player
		playerjoint.Distance = distance * jointDistMultiplier;
		playerjoint.ConnectedRigidbody = FundoRB;
	}
}
