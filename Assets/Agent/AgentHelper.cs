using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentHelper : MonoBehaviour
{
	public NavMeshAgent agent;
	bool inited = false;
	void Start()
    {
		//NavMeshAgent agent = GetComponent("NavMeshAgent") as NavMeshAgent;
		
		// Move to top hierarchy - use for pooling at some point
		//transform.parent = null;
	}

    // Update is called once per frame
    void Update()
    {
		if(! inited)
		{
			NavMeshHit closestHit;
			if (NavMesh.SamplePosition(transform.position, out closestHit, 100f, NavMesh.AllAreas))
			{
				transform.position = closestHit.position;
				agent.enabled = true;
				inited = true;
				//Debug.Log("Agenthelper: Agent inited");
			}
		
		}
    }

}
