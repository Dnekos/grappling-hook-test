using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMesh : MonoBehaviour
{
	[SerializeField] DistanceJoint3D joint;
	[SerializeField] Transform meshHolder;
    // Start is called before the first frame update
    void Start()
    {
		joint = GetComponent<DistanceJoint3D>();
		//meshHolder = transform.GetChild(0);
	}

    // Update is called once per frame
    void Update()
    {
		if (joint.ConnectedRigidbody == null)
			return;

		meshHolder.LookAt(joint.ConnectedRigidbody.transform);//, Vector3.forward);
		meshHolder.localEulerAngles = new Vector3(meshHolder.localEulerAngles.x, meshHolder.localEulerAngles.y, joint.ConnectedRigidbody.transform.localEulerAngles.z + 90);
		//transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position), rotationSpeed * Time.deltaTime);

	}
}
