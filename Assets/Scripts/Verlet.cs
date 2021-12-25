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
	[Header("Verlet Integration"), SerializeField] int numIterations = 10;
	public List<Point> points;
	[SerializeField] List<Stick> sticks;

	public enum ChainState
	{
		Away,
		Thrown,
		Stuck,
		Returning
	}
	[Header("Core Chain Properties")]
	public ChainState chainState = ChainState.Away;
	[SerializeField] HookManager fundoScript;
	[SerializeField] LineRenderer chain;

	[Header("Returning"), SerializeField] float returnSpeed = 2f;
	[SerializeField] float MinReturnDist = 0.05f;

	[Header("Attaching"), SerializeField] DistanceJoint3D playerjoint;
	[SerializeField] float jointDistMultiplier;
	[HideInInspector] Gun HeldWeapon;

	[Header("Pulling values")]
	public Grabbable grabbedObj;

	[Header("DEBUG"), SerializeField]
	bool OnlyPullWhenTaut = false;

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

	private void Start()
	{
		// create lists
		points = new List<Point>();
		sticks = new List<Stick>();

		// create points and sticks
		points.Add(new Point(true)); // end points are manually moved to transforms, so are locked
		for (int i = 1; i < numIterations - 1; i++)
		{
			points.Add(new Point());
		}
		points.Add(new Point(true)); // end points are manually moved to transforms, so are locked
		for (int i = 0; i < numIterations - 1; i++)
		{
			sticks.Add(new Stick(points[i], points[i + 1]));
		}
	}

	public void Update()
	{
		if (chainState != ChainState.Away)
		{
			if (chainState == ChainState.Stuck)
			{
				// keep end points on transforms
				points[0].SetBoth(transform.position);
				points[points.Count - 1].SetBoth(fundoScript.transform.position);
				
				// simulate verlet
				Simulate();
			}
			else // if in midair, keep it a straight line
			{
				for (int i = 0; i < points.Count; i++)
					points[i].SetBoth(Vector3.Lerp(transform.position, fundoScript.transform.position, i / (float)points.Count));
			}

			// update line render
			chain.positionCount = points.Count;
			for (int i = 0; i < points.Count; i++)
				chain.SetPosition(i, points[i].position); 

			if (chainState == ChainState.Returning) // have it lerp back to hand
				Returning();
		}
	}

	void Returning()
	{
		fundoScript.transform.position = Vector3.Lerp(fundoScript.transform.position, transform.position, Time.deltaTime * returnSpeed);
		if (Vector3.Distance(fundoScript.transform.position, transform.position) <= MinReturnDist)
		{
			chainState = ChainState.Away;

			// hide line renderer
			chain.enabled = false;

			if (fundoScript.GrabbedWeapon)
			{
				// set up HeldWeapon
				HeldWeapon = fundoScript.GrabbedWeapon;
				HeldWeapon.Stick(transform);

				fundoScript.GrabbedWeapon = null;
			}
		}
	}
	
	/// <summary>
	/// more like an Intract with Right Hand function
	/// </summary>
	/// <param name="context"></param>
	public void OnPull(InputAction.CallbackContext context)
	{
		if (!context.performed)
			return;

		if (HeldWeapon) // if holding weapon, use it
		{
			HeldWeapon.ShootProjectile(true);
		}
		else if (chainState != ChainState.Away) // else the ball has been thrown, pull on tethered object
		{
			if (OnlyPullWhenTaut && !playerjoint.Taut)
				return;
			//Debug.Log(grabbedObj + " "+ grabbedObj.gameObject);
			if (grabbedObj == null)
				playerjoint.PullPlayer();
			else
				grabbedObj.Pull(playerjoint.transform, fundoScript);
		}
	}

	public void OnThrow(InputAction.CallbackContext context)
	{
		if (!gameObject.activeInHierarchy || !context.performed) // prevents double throws
			return;

		grabbedObj = null;

		// small state machine
		if (HeldWeapon) // if holding a weapon...
		{
			HeldWeapon.Throw(); // toss it
			HeldWeapon = null; // forget about it
		}
		else if (chainState == ChainState.Away) // if holding the ball, throw it
			Throw();
		else if (chainState != ChainState.Returning) // if out, return it
			ReturnToHand();
	}

	public void ReturnToHand()
	{
		fundoScript.Stick(transform); // turn off collisions and connect to hand

		chainState = ChainState.Returning;
		playerjoint.ConnectedRigidbody = null;
	}

	public void Throw()
	{
		// show line renderer
		chain.enabled = true;

		chainState = ChainState.Thrown;
		fundoScript.Throw(transform); // fundo itself handles the throw
	}
	public void Stick()
	{
		// set state
		chainState = Verlet.ChainState.Stuck;

		// set stick length to be equidistant
		float distance = Vector3.Distance(fundoScript.transform.position, transform.position);
		float sticklength = distance / (float)sticks.Count;
		foreach (Stick stick in sticks)
		{
			stick.length = sticklength;
		}

		// attach player
		playerjoint.Distance = distance * jointDistMultiplier;
		playerjoint.ConnectedRigidbody = fundoScript.rb;
	}
}
