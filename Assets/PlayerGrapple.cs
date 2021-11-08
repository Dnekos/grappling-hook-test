using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrapple : MonoBehaviour
{
	[SerializeField] GameObject FundoPrefab;
	[SerializeField] Transform ropeholder;

	bool throwing = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }


	public void OnThrow()
	{
		if (throwing)
			return;
		throwing = true;
		Cloner fundo = Instantiate(FundoPrefab, transform.position, transform.rotation, ropeholder).GetComponent<Cloner>();
		fundo.Spawnpoint = transform;
		fundo.ParentHolder = ropeholder;
		fundo.ApplyChanges();
	}
    // Update is called once per frame
    void Update()
    {
        
    }
}
