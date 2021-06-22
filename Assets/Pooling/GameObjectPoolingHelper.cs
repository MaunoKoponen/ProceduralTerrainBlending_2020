using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPoolingHelper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	void Finish()
	{
		PoolManager.ReleaseObject(this.gameObject);

		//Note: 
		// This takes the gameObject instance, and NOT the prefab instance.
		// Without this call the object will never be available for re-use!
		// gameObject.SetActive(false) is automatically called
	}

}
