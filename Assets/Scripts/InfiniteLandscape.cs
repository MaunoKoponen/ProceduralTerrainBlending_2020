using UnityEngine;
using System.Collections;

public class InfiniteLandscape : MonoBehaviour
{
	public static int RandomSeed = 1;
	public static bool BuildOnlyCenterTile;


	public GameObject PlayerObject;

    public static float waterHeight = 50;

	public static int m_landScapeSize = 2048;// 3072;

	// Amount of terrains used: 3 = 3x3:
	protected const int dim = 3;

	public static int initialGlobalIndexX = 234;

	public static int initialGlobalIndexZ = 234;

	public static int initialPlayerPositionX = 300;
	public static int initialPlayerPositionZ = 300;



	protected bool patchIsFilling = false;
    protected int prevGlobalIndexX = -1;
    protected int prevGlobalIndexZ = -1;
	protected int curGlobalIndexX = initialGlobalIndexX + 1;
    protected int curGlobalIndexZ = initialGlobalIndexZ + 1;
    protected int prevLocalIndexX = -1;
    protected int prevLocalIndexZ = -1;
    protected int curLocalIndexX = 1;
    protected int curLocalIndexZ = 1;
    protected int prevCyclicIndexX = -1;
    protected int prevCyclicIndexZ = -1;
    protected int curCyclicIndexX = 1;
    protected int curCyclicIndexZ = 1;

    protected bool updateLandscape = false;

    protected bool UpdateIndexes()
    {

		int currentLocalIndexX = GetLocalIndex(PlayerObject.transform.position.x);
        int currentLocalIndexZ = GetLocalIndex(PlayerObject.transform.position.z);

        if (curLocalIndexX != currentLocalIndexX || curLocalIndexZ != currentLocalIndexZ)
        {

			Debug.LogError("----------------- UpdateIndexes -> changed -----------------");


			prevLocalIndexX = curLocalIndexX;
            curLocalIndexX = currentLocalIndexX;

            prevLocalIndexZ = curLocalIndexZ;
            curLocalIndexZ = currentLocalIndexZ;

            int dx = curLocalIndexX - prevLocalIndexX;
            int dz = curLocalIndexZ - prevLocalIndexZ;
            
			prevGlobalIndexX = curGlobalIndexX;
            curGlobalIndexX += dx;
            
			prevGlobalIndexZ = curGlobalIndexZ;
            curGlobalIndexZ += dz;
            prevCyclicIndexX = curCyclicIndexX;


			if(curGlobalIndexX > prevGlobalIndexX)
			{
				curCyclicIndexX = curCyclicIndexX + 1;
				if (curCyclicIndexX > 2)
					curCyclicIndexX = 0;
			}
			else if (curGlobalIndexX < prevGlobalIndexX)
			{
				curCyclicIndexX = curCyclicIndexX - 1;
				if (curCyclicIndexX < 0)
					curCyclicIndexX = 2;
			}

			prevCyclicIndexZ = curCyclicIndexZ;

			if (curGlobalIndexZ > prevGlobalIndexZ)
			{
				curCyclicIndexZ = curCyclicIndexZ + 1;
				if (curCyclicIndexZ > 2)
					curCyclicIndexZ = 0;
			}
			else if (curGlobalIndexZ < prevGlobalIndexZ)
			{
				curCyclicIndexZ = curCyclicIndexZ - 1;
				if (curCyclicIndexZ < 0)
					curCyclicIndexZ = 2;
			}
			
			Debug.Log("Entered new terrain at : " + curGlobalIndexX + "  " + curGlobalIndexZ);

			return true;
        }
        else return false;
    }

    public static int GetLocalIndex(float x)
    {
        return (Mathf.CeilToInt(x / m_landScapeSize));
    }


    protected virtual void Update()
    {
        if (UpdateIndexes())
            updateLandscape = true;
        else
            updateLandscape = false;
    }
}
