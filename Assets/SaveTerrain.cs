using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SaveTerrain : MonoBehaviour
{
	RaycastHit m_HitInfo = new RaycastHit();

	
	// Start is called before the first frame update
    void Start()
    {
        
    }

	// Update is called once per frame
	void Update()
	{
		
		if (Input.GetKeyDown("t") && !Input.GetKey(KeyCode.LeftShift))
		{
			Debug.Log("Save terrain!   " );

			TerrainData terrainData;
			Terrain terrain;

			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray.origin, ray.direction, out m_HitInfo))
			{
				Debug.Log(m_HitInfo.transform.name);
				terrain = m_HitInfo.transform.gameObject.GetComponent<Terrain>();
				terrainData = terrain.terrainData;

				Debug.Log("terrainData treeInstanceCount " + terrainData.treeInstanceCount);

#if UNITY_EDITOR

				//saveEverything(terrain);

				//These save just terrain height and trees:
				//float[,,] sourceAlphamaps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
				//terrainData.SetAlphamaps(0, 0, sourceAlphamaps);
				//terrain.Flush();
				AssetDatabase.CreateAsset(terrainData, "Assets/TestSaveTerrainData.asset");
				AssetDatabase.SaveAssets();
#endif
			}	
		}
	}


	void saveEverything(Terrain sourceTerrain)
	{


#if UNITY_EDITOR

		TerrainData workData = new TerrainData();
		//workData = my new TerrainData created in code, sourceData = the TerrainData attached to the source terrain

		workData.SetDetailResolution(256, 16);
		workData.size = new Vector3(3072/16, 3072/16, 3072/16);

		workData.alphamapResolution = sourceTerrain.terrainData.alphamapResolution;
		workData.baseMapResolution = sourceTerrain.terrainData.baseMapResolution;
		SplatPrototype[] workSplatPrototypes = new SplatPrototype[sourceTerrain.terrainData.splatPrototypes.Length];
		for (int sp = 0; sp < workSplatPrototypes.Length; sp++)
		{
			SplatPrototype clonedSplatPrototype = new SplatPrototype();
			// texture
			clonedSplatPrototype.texture = sourceTerrain.terrainData.splatPrototypes[sp].texture;
			// tileSize
			clonedSplatPrototype.tileSize = sourceTerrain.terrainData.splatPrototypes[sp].tileSize;
			// tileOffset
			clonedSplatPrototype.tileOffset = sourceTerrain.terrainData.splatPrototypes[sp].tileOffset;

			workSplatPrototypes[sp] = clonedSplatPrototype;
		}

		workData.splatPrototypes = workSplatPrototypes;
		// TODO: Figure out how to copy the resolutionPerPatch - currently hard coded to 16
		
		workData.SetDetailResolution(256, 16);

		//workData.SetDetailResolution(sourceTerrain.terrainData.detailResolution, 16);

		Debug.Log("----> sourceTerrain.terrainData.heightmapWidth " + sourceTerrain.terrainData.heightmapResolution);
		Debug.Log("----> WorkData heightmapWidth " + workData.heightmapResolution);
		Debug.Log("----> sourceTerrain.terrainData.heightmapScale " + sourceTerrain.terrainData.heightmapScale);
		Debug.Log("----> WorkData heightmapScale " + workData.heightmapScale);

		workData.heightmapResolution = sourceTerrain.terrainData.heightmapResolution;
		Debug.Log("--> source heightmapresolution " + sourceTerrain.terrainData.heightmapResolution);
		Debug.Log("--> work   heightmapresolution " + workData.heightmapResolution);

		float[,] sourceHeights = sourceTerrain.terrainData.GetHeights(0, 0, sourceTerrain.terrainData.heightmapResolution, sourceTerrain.terrainData.heightmapResolution);
		workData.SetHeights(0, 0, sourceHeights);

		float[,,] sourceAlphamaps = sourceTerrain.terrainData.GetAlphamaps(0, 0, sourceTerrain.terrainData.alphamapWidth, sourceTerrain.terrainData.alphamapHeight);
		workData.SetAlphamaps(0, 0, sourceAlphamaps);

		
		// Forum note: The below two are only here so I can verify that it did in fact copy the alphamap over properly. I have a break point here so I can manually inspect the sourceAlphamaps and newAlphamaps to compare them.
		//float[,,] newAlphamaps = workData.GetAlphamaps(0, 0, workData.alphamapWidth, workData.alphamapHeight);
		//Debug.Log(newAlphamaps.Length);


		workData.RefreshPrototypes();
		Terrain workTerrain = (Terrain)Terrain.CreateTerrainGameObject(workData).GetComponent(typeof(Terrain));
		workTerrain.Flush();

		AssetDatabase.CreateAsset(workData, "Assets/TestSaveTerrainData.asset");
		AssetDatabase.SaveAssets();
		workData.SetAlphamaps(0, 0, sourceAlphamaps);

#endif

	}
}
