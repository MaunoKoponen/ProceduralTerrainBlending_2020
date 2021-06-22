using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class NpcSpawner : MonoBehaviour
{
	public GameObject NpcGameObject;

	public GameObject PlayerGameObject;


	//Optional: Warm the pool and preallocate memory
	void Start()
	{
		PoolManager.WarmPool(NpcGameObject, 5);

		//Notes
		// Make sure the prefab is inactive, or else it will run update before first use
	}

	void Update()
	{

		//return;

		if (Input.GetMouseButtonDown(1))
		{
			//Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);

			//RaycastHit info;

			//var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			//if (Physics.Raycast(ray.origin, ray.direction, out info))

				//Debug.Log("---------------------------------------------------------> Click position: " + info.point);

			//Vector3 spawnPoint = new Vector3(info.point.x, 500f, info.point.z);

			Debug.Log("---------------------------------------------------------> PlayerGameObject.transform.position " + PlayerGameObject.transform.position);

			//Vector3 spawnPoint = PlayerGameObject.transform.position + new Vector3(10, 10, 10);


			//Debug.Log("---------------------------------------------------------> spawnPoint " + spawnPoint);


			NavMeshHit hit; // NavMesh Sampling Info Container
			Vector3 randomPos = Random.insideUnitSphere * 3 + PlayerGameObject.transform.position;


			// from randomPos find a nearest point on NavMesh surface in range of maxDistance
			NavMesh.SamplePosition(randomPos, out hit, 3, NavMesh.AllAreas);

			Vector3 spawnPoint = hit.position;


			SpawnNpc(spawnPoint, Quaternion.identity);
		}
	}

	//Spawn pooled objects
	void SpawnNpc(Vector3 position, Quaternion rotation)
	{
		var npc = PoolManager.SpawnObject(NpcGameObject, position, rotation).GetComponent<GameObjectPoolingHelper>();

		//Notes:
		// bullet.gameObject.SetActive(true) is automatically called on spawn 
		// When done with the instance, you MUST release it!
		// If the number of objects in use exceeds the pool size, new objects will be created
	}

}

