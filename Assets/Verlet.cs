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
	[SerializeField] float returnSpeed = 2f;
	[SerializeField] float MinReturnDist = 0.05f;

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
	}

	public void Update()
	{
		if (chainState != ChainState.Away)
		{
			if (chainState == ChainState.Stuck)
			{
				points[0].SetBoth(transform.position);
				Simulate();
			}
			else
			{
				for (int i = 0; i < points.Count; i++)
					points[i].SetBoth(Vector3.Lerp(transform.position, fundo.position, i / (float)points.Count));
			}

			chain.positionCount = points.Count;
			for (int i = 0; i < points.Count; i++)
			{
				chain.SetPosition(i, points[i].position);
			}
			if (chainState == ChainState.Returning)
			{
				fundo.position = Vector3.Lerp(fundo.position, transform.position, Time.deltaTime * returnSpeed);
				if (Vector3.Distance(fundo.position, transform.position) <= MinReturnDist)
					chainState = ChainState.Away;
			}
		}
		else if (chainState == ChainState.Away)
		{
			fundo.position = transform.position;
		}
	}

	public void OnThrow(InputAction.CallbackContext context)
	{
		if (!gameObject.activeInHierarchy || !context.performed)
			return;
		Debug.Log(context.ReadValue<float>());

		if (chainState == ChainState.Away)
			Throw();
		else if (chainState != ChainState.Returning)
			ReturnToHand();
	}
	void ReturnToHand()
	{
		FundoRB.isKinematic = true;
		chainState = ChainState.Returning;
	}
	public void Throw()
	{
		chainState = ChainState.Thrown;
		FundoRB.isKinematic = false;
		FundoRB.AddForce(transform.forward * ThrowForce, ForceMode.Impulse);
	}
	public void Stick()
	{
		FundoRB.isKinematic = true;
		FundoRB.velocity = Vector3.zero;
		chainState = Verlet.ChainState.Stuck;


		foreach (Stick stick in sticks)
		{
			stick.length = Vector3.Distance(fundo.position, transform.position) / (float)sticks.Count;
		}
		points[points.Count - 1].SetBoth(fundo.position);
	}
}
