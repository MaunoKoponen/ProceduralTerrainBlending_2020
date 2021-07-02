using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TerrainScript : MonoBehaviour
{
	bool created = false;
	Terrain thisTerrain;

	public static int tempCounter = 0;

	AreaData areaData;

	public string terrainName;
	public int mapX;
	public int mapZ;

	public int WorlCoordinateStartX;
	public int WorlCoordinateStartZ;

	public int SceneCoordinateStartX;
	public int SceneCoordinateStartZ;

	public int creationTimeI;
	public int creationTimeJ;

	public Texture2D smallScalePathImage;
	public Texture2D largeScalePathImage;

	private bool buildingsGenerated;

	SingleBuildingPlacer buildingPlacer;

	public float[,] TerrainHeights;// = new float[513, 513];

	public int terrainHeightsArraySize = 513;

	void Start ()
    {
		buildingPlacer = GetComponentInParent<SingleBuildingPlacer>();
	}

	float conversionFromWorldXtoSceneX;
	float conversionFromWorldZtoSceneZ;

	void Awake()
	{
		TerrainHeights = new float[terrainHeightsArraySize, terrainHeightsArraySize];
		thisTerrain = GetComponent<Terrain>();
		buildingsGenerated = false;
	}
	
	public void SetImportantValues(string aTerrainName, int terrainsI, int terrainsJ)
	{
		terrainName = aTerrainName;

		mapX = InfiniteTerrain.AreaDict[aTerrainName].mapX;
		mapZ = InfiniteTerrain.AreaDict[aTerrainName].mapZ;

		Debug.Log(">>>>>  SetImportantValues " + terrainName + " terrainsI " + terrainsI + " terrainsJ " + terrainsJ + " mapX " + mapX + " mapZ " + mapZ);
		
		creationTimeI = terrainsI; // the pos in 9x9 grid of terrains - note this value is valid only when creating terrains first time
		creationTimeJ = terrainsJ;

		WorlCoordinateStartX = InfiniteLandscape.m_landScapeSize * mapX; 
		WorlCoordinateStartZ = InfiniteLandscape.m_landScapeSize * mapZ;

		SceneCoordinateStartX = InfiniteLandscape.m_landScapeSize * creationTimeI;
		SceneCoordinateStartZ = InfiniteLandscape.m_landScapeSize * creationTimeJ;

		conversionFromWorldXtoSceneX = InfiniteLandscape.m_landScapeSize * (creationTimeI);
		conversionFromWorldZtoSceneZ = InfiniteLandscape.m_landScapeSize * (creationTimeJ);

		areaData = InfiniteTerrain.GetAreaData(mapX, mapZ);

		if (WorldParameters.createRoads)
		{
			buildingsGenerated = areaData.artefactList != null;

			if (!buildingsGenerated)
			{
				areaData.artefactList = new List<ArtefactInfo>();
			}

			//Debug.Log(">>>>> SetImportantValues WorlCoordinateStartX " + WorlCoordinateStartX + " WorlCoordinateStartZ " + WorlCoordinateStartZ);

			StartCoroutine(GenerateOrRebuildArtefacts());
		}		
	}

	public void EraseArtifacts()
	{
		Debug.Log("EraseArtifacts for " + thisTerrain.name);
		// delete artifacts like buildings in this terrain, referncing the list of them
	}


	IEnumerator GenerateOrRebuildArtefacts()
	{
		//yield return new WaitForSeconds(4); // this still fails to get all buildings on top of terrain
		yield return new WaitForEndOfFrame();

		GenerateOrRebuildArtefacts2();
	}


	public void GenerateOrRebuildArtefacts2()
	{
		if(buildingsGenerated)
		{
			// recreate from artefacts list	
			foreach(ArtefactInfo artefact in areaData.artefactList)
			{
				var gameObject = Instantiate(artefact.prefabToInstantiate, artefact.position, artefact.orientation);
				gameObject.name = artefact.name;

				// TODO use pooling
			}
		}
		else
		{
			// buildings to generate, different types in separate functions

			MakePathwayBuildings();
			
			// once all generated, then
			buildingsGenerated = true; // although that is not necessary, since we get teh boolean out of the existence of artifacts list
		}

		
	}
	
	
	private void MakePathwayBuildings()
	{
		if(! buildingPlacer.Tower)
			return;

		GameObject objectToInstantiate = buildingPlacer.Tower;

		Transform prev = null;

		int counter = 0;

		foreach (var lists in WorldParameters.AllPathsGlobalCoordinated)
		{
			Debug.Log("---------------------------- PathItems  for " + terrainName + " -------------------");

			// Generate "Milestones"
			foreach (var point in lists)
			{
				int myPointX = (int)(point.x * 0.666666666f); // some weird size conversion thing here
				int myPointY = (int)(point.y * 0.666666666f);

				float myPointXFloat = point.x * 0.666666666f; // some weird size conversion thing here
				float myPointZFloat = point.y * 0.666666666f;

				counter++;

				if (WorldPositionInsideThisTerrain(myPointX, myPointY) && counter% 20 == 0)
				{
					float scenePositionX = myPointXFloat - InfiniteTerrain.conversionFromWorldXtoSceneX;
					float scenePositionZ = myPointZFloat - InfiniteTerrain.conversionFromWorldZtoSceneZ;
				
					float height = TerrainPatch.getHeightByWorldPosition(myPointXFloat, myPointZFloat, WorldParameters.staticCurves);

					height = (height * 1500.0f); // + 10.0f;  // just guesswork values

				
					Vector3 scenePositionOnGround = SetOnGround(new Vector3(scenePositionX, 500, scenePositionZ));
					Vector3 worldPos = new Vector3(myPointXFloat, 0, myPointZFloat);

					// takes in terrain coordinates
					Vector3 terrainPos = ConvertWorldCor2TerrCor(scenePositionOnGround, thisTerrain);
					float valX = terrainPos.x;

					// we need to get values to correspond with _this terrain_
					while(valX > 2048)
					{
						valX -= 2048;
					}

					float valZ = terrainPos.z;

					while (valZ > 2048)
					{
						valZ -= 2048;
					}

					Vector3 terPos = new Vector3(valX, 0, valZ)/2.0f;

					if ( RaiseGround(terPos, 64) ) // if too near terrain edge so we cant flaten the ground  properly, then dont build
					{

						GameObject instantiated = Instantiate(objectToInstantiate, scenePositionOnGround, Quaternion.identity); // yes we map x,y to x,y,z thats intended
						instantiated.name = mapX + "_" + mapZ + ": point: " + point.x + "_" + point.y + "scene: " + scenePositionX + "_" + scenePositionZ;

						/*
						if (prev != null)
						{
							PathwayBuilding.transform.LookAt(prev);
						}
						*/

						// crete  artefactinfo, push to list
						ArtefactInfo info = new ArtefactInfo();
						info.prefabToInstantiate = objectToInstantiate;
						info.instantiatedprefab = instantiated;
						info.position = scenePositionOnGround;
						info.orientation = instantiated.gameObject.transform.rotation;
						info.name = instantiated.name;

						areaData.artefactList.Add(info);

						prev = instantiated.transform;
					}
				}
			}	
		}
	}


// Waterside  building
/*
		// Generate water side town Positions
		foreach (var point in WorldParameters.magentaPoints)
		{
			Debug.Log("Handling Magenta: " + point.x + "  " + point.y);

			if (WorldPositionInsideThisTerrain(point.x, point.y))
			{
				Debug.Log("Handling Magenta: " + point.x + "  " + point.y + " is on thisterrain ");

				int correctedPositionX = point.x - InfiniteTerrain.conversionFromWorldXtoSceneX;
				int correctedPositionZ = point.y - InfiniteTerrain.conversionFromWorldZtoSceneZ;

				Vector3 positionOnGround = SetOnGround(new Vector3(correctedPositionX, 500, correctedPositionZ));
				//Vector3 positionOnGround = new Vector3(correctedPositionX, 55, correctedPositionZ); /// waterLevel

				pathbox = Instantiate(pathbox, positionOnGround, Quaternion.identity); // yes we map x,y to x,y,z thats intended
				pathbox.name = "magenta_" + point.x + "_" + point.y;

				pathbox.transform.position = SetOnGround(pathbox.transform.position);
				
				//Debug.Log("PathItem " + pathbox.name);
				//Debug.Log("Position A " + pathbox.transform.position);

				//pathbox.transform.position = SetOnGround(pathbox.transform.position);
				//Debug.Log("Position A " + pathbox.transform.position);
				
			}
			else
			{

				//Debug.Log("Handling Magenta: " + point.x + "  " + point.y + " is NOT on thisterrain ");

			}
		}
		*/


	public static Texture2D Resize(Texture2D source, int newWidth, int newHeight)
	{
		source.filterMode = FilterMode.Bilinear;
		RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
		rt.filterMode = FilterMode.Bilinear;
		RenderTexture.active = rt;
		Graphics.Blit(source, rt);
		Texture2D nTex = new Texture2D(newWidth, newHeight);
		nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
		nTex.Apply();
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary(rt);
		return nTex;
	}




	private bool WorldPositionInsideThisTerrain(int x, int z)
	{
		if(
			x >= WorlCoordinateStartX 
			&& x < WorlCoordinateStartX +InfiniteLandscape.m_landScapeSize
			&& z >= WorlCoordinateStartZ
			&& z < WorlCoordinateStartZ + InfiniteLandscape.m_landScapeSize
		)
			return true;

		return false;
	}


	public void GenerateSomeBuildings()
	{
		Debug.Log("-------------------------> Create Buildings! ");

		int counter = 0;
		int createdAmount = 0;

		if(created)
		{
			return;
		}

		int setX = 64;
		int setZ = 64;

		while(createdAmount <20 && counter < 1000)
		{
			counter++;
			//Vector3 randomPos = new Vector3(120*i, 100, 120*i);// Random.insideUnitSphere * 1000 + new Vector3(1500, 0, 1500);
			
			Vector3 randomPos = (UnityEngine.Random.insideUnitSphere * 500) + new Vector3(1500, 0, 1500) + thisTerrain.transform.position;
			
			
			// good for testing
			//Vector3 randomPos = new Vector3(counter * setX, 0, counter * setZ) + new Vector3 (thisTerrain.transform.position.x,0,thisTerrain.transform.position.z);

			randomPos = new Vector3(randomPos.x, 0, randomPos.z);

			if (!CoordinateInsideThisTerrain(randomPos))
				{
					return;
				}

				created = true;

				// Raycast on terrain

				var tempPoint = new Vector3(randomPos.x, 2000, randomPos.z);
				RaycastHit info;
				Vector3 terrainPos;

				Ray ray = new Ray(tempPoint, Vector3.down);

				if (thisTerrain.GetComponent<Collider>().Raycast(ray, out info, 3000f))
				{
					terrainPos = info.point;
				}
				else
				{
					//Debug.Log("abort, raycast fails");
					continue;
				}

				if (info.point.y  < 55) 
				{
					//Debug.Log("abort, below waterline");
					//continue;
				}

				Vector3 terrainCoor = ConvertWorldCor2TerrCor(terrainPos, thisTerrain);

				var xVal = Map(terrainCoor.x, 0, terrainHeightsArraySize, 0, 1);  
				var zVal = Map(terrainCoor.z, 0, terrainHeightsArraySize, 0, 1);


				float steepness = thisTerrain.terrainData.GetSteepness(zVal, xVal);
					
				GameObject building = buildingPlacer.TooSteep;

				if (steepness > 5f)
				{
					continue;
				//	//building = buildingPlacer.TooSteep;
				}

				//RaiseGround(terrainCoor);
				
				var buildingPosition = SetOnGround(randomPos) ;//+ new Vector3(15,0,15); // test

				building = Instantiate(building, buildingPosition, Quaternion.identity);

				building.name = "BLD_" + thisTerrain.name + "_ " + steepness;

				createdAmount++;

			}
	}

	// needs terrain flush afterwards
	private bool RaiseGround(Vector3 terrainCoordCenter, int size)
	{
		int intXStartOfArea = Mathf.FloorToInt(terrainCoordCenter.x - (size/2.0f)); // UNcentering the area
		int intZStartOfArea = Mathf.FloorToInt(terrainCoordCenter.z - (size/2.0f)) ;

		if (intXStartOfArea <0  || intZStartOfArea < 0)
		{
			return false;
		}

		if(intXStartOfArea + size >=  terrainHeightsArraySize || intZStartOfArea +size  >= terrainHeightsArraySize)
		{
			return false;
		}

		float[,] heights = new float[size, size];

		float desiredHeight = TerrainHeights[(int)terrainCoordCenter.z, (int)terrainCoordCenter.x]; // + 0.005f // for testing 
		float finalHeight =0;

		for (int j = 0; j < size; j++)
		{
			for (int i = 0; i < size; i++)
			{
				float originalHeight = TerrainHeights[intZStartOfArea + j, intXStartOfArea + i];
				Color color = buildingPlacer.HeightMask.GetPixel(i, j);
				float heightDelta = originalHeight - desiredHeight;
				float heightDeltaAfterMask = heightDelta * color.r; // 0...1 value
				finalHeight = originalHeight - heightDeltaAfterMask;
				heights[j, i] = finalHeight;
			}
		}
			
		if (intXStartOfArea + size >= terrainHeightsArraySize || intZStartOfArea + size >= terrainHeightsArraySize)
		{
			return false;
		}

		thisTerrain.terrainData.SetHeights(intXStartOfArea, intZStartOfArea, heights);
		thisTerrain.Flush();

		return true;
	}

	
	private void RaiseGroundWhereClicked()
	{
		if (Input.GetMouseButtonDown(0))
		{
			//Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);

			RaycastHit info;

			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (thisTerrain.GetComponent<Collider>().Raycast(ray, out info, 1000f))
			{
				Debug.Log("---------------------------------------------------------> Click position: " + info.point);
				Vector3 terrainCoor = ConvertWorldCor2TerrCor(info.point, thisTerrain);
				Debug.Log("-------------------> Terrain X: " + terrainCoor.x + "  Terrain Z: " + terrainCoor.z);

				int intX = Mathf.RoundToInt(terrainCoor.x);
				int intZ = Mathf.RoundToInt(terrainCoor.z);

				Debug.Log(" TerrainHeights[x, z] " + TerrainHeights[intX, intZ]  ) ;
				Debug.Log(" TerrainHeights[z, x] " + TerrainHeights[intZ, intX] );
				Debug.Log("terrainCoor: " + terrainCoor);

				int size = 3; 
				float[,] heights = new float[size, size];

				float desiredHeight = TerrainHeights[intZ, intX] + 0.005f;

				for (int i = 0; i < size; i++)
					for (int j = 0; j < size; j++)
						heights[i, j] = desiredHeight;

				thisTerrain.terrainData.SetHeights(intX, intZ, heights);
				thisTerrain.Flush();
			}
		}
	}


	private void RaiseGroundInWorldCoordinate(Vector3 worldPos)
	{
		
		if ( CoordinateInsideThisTerrain(worldPos))
		{
			Vector3 terrainCoor = ConvertWorldCor2TerrCor(worldPos, thisTerrain);

			Debug.Log("RaiseGroundInWorldCoordinate-------------------> Terrain X: " + terrainCoor.x + "  Terrain Z: " + terrainCoor.z);

			int intX = Mathf.RoundToInt(terrainCoor.x);
			int intZ = Mathf.RoundToInt(terrainCoor.z);

			//Debug.Log(" TerrainHeights[x, z] " + TerrainHeights[intX, intZ]);
			//Debug.Log(" TerrainHeights[z, x] " + TerrainHeights[intZ, intX]);
			//Debug.Log("terrainCoor: " + terrainCoor);

			int size = 10;
			float[,] heights = new float[size, size];

			if(intX >= terrainHeightsArraySize || intZ >= terrainHeightsArraySize)
			{
				Debug.Log("RaiseGroundInWorldCoordinate returning... because intX " + intX + "intZ " + intZ + "and terrainHeightsArraySize " + terrainHeightsArraySize);
				return;
			}

			float desiredHeight = TerrainHeights[intZ, intX];// + 0.005f;

			for (int i = 0; i < size; i++)
				for (int j = 0; j < size; j++)
					heights[i, j] = desiredHeight;

			Debug.Log("RaiseGroundInWorldCoordinate Setting heights...");

			thisTerrain.terrainData.SetHeights(intX, intZ, heights);
			// todo: optoimize flus moments
			thisTerrain.Flush();
		}
		Debug.Log("RaiseGroundInWorldCoordinate noyt in this terrai:n " + worldPos +  " / " + thisTerrain.name);
			
	}
		
	


	Vector3 ConvertWorldCor2TerrCor(Vector3 worldCoordinate, Terrain terrain)
	{
		Vector3 TerrainCoordinate = new Vector3();
		//Terrain ter = Terrain.activeTerrain;
		Vector3 terrainsPositionInScene = terrain.transform.position;
		TerrainCoordinate.x = ((worldCoordinate.x - terrainsPositionInScene.x) / terrain.terrainData.size.x) * terrain.terrainData.alphamapWidth * 2;
		TerrainCoordinate.z = ((worldCoordinate.z - terrainsPositionInScene.z) / terrain.terrainData.size.z) * terrain.terrainData.alphamapHeight * 2;
		return TerrainCoordinate;
	}


	bool CoordinateInsideThisTerrain(Vector3 pos)
	{
		Bounds bounds = thisTerrain.terrainData.bounds;
		bounds.center = thisTerrain.transform.position + new Vector3(1024, 0, 1024);
		bounds.Expand(new Vector3(0, 1000, 0));

		if(bounds.Contains(pos))
		{
			return true;
		}
		return false;
	}


	Vector3 SetOnGround(Vector3 position)
	{
		RaycastHit hit;

		var tempPoint = new Vector3(position.x, 2000, position.z);

		if (Physics.Raycast(tempPoint, transform.TransformDirection(Vector3.down), out hit, 3000))
		{
			//UnityEngine.Debug.Log("Found Ground!!  " + hit.point);
			position = hit.point;
		}
		else
		{
			//UnityEngine.Debug.Log("Did Not Find Ground!!");
		}
		return position;
	}


	float Map(float s, float a1, float a2, float b1, float b2)
	{
		return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
	}


	void Update_unused()
	{
		//RaiseGroundWhereClicked();
		//GenerateSomeBuildings();

		float height = 0;

		if (Input.GetMouseButtonDown(1))
		{

			// testing  getHeightByWorldPosition value accuracy:

			Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);

			RaycastHit info;
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray.origin, ray.direction, out info))
			{


				float worldX = info.point.x * 1.0f + InfiniteTerrain.conversionFromWorldXtoSceneX;
				float worldZ = info.point.z * 1.0f + InfiniteTerrain.conversionFromWorldZtoSceneZ;

				if (WorldPositionInsideThisTerrain((int)worldX, (int)worldZ))
				{

					//Debug.Log("---------> info.point " + info.point + " world " + worldX + "  " + worldZ);

					height = TerrainPatch.getHeightByWorldPosition(worldX, worldZ, WorldParameters.staticCurves);
					height = height * 1500.00f;

					float difference = height - info.point.y;

					Debug.Log("point " + info.point.y + " <-------> " + height + "  difference >>>>>> " + difference);

				}
			}
		}
	}



}

