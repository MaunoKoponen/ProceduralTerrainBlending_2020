using UnityEngine;
using System.Collections;

public class TreePatch : IPatch
{
	private Terrain terrain;

	private PatchManager.TerrainInfo m_info;
	private int h1;
	private TreeInstance[] treeInstances;

	NoiseModule m_treeNoise = new RidgedNoise(InfiniteLandscape.RandomSeed);

	public TreePatch(int globTileX_i, int globTileZ_i, Terrain terrain_i, int h0_i, int h1_i, PatchManager.TerrainInfo info)
	{
		terrain = terrain_i;
		m_info = info;
		h1 = h1_i;
	}

	public void ExecutePatch()
	{
		if (!InfiniteTerrain.RenderTreesStatic)
			return;

		FillTreePatch();
		if (h1 == InfiniteTerrain.numOfTreesPerTerrain)
		{
			terrain.terrainData.treeInstances = treeInstances;
		}
	}

	protected float RoadVisibility(float x, float z)
	{
		Color color =  m_info.PathImage.GetPixel(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
		//Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>> color: " + color +  " in  " + x + " / " + z + " name:  " +  m_info.terrain.name +  "  height: " + m_info.PathImage.height );
		return color.r;
	}

	private void FillTreePatch()
	{
		
		if (!InfiniteTerrain.RenderTreesStatic)
			return;
		treeInstances = new TreeInstance[InfiniteTerrain.numOfTreesPerTerrain];

		float bushHeight = 55;
		float testHeight = 100;
		float pineHeight = 200;
		float noTreeHeight = 350;

		// TODO: use terrain type information (via m_info) to decide what kind of forestation is needed:
		// "savannah " trees in random position, spread evenly
		// temperate meadow/forest - trees form forests, meadows between them
		// " bumpy " areas - rather small bushes than trees
		// low land, small bushes, weed
		// mangrove - big trees near and in water, even if steep angle near water 
		// boreal - tundra Pine forests and open lands, trees get smaller ad rare when going up the hill, bare coasts 

		float ForestCenterX = Random.Range(0.5f, 0.6f);
		float ForestCenterY = Random.Range(0.5f, 0.6f);


		// test: dont make trees in castle Area
		string key = m_info.globalX + "_" + m_info.globalZ;

		// TODO if castle data is already created, dont recreate it, just pass the old data to build list


		if (InfiniteTerrain.AreaDict.ContainsKey(key) && InfiniteTerrain.AreaDict[key].castleData != null)
		{
			//Debug.Log("---------------------->>>>>>>>>>>>>>>>>>> castleData.CoordX " + InfiniteTerrain.AreaDict[key].castleData.coordX);
		}
		else
		{
			//Debug.Log("---------------------->>>>>>>>>>>>>>>>>>> castleData key with no castle: "+  key);
		}


		bool castleExists = InfiniteTerrain.AreaDict.ContainsKey(key) && InfiniteTerrain.AreaDict[key].castleData != null;
		var castleData = InfiniteTerrain.AreaDict[key].castleData;

		//Debug.Log("CastExists---------------------------------->" + castleExists);

		for (int k = 0; k < InfiniteTerrain.numOfTreesPerTerrain; k++)
		{
			float x = Random.value;// ForestCenterX + Random.Range(0.0f, 0.4f);
			float z = Random.value;//ForestCenterY + Random.Range(0.0f, 0.4f);

			float inSideCastleValue = 1.0f;

			if (castleExists)
			{


				if ((x < 0.49f || x > 0.61f) || (z < 0.49f || z > 0.61f)) // completely hardcoded, and not dynamic wip solution
				{
					// not inside castle area
				}
				else
				{
					inSideCastleValue = 0; //Scaling so small its invisible 
										   //Debug.Log("Not Making tree: CastleExists and x is " + x + "and z is " + z);
				}
			}


			float forestX = ForestCenterX + Random.Range(0.0f, 0.4f);
			float forestZ = ForestCenterY + Random.Range(0.0f, 0.4f);

			float angle = terrain.terrainData.GetSteepness(x, z);
			float forestAngle = terrain.terrainData.GetSteepness(forestX, forestZ);

			float ht = terrain.terrainData.GetInterpolatedHeight(x, z);
			float forestHt = terrain.terrainData.GetInterpolatedHeight(forestX, forestZ);
			//Debug.LogWarning("Evaluating flatness for tree");

			float roadAlpha = 0;
			if (WorldParameters.createRoads)
			{
				// roadAlpha =  InfiniteTerrain.m_alphaMap[(int)z * 513, (int)x * 513, 4];


				//roadAlpha = m_info.PathImage.GetPixel(Mathf.RoundToInt((int)z * 513), Mathf.RoundToInt((int)x * 513)).r;

				//roadAlpha = terrain.terrainData.GetAlphamapTexture(4).GetPixel((int)z * 513, (int)x * 513).a;

				roadAlpha = RoadVisibility((int)(x * 512.0f), (int)(z * 512.0f));

			}


			if (roadAlpha != 0) // road exists not, then
			{
				//Debug.Log("Road exists: " + roadAlpha + "  " + z + " " + x);
			}


			if (roadAlpha < 0.1f) // road exists not, then
			{
				if (ht > testHeight * 1.1f && ht < pineHeight && angle < 20 && !m_info.HasHills)
				{
					treeInstances[k].position = new Vector3(x, ht / InfiniteTerrain.m_terrainHeight, z);
					treeInstances[k].prototypeIndex = 3;//Random.Range(1, 2);
					treeInstances[k].widthScale = inSideCastleValue * 4;  //Random.Range(4f, 4.5f);
					treeInstances[k].heightScale = inSideCastleValue * 4; //Random.Range(4f, 4.5f);
					treeInstances[k].color = Color.white;
					treeInstances[k].lightmapColor = Color.white;
				}
				else
				{
					if (ht > InfiniteTerrain.waterHeight + 1.1f)
					{
						float noise = 1;  //m_treeNoise.FractalNoise2D(x, y, 2, 100, 0.4f); //= 1; 

						if (forestHt > bushHeight && forestHt < pineHeight && forestAngle < 20 && !castleExists)
						{
							noise = m_treeNoise.FractalNoise2D(forestX, forestZ, 2, 100, 0.4f);
							if (noise > 0)
							{
								treeInstances[k].position = new Vector3(forestX, forestHt / InfiniteTerrain.m_terrainHeight, forestZ);
								treeInstances[k].prototypeIndex = Random.Range(1, 4);
								treeInstances[k].widthScale = Random.Range(2f, 2.5f) * inSideCastleValue;
								treeInstances[k].heightScale = Random.Range(2f, 2.5f) * inSideCastleValue;
								treeInstances[k].color = Color.white;
								treeInstances[k].lightmapColor = Color.white;
							}
						}
						else if (ht > pineHeight && ht < noTreeHeight && angle < 20)
						{
							if (noise > 0)
							{
								treeInstances[k].position = new Vector3(x, ht / InfiniteTerrain.m_terrainHeight, z);
								treeInstances[k].prototypeIndex = Random.Range(4, 6);
								treeInstances[k].widthScale = Random.Range(2f, 2.5f) * inSideCastleValue;
								treeInstances[k].heightScale = Random.Range(2f, 2.5f) * inSideCastleValue;

								treeInstances[k].color = Color.white;
								treeInstances[k].lightmapColor = Color.white;
							}
						}
					}
					else
					{
						treeInstances[k].widthScale = 0;
						treeInstances[k].heightScale = 0;
					}
				}
			}
		}
	}
}

