using UnityEngine;
using System.Collections;

    [ExecuteInEditMode]
public class Cloner : MonoBehaviour

{
    //Whether you want to clone the object the same time you hit Apply, or just modify its Transform info. 
    public bool autoInstantiate;
    //The settings that will be applied (added or subtracted) to the Transform of this object.
    [Header("Transform Modifiers"),SerializeField] Vector3 worldPositionModifier;
    [SerializeField] Vector3 localPositionModifier;
    [SerializeField] Vector3 worldRotationModifier;
    [SerializeField] Vector3 localRotationModifier;
    [SerializeField] Vector3 ScaleModifier;
    [SerializeField] GameObject objectToClone;
    GameObject objectToName;

    [Header("Other options"),SerializeField,Tooltip("The amount of times your settings will be applied.")] int iterations;
    [SerializeField,Tooltip("The amount of seconds between executions, if we're iterating.")] float ExecuteRate;
    [SerializeField,Tooltip("An add-on for the name of a newly cloned object, depending on which one you hit Apply from.")] int InstancesInScene;
    [SerializeField,Tooltip("The name of a newly cloned object when you apply your settings.")] string cloneName;
	public Transform Spawnpoint;
	public Transform ParentHolder;

	GameObject LastInstantiated;
	GameObject LastLastInstantiated;

	enum jointtype
	{
		Distance3D,
		Configurable
	}
	[Header("Debug"), SerializeField]
	jointtype PrefabJoint = jointtype.Configurable;



    //A coroutine for the iterations. We use this at times, because it allows us to use the following function...
    IEnumerator iteration()
    {
        //...This function will wait the ExecuteRate number of seconds to execute the braced({}) functions.
        yield return new WaitForSeconds(ExecuteRate);
        {
            //Why 2, you ask? Because the OnInspectorGUI void in the Editor script will also execute the 
            //ApplyChanges void, and topping the iteration functions with it causes it to do this the amount 
            //of times it's SUPPOSED to iterate (int Iterations) plus the time you click Apply, so that's one additional iteration.
            if (iterations >= 2)
            {
                //Execute the ApplyChanges function in this script
                ApplyChanges();
                //Decrease the "iterations" integer * 1
                iterations -= 1;
            }
			//One more thing required to compensate for the Editor GUI script- check if the Integer reaches 1,
			//then decrease it to 0, compensating for when we check for if it's 2 or more to decrease
			if (iterations == 1)
			{
				EnablePhysics();
				iterations -= 1;
			}
        }
    }

	public void EnablePhysics()
	{
		foreach(Transform child in ParentHolder)
		{
			if (child != ParentHolder)// && child != transform)
			{
				child.GetComponent<Rigidbody>().isKinematic = false;
				child.GetComponent<Rigidbody>().useGravity = true;

			}
		}
		ParentHolder.GetChild(ParentHolder.childCount - 1).GetComponent<Rigidbody>().isKinematic = true;
		if (PrefabJoint == jointtype.Distance3D)
			ParentHolder.GetChild(ParentHolder.childCount - 1).GetComponent<DistanceJoint3D>().ConnectedRigidbody = Spawnpoint.GetComponentInParent<Rigidbody>();
		else if (PrefabJoint == jointtype.Configurable)
			ParentHolder.GetChild(ParentHolder.childCount - 1).GetComponent<ConfigurableJoint>().connectedBody = Spawnpoint.GetComponentInParent<Rigidbody>();
		//ParentHolder.GetChild(ParentHolder.childCount - 1).SetParent(Spawnpoint);

	}


	public void ApplyChanges()
    {
        //Check if the editable box for Instantiation is ticked, because this means you want to clone the object.
        if (autoInstantiate == true)
        //So that's what we're doing.
        {
            InstancesInScene += 1;
            objectToName = (Instantiate(objectToClone, Spawnpoint.position, Spawnpoint.rotation, ParentHolder));
            objectToName.name = cloneName + InstancesInScene;

			if (LastInstantiated != null && PrefabJoint == jointtype.Distance3D)
			{
				if (LastInstantiated != null)
					LastInstantiated.GetComponent<DistanceJoint3D>().ConnectedRigidbody = objectToName.GetComponent<Rigidbody>();
				else
					GetComponent<DistanceJoint3D>().ConnectedRigidbody = objectToName.GetComponent<Rigidbody>();

				//LastInstantiated.GetComponent<DistanceJoint3D>().ConnectedRigidbody = (LastLastInstantiated == null) ? GetComponent<Rigidbody>() : LastLastInstantiated.GetComponent<Rigidbody>();
				//LastInstantiated.GetComponent<DistanceJoint3D>().PreviousRigidbody = objectToName.GetComponent<Rigidbody>();
			}
			else if (PrefabJoint == jointtype.Configurable)
			{
				if (LastInstantiated != null)
					LastInstantiated.GetComponent<ConfigurableJoint>().connectedBody = objectToName.GetComponent<Rigidbody>();
				else
					GetComponent<ConfigurableJoint>().connectedBody = objectToName.GetComponent<Rigidbody>();

			}
			objectToName.GetComponent<Rigidbody>().isKinematic = true;
			objectToName.GetComponent<Rigidbody>().useGravity = false;

			LastLastInstantiated = LastInstantiated;
			LastInstantiated = objectToName;

			if (objectToClone == null)
                objectToClone = gameObject;
        }
  
        //Apply all of our iterations. The Translate and Rotate functions here are more simple, but they 
        //only work on the object's local axis, hence why it's used here for the local axis modifier settings.
        transform.position += worldPositionModifier;
        transform.eulerAngles += worldRotationModifier;
        transform.Translate(localPositionModifier);
        transform.Rotate(localRotationModifier);
        transform.localScale += ScaleModifier;
        //Start the Iteration coroutine.
        StartCoroutine(iteration());
    }
}
