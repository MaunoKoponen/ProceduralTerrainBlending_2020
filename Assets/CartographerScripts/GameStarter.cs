using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameStarter : MonoBehaviour
{

	public WorldParameters worldParameters;

	// Start is called before the first frame update
	void Start()
    {
		WorldParameters.startedFromMenu = true;
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public void ConvertClickToMapCordinate(Vector2 localPoint)
	{
		Vector2 coordinatesFromZero = localPoint + new Vector2(256, 256);
		Vector2 selectedTerrain = coordinatesFromZero / (512/worldParameters.worldSizeX); // ok as long as world x and y sizes match 
		Debug.Log("selectedTerrain  -----------------------> " + Mathf.Floor(selectedTerrain.x ) + "  " + Mathf.Floor(selectedTerrain.y));

		InfiniteLandscape.initialGlobalIndexX = (int)Mathf.Floor(selectedTerrain.x) - 1; // -1 reason: the terrain "grid" is created from 0.0 to 2.2 and player placed in center, 
		InfiniteLandscape.initialGlobalIndexZ = (int)Mathf.Floor(selectedTerrain.y) - 1;

		InfiniteLandscape.initialPlayerPositionX  = (int)(coordinatesFromZero.x % (512 / worldParameters.worldSizeX))*32;  // should end up being max 3072
		InfiniteLandscape.initialPlayerPositionZ = (int)(coordinatesFromZero.y % (512 / worldParameters.worldSizeY)) * 32;  // should end up being max 3072

		//-(selectedTerrain * (512.0f / worldParameters.worldSizeX));
		//InfiniteLandscape.initialPlayerPositionZ  = (int)(selectedTerrain.y - InfiniteLandscape.initialGlobalIndexZ) * InfiniteLandscape.m_landScapeSize;

		//Debug.Log("Player Coords: selectedTerrain.x * (512.0f / worldParameters.worldSizeX) " +selectedTerrain.x * (512.0f / worldParameters.worldSizeX));
		Debug.Log("Player Coords: -------------------------------------------------------> x " + InfiniteLandscape.initialPlayerPositionX);
		Debug.Log("Player Coords: -------------------------------------------------------> z " + InfiniteLandscape.initialPlayerPositionZ);


	}

	public void StartTerrainEngine()
	{
		  SceneManager.LoadScene("CartographerTerrainEngine");
	}

}
