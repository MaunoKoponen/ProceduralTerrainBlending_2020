using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cullingTest2 : MonoBehaviour
{
	// distance to search objects from
	public float searchDistance = 200;

	public bool colorInvisibleObjects = false;

	CullingGroup cullGroup;
	BoundingSphere[] bounds;

	public Camera m_camera;

	public GameObject[] targets;
	public List<GameObject> targetsList;


	void Start()
	{
		targetsList = new List<GameObject>();

		
		

		cullGroup = new CullingGroup();

		// measure distance to our transform
		cullGroup.SetDistanceReferencePoint(m_camera.transform);


		// search distance "bands" starts from 0, so index=0 is from 0 to searchDistance
		cullGroup.SetBoundingDistances(new float[] { searchDistance, float.PositiveInfinity });

		Setup();
		// subscribe to event
		cullGroup.onStateChanged += StateChanged;


	}

	public void Setup()
	{
		// All the objects that have a sphere tag

		
		// create culling group
		cullGroup.targetCamera = m_camera;

		var gobjs = GameObject.FindGameObjectsWithTag("CloseRange");

		for (int i = 0; i < gobjs.Length; i++)
		{
			Debug.Log(i + "--> " + gobjs[i].name);
		}

		targets = new GameObject[gobjs.Length];
		targetsList.Clear();
		
		targetsList.AddRange(gobjs);
	
			
		for (int i = 0; i < gobjs.Length; i++)
		{
			targets[i] = gobjs[i];
		}
		
		
		bounds = new BoundingSphere[targetsList.Count];

		for (int i = 0; i < targetsList.Count; i++)
		{
			var b = new BoundingSphere();
			b.position = targetsList[i].transform.position;
			//Debug.Log("Setting  BoundingSphere position to " + b.position);

			// get simple radius
			b.radius = 3;
			bounds[i] = b;
		}

		// set bounds that we track
		cullGroup.SetBoundingSpheres(bounds);

		//cullGroup.SetBoundingSpheres(bounds);
		cullGroup.SetBoundingSphereCount(targetsList.Count);



		/*
		bounds = new BoundingSphere[targets.Length];

		for (int i = 0; i < targets.Length; i++)
		{
			var b = new BoundingSphere();
			b.position = targets[i].transform.position;
			Debug.Log("Setting  BoundingSphere position to " +  b.position);
		
			// get simple radius..works for our sphere
			b.radius = 3; // targets[i].GetComponent<MeshFilter>().mesh.bounds.extents.x;
	
			Debug.Log("Setting  BoundingSphere radius to " + b.radius);

			bounds[i] = b;
		}

		// set bounds that we track
		cullGroup.SetBoundingSpheres(bounds);

		//cullGroup.SetBoundingSpheres(bounds);
		cullGroup.SetBoundingSphereCount(targets.Length);

		// subscribe to event
		cullGroup.onStateChanged += StateChanged;
		*/

	}

	void Update()
	{

		if (Input.GetKeyDown("p") && !Input.GetKey(KeyCode.LeftShift))
		{
			Debug.Log("Recalculate Culling ");
			Setup();
		}
		cullGroup.SetDistanceReferencePoint(m_camera.transform);
	}


	// object state has changed in culling group
	void StateChanged(CullingGroupEvent e)
	{

		//Debug.Log("State Changed..");
		if (colorInvisibleObjects == true && e.isVisible == false)
		{
			//objects[e.index].GetComponent<Renderer>().material.color = Color.gray;
			//return;
		}

		// if we are in distance band index 0, that is between 0 to searchDistance
		if (e.currentDistance == 0)
		{
			//Debug.Log("Here!");
			targetsList[e.index].GetComponent<GameObjectHolder>().holder.SetActive(true);

			//objects[e.index].GetComponent<Renderer>().material.color = Color.green;
		}
		else // too far, set color to red
		{
			//Debug.Log("Here! 2");
			targetsList[e.index].GetComponent<GameObjectHolder>().holder.SetActive(false);
			//objects[e.index].GetComponent<Renderer>().material.color = Color.red;
		}
	}

	// cleanup
	private void OnDestroy()
	{
		cullGroup.onStateChanged -= StateChanged;
		cullGroup.Dispose();
		cullGroup = null;
	}

}