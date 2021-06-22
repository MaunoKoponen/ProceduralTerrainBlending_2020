using UnityEngine;
using UnityEngine.AI;

// Walk to a random position and repeat
[RequireComponent(typeof(NavMeshAgent))]
public class RandomWalk : MonoBehaviour
{
    public float m_Range = 25.0f;
    NavMeshAgent m_Agent;

	int counter = 0;

	void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();

    }

    
	void Update()
    {
		counter++;

		/*
		if(counter > 100)
		{
			Debug.Log("----------------------------------------");
			Debug.Log("Agent: activated and enabled " + m_Agent.isActiveAndEnabled);
			Debug.Log("Agent: isOnNavMesh " + m_Agent.isOnNavMesh);
			Debug.Log("Agent: pathStatus " + m_Agent.pathStatus.ToString());
			Debug.Log("Agent: remainingDistance " + m_Agent.remainingDistance);
			Debug.Log("Agent: isPathStale " + m_Agent.isPathStale);
			Debug.Log("Agent: destination " + m_Agent.destination);
			Debug.Log("Agent: pathPending " + m_Agent.pathPending);
		}
		*/
	}
	
}
