using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoReticle : MonoBehaviour
{
	[SerializeField] Transform reticle;
	LayerMask enemyMask;
	Camera cam;

	public Transform enemy;

	Vector3 center;

    // Start is called before the first frame update
    void Start()
	{
		center = reticle.position;
		cam = Camera.main;
		enemyMask = LayerMask.GetMask("Enemies");
    }

	// Update is called once per frame
	void Update()
	{
		if (enemy == null)
		{
			Ray camray = cam.ScreenPointToRay(center);
			RaycastHit info;
			if (Physics.Raycast(camray, out info, 20, enemyMask))
			{
				enemy = info.transform;
			}
		}
		else
		{
			reticle.position = cam.WorldToScreenPoint(enemy.position);
			if (Vector3.Distance(center, reticle.position) > 200)
			{
				enemy= null;
				reticle.position = center;
			}	
		}
	}
}
