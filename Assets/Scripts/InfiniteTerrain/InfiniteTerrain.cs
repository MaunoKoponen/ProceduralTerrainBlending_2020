using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Diagnostics;
//using VisualDesignCafe.Nature;


public class ArtefactInfo
{
	public GameObject prefabToInstantiate;
	public GameObject instantiatedprefab;

	public Vector3 position;
	public Quaternion orientation;
	public string name;
}

public class AreaData
{
	public string areaName;
	public int landMassValue;
	public int mapX;
	public int mapZ;
	public float xCoord;
	public float yCoord;
	public CastleData castleData;
	public List<ArtefactInfo> artefactList;
}

public class CastleData
{
	public int size;
	public GameObject rootGameObject;

	public int mapX;
	public int mapZ;

	public string terrainName;
	public float coordX;

	public float coordZ;
	public float coordXMin;
	public float coordZMin;

	public float coordXMax;
	public float coordZMax;


	public Vector3 NavMeshPos;
	public Vector3 NavMeshSize;



}

public class InfiniteTerrain : InfiniteLandscape
{
	public LocalNavMeshBuilder navMeshBuilder;
	public GameObject navMeshGameObject;

	public static NoiseModule m_mapNoise;

	// for using something else than Unity built-in material for terrain
	public bool useTestMaterial;
	public static bool RenderTreesStatic;
	public bool RenderTrees;
	public static bool RenderDetailsStatic;
	public bool RenderDetails;

	public Material testMaterial;
	public bool useDrawInstancing;


	public static float conversionFromWorldXtoSceneX = 0;
	public static float conversionFromWorldZtoSceneZ = 0;


	public static Dictionary<string, AreaData> AreaDict = new Dictionary<string, AreaData>();


	// 2-dimensional table for holding values that form a round sine bell shape 
	public static float[,] fallOffTable;

	static int[] m_storedRandoms = new int[100];
	static int storedRandomsCounter = 0;
	private IPatch patchToBeFilled = null;
    bool terrainIsFlushed = true;

    public const int m_heightMapSize = 513;
    public const float m_terrainHeight = 1500;
    public static float[,] m_terrainHeights = new float[m_heightMapSize, m_heightMapSize];

    public static Terrain[,] m_terrainGrid = new Terrain[dim, dim];


	public static Biome biomeStatic;
	public Biome biome;

	[Header("Trees")]

	//public GameObject[] Trees;
    public static int numOfTreesPerTerrain = 9000;
	//private GameObject[] trees = new GameObject[numOfTreePrototypes];
	TreePrototype[] m_treeProtoTypes; //= new TreePrototype[numOfTreePrototypes];
    public float m_treeDistance = 2000.0f;          //The distance at which trees will no longer be drawn
    public float m_treeBillboardDistance = 400.0f;  //The distance at which trees meshes will turn into tree billboards
    public float m_treeCrossFadeLength = 50.0f;     //As trees turn to billboards there transform is rotated to match the meshes, a higher number will make this transition smoother
    public int m_treeMaximumFullLODCount = 400;     //The maximum number of trees that will be drawn in a certain area. 


	[Header("Textures")]


	public const int m_alphaMapSize = (InfiniteTerrain.m_heightMapSize - 1) * 1;
	//public const int m_alphaMapSize = (InfiniteTerrain.m_heightMapSize - 1) *2; // gives more details but is slower

	public const int TerrainTextureAmount = 5;
	
	public static float[, ,] m_alphaMap = new float[m_alphaMapSize, m_alphaMapSize, TerrainTextureAmount];
    
	

	[Header("Details")]

	public const int numOfDetailPrototypes = 6;
	private DetailPrototype[] m_detailProtoTypes = new DetailPrototype[numOfDetailPrototypes];
	public Texture2D[] detailTexture = new Texture2D[numOfDetailPrototypes];

	public GameObject[] detailMesh = new GameObject[numOfDetailPrototypes];

	private DetailRenderMode detailMode;

    public const int m_detailMapSize = m_alphaMapSize;                 //Resolutions of detail (Grass) layers SHOULD BE EQUAL TO Terrain texture reso
    public static int[,] detailMap0 = new int[m_detailMapSize, m_detailMapSize];
    public static int[,] detailMap1 = new int[m_detailMapSize, m_detailMapSize];
    public static int[,] detailMap2 = new int[m_detailMapSize, m_detailMapSize];
	public static int[,] detailMap3 = new int[m_detailMapSize, m_detailMapSize];
	public static int[,] detailMap4 = new int[m_detailMapSize, m_detailMapSize];
	public static int[,] detailMap5 = new int[m_detailMapSize, m_detailMapSize];

	//These are private, because changing these without too much thought will introduce artifacts in density/placement
	private int m_detailObjectDistance = 500;//500;                                //The distance at which details will no longer be drawn
	private float m_detailObjectDensity = 1.0f;     // 0.0 ... 1.0                         //Creates more dense details within patch
	private int m_detailResolutionPerPatch = 8;//16;                             //The size of detail patch. A higher number may reduce draw calls as details will be batch in larger patches

	public float m_wavingGrassStrength = 50.0f;//0.4f;
	public float m_wavingGrassAmount = 50.0f;//0.2f;
	public float m_wavingGrassSpeed = 0.4f;
    public Color m_wavingGrassTint = Color.white;
    public Color m_grassHealthyColor = Color.white;
    public Color m_grassDryColor = Color.white;

	//UI
	/*
	public Text terrainName;
	public Text hillsValue;
	public Text mountainsValue;
	public Text ridgedMountainsValue;
	public Text plainsValue;
	*/
	
	// Castle/town that will obey terrain height
	public CastleCreator castleCreator;
	public bool createCastle;
	private List<CastleData> CastleBuildList = new List<CastleData>();


	public SingleBuildingPlacer buildingPlacer;


	// this is used to create "fjord"- landmass type. Its like small walleys and flat topped and hills 
	public AnimationCurve TestCurve;
	public static AnimationCurve StaticTestCurve;

	public Curves curves;


	// not in use currently
	public AnimationCurve TestCurve2;
	public static AnimationCurve StaticTestCurve2;
	Stopwatch stopWatch;
	void Awake()
	{

		biomeStatic = biome;

		PlayerObject.gameObject.SetActive(false);

		stopWatch = new Stopwatch();
		stopWatch.Start();
		UnityEngine.Debug.Log("InfiniteTerrain Awake -> " + stopWatch.Elapsed.Seconds);

		RenderDetailsStatic = WorldParameters.renderDetails; // todo use WorldParameters everyWhere for these 
		RenderTreesStatic = WorldParameters.renderTrees;

		StaticTestCurve = curves.HillCurve;
		//StaticTestCurve = TestCurve;
		//StaticTestCurve2 = TestCurve2;

		Texture2D falloff = new Texture2D(513, 513);
		
		fallOffTable = GenerateFalloffTable();

		UnityEngine.Debug.Log("InfiniteTerrain Awake generated fallofftable -> " + stopWatch.Elapsed.Seconds);

		UnityEngine.Random.InitState(InfiniteLandscape.RandomSeed);

		m_treeProtoTypes = new TreePrototype[biome.Trees.Length];

		for (int i = 0; i < biome.Trees.Length; i++)
		{
			m_treeProtoTypes[i] = new TreePrototype();
			m_treeProtoTypes[i].prefab = biome.Trees[i];
		}


		for (int i = 0; i < numOfDetailPrototypes; i++)
		{
			// if item detail mesh list empty, use that to override the texture:
			if (biome.detailMesh[i] != null)
			{
				m_detailProtoTypes[i] = new DetailPrototype();
				m_detailProtoTypes[i].usePrototypeMesh = true;

				m_detailProtoTypes[i].prototype = biome.detailMesh[i];
				m_detailProtoTypes[i].renderMode = DetailRenderMode.VertexLit;

				m_detailProtoTypes[i].minHeight = 0.5f;
				m_detailProtoTypes[i].minWidth = 0.5f;

				m_detailProtoTypes[i].maxHeight = 1;
				m_detailProtoTypes[i].maxWidth = 1;

				//m_detailProtoTypes[i].noiseSpread = ???

				m_detailProtoTypes[i].healthyColor = Color.white;
				m_detailProtoTypes[i].dryColor = Color.white;
			}
			else if(biome.detailTexture[i] != null)
			{
				m_detailProtoTypes[i] = new DetailPrototype();
				m_detailProtoTypes[i].prototypeTexture = biome.detailTexture[i];
				m_detailProtoTypes[i].renderMode = detailMode;
				m_detailProtoTypes[i].healthyColor = m_grassHealthyColor;
				m_detailProtoTypes[i].dryColor = m_grassDryColor;
				m_detailProtoTypes[i].maxHeight = 1;//0.5f;
				m_detailProtoTypes[i].maxWidth = 1;//0.2f;
				m_detailProtoTypes[i].noiseSpread = 0.5f;
			}
		}
	}

	void OnEnable()
	{

		UnityEngine.Debug.Log("InfiniteTerrain OnEnable -> " + stopWatch.Elapsed.Seconds);

		for (int i = 0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
                TerrainData terrainData = new TerrainData();

				terrainData.thickness = 10; // test value
				terrainData.wavingGrassStrength = m_wavingGrassStrength;
                terrainData.wavingGrassAmount = m_wavingGrassAmount;
                terrainData.wavingGrassSpeed = m_wavingGrassSpeed;
                terrainData.wavingGrassTint = m_wavingGrassTint;
                terrainData.heightmapResolution = m_heightMapSize;
                terrainData.size = new Vector3(m_landScapeSize, m_terrainHeight, m_landScapeSize);
                terrainData.alphamapResolution = m_alphaMapSize;


				terrainData.treePrototypes = m_treeProtoTypes;
                terrainData.SetDetailResolution(m_detailMapSize, m_detailResolutionPerPatch);
				terrainData.detailPrototypes = m_detailProtoTypes;

				// for adding nav mesh:
				GameObject terrainGameObject = Terrain.CreateTerrainGameObject(terrainData);
				
				// TEST:
				terrainGameObject.AddComponent<NavMeshSourceTag>();
				
				//terrainGameObject.AddComponent<NatureRenderer>();

				// If need to Access terrain itself here:
				Terrain terrain = terrainGameObject.GetComponent<Terrain>();

				FillTerrainLayer(terrain);

				m_terrainGrid[i, j] = terrain;

				if (useTestMaterial)
				{
					m_terrainGrid[i, j].materialType = Terrain.MaterialType.Custom;
					m_terrainGrid[i, j].materialTemplate = testMaterial;
				}

				if(useDrawInstancing)
					m_terrainGrid[i, j].drawInstanced = false;

			}
        }

		UnityEngine.Debug.Log("InfiniteTerrain OnEnable terrains set up -> " + stopWatch.Elapsed.Seconds);

		/* Original:
		for (int i = 0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
                m_terrainGrid[i, j].gameObject.AddComponent<TerrainScript>();
                m_terrainGrid[i, j].transform.parent = gameObject.transform;

                m_terrainGrid[i, j].transform.position = new Vector3(
                m_terrainGrid[1, 1].transform.position.x + (i - 1) * m_landScapeSize, m_terrainGrid[1, 1].transform.position.y,
                m_terrainGrid[1, 1].transform.position.z + (j - 1) * m_landScapeSize);

                m_terrainGrid[i, j].treeDistance = m_treeDistance;
                m_terrainGrid[i, j].treeBillboardDistance = m_treeBillboardDistance;
                m_terrainGrid[i, j].treeCrossFadeLength = m_treeCrossFadeLength;
                m_terrainGrid[i, j].treeMaximumFullLODCount = m_treeMaximumFullLODCount;

                m_terrainGrid[i, j].detailObjectDensity = m_detailObjectDensity;
                m_terrainGrid[i, j].detailObjectDistance = m_detailObjectDistance;

                m_terrainGrid[i, j].GetComponent<Collider>().enabled = false;
                m_terrainGrid[i, j].basemapDistance = 4000;
                m_terrainGrid[i, j].castShadows = false;

                // m_terrainGrid[i, j].terrainData.wavingGrassAmount = 1000;

                string xName = (curGlobalIndexX + i - 1).ToString(); // name will be used for identifying the correct entry in landmamassDictionary
                string zName = (curGlobalIndexZ + j - 1).ToString();
                m_terrainGrid[i, j].name = xName + "_new_" + zName;

                PatchManager.AddTerrainInfo(curGlobalIndexX + i - 1, curGlobalIndexZ + j - 1, m_terrainGrid[i, j], m_terrainGrid[i, j].transform.position);
			}
        }
		*/

		// Just the center tile
		for (int i = 1; i < 2; i++)
		{
			for (int j = 1; j < 2; j++)
			{


				m_terrainGrid[i, j].gameObject.AddComponent<TerrainScript>();
				
				m_terrainGrid[i, j].transform.parent = gameObject.transform;

				m_terrainGrid[i, j].transform.position = new Vector3(
				m_terrainGrid[1, 1].transform.position.x + (i - 1) * m_landScapeSize, m_terrainGrid[1, 1].transform.position.y,
				m_terrainGrid[1, 1].transform.position.z + (j - 1) * m_landScapeSize);

				m_terrainGrid[i, j].treeDistance = m_treeDistance;
				m_terrainGrid[i, j].treeBillboardDistance = m_treeBillboardDistance;
				m_terrainGrid[i, j].treeCrossFadeLength = m_treeCrossFadeLength;
				m_terrainGrid[i, j].treeMaximumFullLODCount = m_treeMaximumFullLODCount;

				m_terrainGrid[i, j].detailObjectDensity = m_detailObjectDensity;
				m_terrainGrid[i, j].detailObjectDistance = m_detailObjectDistance;

				m_terrainGrid[i, j].GetComponent<Collider>().enabled = false;
				m_terrainGrid[i, j].basemapDistance = 4000;
				//m_terrainGrid[i, j].castShadows = false;

				// m_terrainGrid[i, j].terrainData.wavingGrassAmount = 1000;

				string xName = (curGlobalIndexX + i - 1).ToString(); // name will be used for identifying the correct entry in landmamassDictionary
				string zName = (curGlobalIndexZ + j - 1).ToString();
				m_terrainGrid[i, j].name = xName + "_new_" + zName;

				conversionFromWorldXtoSceneX = InfiniteLandscape.m_landScapeSize * (curGlobalIndexX); 
				conversionFromWorldZtoSceneZ = InfiniteLandscape.m_landScapeSize * (curGlobalIndexZ);

				// Test: Add castle to first Area:
				StoreCordinatesForCastle(curGlobalIndexX + i - 1, curGlobalIndexZ + j - 1, new Vector3(0, 500, 0));

				PatchManager.AddTerrainInfo(curGlobalIndexX + i - 1, curGlobalIndexZ + j - 1, m_terrainGrid[i, j], m_terrainGrid[i, j].transform.position);
				
			}
		}


		UnityEngine.Debug.Log("InfiniteTerrain OnEnable Added Terrain infos to patch managers -> " + stopWatch.Elapsed.Seconds);

		PatchManager.MakePatches();

		UnityEngine.Debug.Log("InfiniteTerrain OnEnable Made patches -> " + stopWatch.Elapsed.Seconds);

		int patchCount = PatchManager.patchQueue.Count;

		UnityEngine.Debug.Log("PatchManager.patchQueue.Count:  " + patchCount);


		for (int i = 0; i < patchCount; i++)
			PatchManager.patchQueue.Dequeue().ExecutePatch();

		m_terrainGrid[1, 1].transform.GetComponent<TerrainCollider>().enabled = true;
		m_terrainGrid[1, 1].Flush();

		//UnityEngine.Debug.Log("InfiniteTerrain Executed Patches -> " + stopWatch.Elapsed.Seconds);

		PlayerObject.gameObject.transform.position = new Vector3(InfiniteLandscape.initialPlayerPositionX, 500, InfiniteLandscape.initialPlayerPositionZ);
		PlayerObject.gameObject.SetActive(true);
		

		// Then Add rest:

		
		for (int i = 0; i < dim; i++)
		{
			for (int j = 0; j < dim; j++)
			{
				if (!(i == 1 && j == 1)) // dont do center terrain again
				{
					m_terrainGrid[i, j].gameObject.AddComponent<TerrainScript>();

					m_terrainGrid[i, j].transform.parent = gameObject.transform;

					m_terrainGrid[i, j].transform.position = new Vector3(
					m_terrainGrid[1, 1].transform.position.x + (i - 1) * m_landScapeSize, m_terrainGrid[1, 1].transform.position.y,
					m_terrainGrid[1, 1].transform.position.z + (j - 1) * m_landScapeSize);

					m_terrainGrid[i, j].treeDistance = m_treeDistance;
					m_terrainGrid[i, j].treeBillboardDistance = m_treeBillboardDistance;
					m_terrainGrid[i, j].treeCrossFadeLength = m_treeCrossFadeLength;
					m_terrainGrid[i, j].treeMaximumFullLODCount = m_treeMaximumFullLODCount;

					m_terrainGrid[i, j].detailObjectDensity = m_detailObjectDensity;
					m_terrainGrid[i, j].detailObjectDistance = m_detailObjectDistance;

					m_terrainGrid[i, j].GetComponent<Collider>().enabled = false;
					m_terrainGrid[i, j].basemapDistance = 4000;
					m_terrainGrid[i, j].castShadows = false;

					// m_terrainGrid[i, j].terrainData.wavingGrassAmount = 1000;

					string xName = (curGlobalIndexX + i - 1).ToString(); // name will be used for identifying the correct entry in landmamassDictionary
					string zName = (curGlobalIndexZ + j - 1).ToString();
					m_terrainGrid[i, j].name = xName + "_new_" + zName;

					float worldX = m_terrainGrid[i, j].transform.position.x;
					float worldZ = m_terrainGrid[i, j].transform.position.z;


					StoreCordinatesForCastle(curGlobalIndexX + i - 1, curGlobalIndexZ + j - 1, new Vector3(worldX, 500, worldZ));

					PatchManager.AddTerrainInfo(curGlobalIndexX + i - 1, curGlobalIndexZ + j - 1, m_terrainGrid[i, j], m_terrainGrid[i, j].transform.position);
				}
			}
		}
		PatchManager.MakePatches();

		UpdateIndexes();

		UnityEngine.Debug.Log("InfiniteTerrain Updated Indexes -> " + stopWatch.Elapsed.Seconds);

		UpdateTerrainNeighbors();
		UnityEngine.Debug.Log("InfiniteTerrain Updated terrainNeighbors -> " + stopWatch.Elapsed.Seconds);


		

		StartCoroutine(FlushTerrain(false));
		terrainIsFlushed = true;

		PlayerObject.gameObject.SetActive(true);
		SetOnGround(PlayerObject);

		//StartCastleCreation();

		FlattenArea(1000, 1000, 100, 500);

	}

	void FlattenArea (int xCoord, int yCoord , int size, int height)
	{
			// todo
	} 



	public void AddTerrainLayer(Terrain terrain, TerrainLayer terrainLayer)
	{
		
		// get the current array of TerrainLayers
		TerrainLayer[] oldLayers = terrain.terrainData.terrainLayers;

		// check to see that you are not adding a duplicate TerrainLayer
		for (int i = 0; i < oldLayers.Length; ++i)
		{
			if (oldLayers[i] == terrainLayer) return;
		}
		
		TerrainLayer[] newLayers = new TerrainLayer[oldLayers.Length + 1];

		// copy old array into new array
		Array.Copy(oldLayers, 0, newLayers, 0, oldLayers.Length);

		// add new TerrainLayer to the new array
		newLayers[oldLayers.Length] = terrainLayer;
		terrain.terrainData.terrainLayers = newLayers;
	}

	public void FillTerrainLayer(Terrain terrain)
	{
		// add the terrain layer to the terrain before filling

		for (int i = 0; i < biome.TerrainTextures.Length; i++)
		{
			TerrainLayer terrainLayer = new TerrainLayer();
			AddTerrainLayer(terrain, terrainLayer);
			terrainLayer.name = "TerrainLayer_" + i;
			terrainLayer.diffuseTexture = biome.TerrainTextures[i];
			terrainLayer.normalMapTexture = biome.TerrainNormalTextures[i];
			
			terrainLayer.specular = biome.TerrainSpecularColor[i];

			terrainLayer.tileSize = new Vector2 (biome.TerrainTileSizes[i], biome.TerrainTileSizes[i]);
		}
	}

	void UpdateTerrainNeighbors()
    {
        int iC = curCyclicIndexX;           int jC = curCyclicIndexZ;
        int iP = PreviousCyclicIndex(iC);   int jP = PreviousCyclicIndex(jC);
        int iN = NextCyclicIndex(iC);       int jN = NextCyclicIndex(jC);

        m_terrainGrid[iP, jP].SetNeighbors(null, m_terrainGrid[iP, jC], m_terrainGrid[iC, jP], null);
        m_terrainGrid[iC, jP].SetNeighbors(m_terrainGrid[iP, jP], m_terrainGrid[iC, jC], m_terrainGrid[iN, jP], null);
        m_terrainGrid[iN, jP].SetNeighbors(m_terrainGrid[iC, jP], m_terrainGrid[iN, jC], null, null);
        m_terrainGrid[iP, jC].SetNeighbors(null, m_terrainGrid[iP, jN], m_terrainGrid[iC, jC], m_terrainGrid[iP, jP]);
        m_terrainGrid[iC, jC].SetNeighbors(m_terrainGrid[iP, jC], m_terrainGrid[iC, jN], m_terrainGrid[iN, jC], m_terrainGrid[iC, jP]);
        m_terrainGrid[iN, jC].SetNeighbors(m_terrainGrid[iC, jC], m_terrainGrid[iN, jN], null, m_terrainGrid[iN, jP]);
        m_terrainGrid[iP, jN].SetNeighbors(null, null, m_terrainGrid[iC, jN], m_terrainGrid[iP, jC]);
        m_terrainGrid[iC, jN].SetNeighbors(m_terrainGrid[iP, jN], null, m_terrainGrid[iN, jN], m_terrainGrid[iC, jC]);
        m_terrainGrid[iN, jN].SetNeighbors(m_terrainGrid[iC, jN], null, null, m_terrainGrid[iN, jC]);
    }

    private int NextCyclicIndex(int i)
    {
        if (i < 0 || i > dim - 1)
			UnityEngine.Debug.LogError("index outside dim");
        return (i + 1) % dim;
    }

    private int PreviousCyclicIndex(int i)
    {
        if (i < 0 || i > dim - 1)
			UnityEngine.Debug.LogError("index outside dim");
        return i == 0 ? dim - 1 : (i-1) % dim;
    }

	private void UpdateTerrainPositions()
	{
		UnityEngine.Debug.Log("UpdateTerrainPositions, prevX /curX " + prevGlobalIndexX + " / " + curGlobalIndexX + "prevZ / curZ " + prevGlobalIndexZ + " / " + curGlobalIndexZ);
		if (curGlobalIndexZ != prevGlobalIndexZ && curGlobalIndexX != prevGlobalIndexX)
		{
			int z; int z0; int deletionZ;
			if (curGlobalIndexZ > prevGlobalIndexZ)
			{
				z0 = curGlobalIndexZ + 1;
				z = PreviousCyclicIndex(prevCyclicIndexZ);
				deletionZ = prevGlobalIndexZ - 1;
			}
			else
			{
				z0 = curGlobalIndexZ - 1;
				z = NextCyclicIndex(prevCyclicIndexZ);
				deletionZ = prevGlobalIndexZ + 1;
			}

			int[] listX = { PreviousCyclicIndex(prevCyclicIndexX), prevCyclicIndexX, NextCyclicIndex(prevCyclicIndexX) };
			for (int i = 1; i < dim; i++)
			{
				Vector3 newPos = new Vector3(
				m_terrainGrid[prevCyclicIndexX, curCyclicIndexZ].transform.position.x + (i - 1) * m_landScapeSize,
				m_terrainGrid[prevCyclicIndexX, curCyclicIndexZ].transform.position.y,
				m_terrainGrid[prevCyclicIndexX, curCyclicIndexZ].transform.position.z + (curGlobalIndexZ - prevGlobalIndexZ) * m_landScapeSize);

				PatchManager.AddTerrainInfo(prevGlobalIndexX + i - 1, z0, m_terrainGrid[listX[i], z], newPos);
				int mapX = prevGlobalIndexX + i - 1;
				int mapZ = z0;
				StoreCordinatesForCastle(mapX, mapZ, newPos);
				RemoveArtiFactsRelatedToTerrain( curGlobalIndexX + i - 1,deletionZ);
			}
			int x; int x0; int deletionX;
			if (curGlobalIndexX > prevGlobalIndexX)
			{
				x0 = curGlobalIndexX + 1;
				x = PreviousCyclicIndex(prevCyclicIndexX);
				deletionX = prevGlobalIndexX - 1;
			}
			else
			{
				x0 = curGlobalIndexX - 1;
				x = NextCyclicIndex(prevCyclicIndexX);
				deletionX = prevGlobalIndexX + 1;
			}

			int[] listZ = { PreviousCyclicIndex(curCyclicIndexZ), curCyclicIndexZ, NextCyclicIndex(curCyclicIndexZ) };
			for (int i = 0; i < dim; i++)
			{
				Vector3 newPos = new Vector3(
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.x + (curGlobalIndexX - prevGlobalIndexX) * m_landScapeSize,
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.y,
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.z + (i - 1) * m_landScapeSize);

				PatchManager.AddTerrainInfo(x0, curGlobalIndexZ + i - 1, m_terrainGrid[x, listZ[i]], newPos);
				int mapX = x0;
				int mapZ = curGlobalIndexZ + i - 1;
				StoreCordinatesForCastle(mapX, mapZ, newPos);
				RemoveArtiFactsRelatedToTerrain(deletionX, curGlobalIndexZ + i - 1);
			}
		}
		else if (curGlobalIndexZ != prevGlobalIndexZ)
		{
			int z; int z0; int deletionZ;
			if (curGlobalIndexZ > prevGlobalIndexZ)
			{
				z0 = curGlobalIndexZ + 1;
				z = PreviousCyclicIndex(prevCyclicIndexZ);
				deletionZ = prevGlobalIndexZ - 1;
			}
			else
			{
				z0 = curGlobalIndexZ - 1;
				z = NextCyclicIndex(prevCyclicIndexZ);
				deletionZ = prevGlobalIndexZ + 1;
			}
			int[] listX = { PreviousCyclicIndex(prevCyclicIndexX), prevCyclicIndexX, NextCyclicIndex(prevCyclicIndexX) };
			for (int i = 0; i < dim; i++)
			{
				Vector3 newPos = new Vector3(
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.x + (i - 1) * m_landScapeSize,
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.y,
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.z + (curGlobalIndexZ - prevGlobalIndexZ) * m_landScapeSize);

				PatchManager.AddTerrainInfo(curGlobalIndexX + i - 1, z0, m_terrainGrid[listX[i], z], newPos);

				int mapX = curGlobalIndexX + i - 1;
				int mapZ = z0;
				StoreCordinatesForCastle(mapX, mapZ, newPos);
				RemoveArtiFactsRelatedToTerrain( curGlobalIndexX + i - 1, deletionZ);
			}
		}
		else if (curGlobalIndexX != prevGlobalIndexX)
		{
			int x; int x0; int deletionX;
			if (curGlobalIndexX > prevGlobalIndexX)
			{
				x0 = curGlobalIndexX + 1;
				x = PreviousCyclicIndex(prevCyclicIndexX);
				deletionX = prevGlobalIndexX - 1;
			}
			else
			{
				x0 = curGlobalIndexX - 1;
				x = NextCyclicIndex(prevCyclicIndexX);
				deletionX = prevGlobalIndexX + 1;
			}

			int[] listZ = { PreviousCyclicIndex(curCyclicIndexZ), curCyclicIndexZ, NextCyclicIndex(curCyclicIndexZ) };
			for (int i = 0; i < dim; i++)
			{
				Vector3 newPos = new Vector3(
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.x + (curGlobalIndexX - prevGlobalIndexX) * m_landScapeSize,
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.y,
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.z + (i - 1) * m_landScapeSize);

				PatchManager.AddTerrainInfo(x0, curGlobalIndexZ + i - 1, m_terrainGrid[x, listZ[i]], newPos);
				int mapX = x0;
				int mapZ = curGlobalIndexZ + i - 1;
				StoreCordinatesForCastle(mapX, mapZ, newPos);

				RemoveArtiFactsRelatedToTerrain(deletionX, curGlobalIndexZ + i - 1);
			}
		}
		PatchManager.MakePatches();
	}


	private List<GameObject> CastleRootPrefabs = new List<GameObject>();

	private void ClearItemsRelatedToTerrain(string terrainName)
	{
		// test: delete same named castle items:


	}

	private void StartCastleCreation()
	{
		foreach(var item in CastleBuildList)
		{
			
			castleCreator.CreateCastle(item);
		}
		CastleBuildList.Clear();
	}

	private void StartCastleCreationForTerrain(string terrainName)
	{
		//UnityEngine.Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>-----------------------> StartCastleCreationForTerrain " + terrainName);

		foreach (var item in CastleBuildList)
		{
			if(item.terrainName == terrainName)
			{
				//UnityEngine.Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>-----------------------> StartCastleCreationForTerrain  FOUND");
				castleCreator.CreateCastle(item);
			}

		}
		//CastleBuildList.Clear();
		
	}





	public static AreaData GetAreaData(int mapX, int mapZ)
	{
		string key = mapX + "_" + mapZ;
		if (AreaDict.ContainsKey(key))
		{
			return AreaDict[key];
		}
		return null;
	}

	void StoreCordinatesForCastle(int mapX, int mapZ, Vector3 pos)
	{
		string key = mapX + "_" + mapZ;

		// TODO if castle data is already created, dont recreate it, just pass the old data to build list

		var castleData = new CastleData();
		castleData.terrainName = key;
		castleData.mapX = mapX;
		castleData.mapZ = mapZ;
		castleData.coordX = pos.x + 1500; // position in center of terrain tile
		castleData.coordZ = pos.z + 1500;
		castleData.size = 5;

		castleData.coordXMin = castleData.coordX;
		castleData.coordZMin = castleData.coordZ;

		castleData.coordXMax = castleData.coordX + (castleData.size * 100);
		castleData.coordZMax = castleData.coordZ + (castleData.size * 100);

		if (AreaDict.ContainsKey(key))
		{
			UnityEngine.Debug.Log("AreaDict had the area key so We  can store the castleData!!");

			UnityEngine.Debug.Log("Storing CastleData of " + key + " : " + pos);
			AreaDict[key].castleData = castleData;
		}
		else
		{
			UnityEngine.Debug.Log("AreaDict did not have the area key so We  can Not store the castleData!!");
		}
	
	
		CastleBuildList.Add(castleData);
	}

	

	void RemoveArtiFactsRelatedToTerrain(int mapX, int mapZ)
	{
		
		UnityEngine.Debug.LogWarning ("RemoveArtiFactsRelatedToTerrain " + mapX + "_" + mapZ);

		string key = mapX + "_" + mapZ;
		if (AreaDict.ContainsKey(key))
		{
			// Artifact handling
			if (AreaDict[key].artefactList != null)
			{
				foreach (var info in AreaDict[key].artefactList)
				{
					UnityEngine.Debug.LogWarning("About to destroy  " + info.name);

					if(info.instantiatedprefab != null)
					{
						UnityEngine.Debug.LogWarning(" Destroying " + info.instantiatedprefab.name);
						Destroy(info.instantiatedprefab);
					}
					else
					{
						UnityEngine.Debug.LogWarning(" Destroying " + info.name + " - not found, not destroyed");
					}
				}
			}


			// Castle handling; TODO make this use new system
			if(AreaDict[key].castleData != null)
			{
					GameObject root = AreaDict[key].castleData.rootGameObject;
					if (root != null)
					{
					Destroy(root);//.transform.position = new Vector3(0, 0, 0);
				}
					else
					{
					UnityEngine.Debug.Log(" - root was null");
				}
						
			}
			else
			{
				UnityEngine.Debug.Log(" - CastleData  not found");
			}	
		}
		else
		{
			UnityEngine.Debug.Log(" - Did not find key " + key);
		}
	} 


    IEnumerator CountdownForPatch()
    {
        patchIsFilling = true;
        yield return new WaitForEndOfFrame();
        patchIsFilling = false;
    }

	IEnumerator FlushTerrain(bool flushAll = true)
	{

		UnityEngine.Debug.Log("InfiniteTerrain Started terrain flushing -> " + stopWatch.Elapsed.Seconds);

		for (int i = 0; i < dim; i++)
		{
			for (int j = 0; j < dim; j++)
			{
				
				if( flushAll|| (i==1 && j ==1) ) // flush all or only center tile
				{
					UnityEngine.Debug.Log("FlushTerrain, " + i + " " + j);
					m_terrainGrid[i, j].transform.GetComponent<TerrainCollider>().enabled = true;
					m_terrainGrid[i, j].Flush();
					yield return new WaitForEndOfFrame();

					string terrainName = m_terrainGrid[i, j].name;

					//UnityEngine.Debug.Log(">>>>> TERRAIN NAME SETTING  i " + i + " j " + j + " m_terrainGrid[i, j].name " + m_terrainGrid[i, j].name);

					if (createCastle)
						StartCastleCreationForTerrain(terrainName);


					// Put vital info into terrainscript, like pos in world
					m_terrainGrid[i, j].GetComponent<TerrainScript>().SetImportantValues(terrainName, i, j);

				}
				
			}
		}
		//StartCastleCreation();

		UnityEngine.Debug.Log("InfiniteTerrain Finished terrain flushing -> " + stopWatch.Elapsed.Seconds);

		
	}

    float StartTime;
    float oneUpdateTime;
    bool updateRound;
    int updatecounter = 0;
    float biggestUpdateTime = 0f;


	bool savetestDone = false;

	protected override void Update()
    {
        base.Update();

		//-------Just debugging--------
		if(updateRound)
        {
            // previous was updateRound
            float executionTime = Time.time - StartTime;
			//UnityEngine.Debug.Log(updatecounter + ": ----------------------------------------> Execution time " + executionTime);
			updateRound = false;
        }
        updateRound = updateLandscape;
        StartTime = Time.deltaTime;
        //---------------

        if (updateLandscape == true)
        {
            m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].GetComponent<Collider>().enabled = true;        //Slow operation
																											
			// for displaying data in UI:
			Terrain current = m_terrainGrid[curCyclicIndexX, curCyclicIndexZ];
			UnityEngine.Debug.Log("-------> entering " + current.name);

			// repositioning the navmesh
			if (AreaDict.ContainsKey(current.name))
			{
				var  castleData = AreaDict[current.name].castleData;
				if(castleData != null)
				{
					navMeshBuilder.transform.position = castleData.NavMeshPos; // needs proper calculating of pos and size 
				}
			}

			//terrainName.text = current.name;
			int massType = InfiniteTerrain.GetOrAssignLandMassTypes(curCyclicIndexX,curCyclicIndexZ,current.name);

			/*
			hillsValue.text = (massType & 1) > 0 ? "yes": "no";
			mountainsValue.text = (massType & 2) > 0 ? "yes": "no";
			ridgedMountainsValue.text = (massType & 4) > 0 ? "yes" : "no";
			plainsValue.text = (massType & 8) > 0 ? "yes" : "no";
			*/
			
			m_terrainGrid[prevCyclicIndexX, prevCyclicIndexZ].GetComponent<Collider>().enabled = false;

            UpdateTerrainNeighbors();
            UpdateTerrainPositions();
        }

        if (PatchManager.patchQueue.Count != 0)
        {
            terrainIsFlushed = false;
            if (patchIsFilling == false)
            {
                patchToBeFilled = PatchManager.patchQueue.Dequeue();
                StartCoroutine(CountdownForPatch());
            }
            if (patchToBeFilled != null)
            {

                float execTime = Time.time - oneUpdateTime;

				string patchType = "none";

				if(patchToBeFilled is TerrainPatch)
					patchType = "terrainPatch";

				if (patchToBeFilled is SplatDetailPatch)
					patchType = "SplatDetail";

				if (patchToBeFilled is TreePatch)
					patchType = "TreePatch";

                if(execTime > 1f)
                {
					UnityEngine.Debug.LogWarning(Time.time +  "------------------> exec time " + execTime +  " for "  + patchType); // error to get red color in console, not actual error
                    biggestUpdateTime = execTime;
                }

				//for debugging
                oneUpdateTime = Time.time;
                
				patchToBeFilled.ExecutePatch();
                patchToBeFilled = null;
            }
        }
        else if (PatchManager.patchQueue.Count == 0 && terrainIsFlushed == false)
        {
            StartCoroutine(FlushTerrain(true));
            terrainIsFlushed = true;
        }
    }

//Generates sine "bell shape" falloff table, used for blending  terrain height values for each landmass type
public static float[,] GenerateFalloffTable()
     {
        int size = 513; // assuming height is the same
        int halfSize = 257; // Actually half the size + "middle pixel" also
        float n256 = size *0.5f;
        float[,]  table = new float[513,513]; 
        Vector2 center = new Vector2(size / 2f, size / 2f);
        List<int> errorValues = new List<int>();

		// we only iterate one quarter of the area, and "mirror" the results to rest of the area:
		// its faster, and we can be absolutely sure that when fallofftable is used, left and right (and top and bottom) 
		// values are exactly same and there will be no visible seams  
        for (int y = 0; y < halfSize; y++)
        {
            for (int x = 0; x < halfSize; x++)
            {
                float DistanceFromCenter = Vector2.Distance(center, new Vector2(x, y));
                float currentAlpha = 0; // value will act as "alpha mask" when blending terrain heights

                if (DistanceFromCenter > 513 / 2f)
                {
                    currentAlpha = 0f;
                }
                else
                {
                    float normalized = 2 * ((DistanceFromCenter / n256) * (3.1415926f * 0.500f));
                    currentAlpha = (Mathf.Sin(1.5f - normalized) / 2f) + 0.5f;
                }

                if (x <= 257 && y <= 257  && x >=10 && y >= 10)
                {

                    table[x, y] = currentAlpha;
                    table[size - x, y] = currentAlpha;
                    table[x, size - y] = currentAlpha;
                    table[size - x, size - y] = currentAlpha;
                }
                else
                {
                   //debugging:
                   //errorValues.Add(x);
                }
            }
        }

        foreach(int error in errorValues)
			UnityEngine.Debug.Log("x error: " + error);
        return table;    
    }


	// Landmass types are assigned randomly, but it would make no sense if landmass types once set would change then the area is left and re-entered, so
	// values are stored. To make world persistent, these could be saved to a file.
	// 
	// If value for landmasses has already been decided, it is returned. If not, random value is assigned to the new dictionary entry. 

	static int testCounter = 0;

	public static int GetOrAssignLandMassTypes(int xVal, int zVal, string key, int presetValue  = -1000)
	{

		if(m_mapNoise == null)
		m_mapNoise = new PerlinNoise(InfiniteLandscape.RandomSeed);
	

		if (AreaDict.ContainsKey(key))
		{
			return AreaDict[key].landMassValue;
		}
		else if (presetValue == -1000)
		{

			int octNum = 3;
			float frequency= 10000;
			float amplitude = 10.0f;


			float lowEnd = 100000;
			float highEnd = -100000;

			float lowEndMapped = 100000;
			float highEndMapped = -100000;
			/*
			for (int i = 0; i < 5000; i++)
			{
				float testval = m_mapNoise.FractalNoise2D(i, i + i, octNum, frequency, amplitude) +amplitude;

				int testvalMapped = MapFloatValuesToInt(testval, 1, 20, 0, 15);

				if (testval > highEnd)
					highEnd = testval;

				if (testval < lowEnd)
					lowEnd = testval;


				if (testvalMapped > highEndMapped)
					highEndMapped = testvalMapped;

				if (testvalMapped < lowEndMapped)
					lowEndMapped = testvalMapped;

			}*/
			//UnityEngine.Debug.Log("----------------------> testval  Low <--> high:    " + lowEnd + " <-> " + highEnd);
			//UnityEngine.Debug.Log("----------------> testvalmapped  Low <--> high:    " + lowEndMapped + " <-> " + highEndMapped);

			// using persistent value tied to map coordinate:	
			float val = m_mapNoise.FractalNoise2D(xVal*8746, zVal*9387, octNum, frequency, amplitude) + amplitude;

			// Note:	
			//int value = Random.Range(1, 17); // randomly combine all types with even weights - gives more square and blocky coastline
			//int value =  Random.Range(1, 9) + 8; // always have plains - gives more natural coastline

			int mapped = MapFloatValuesToInt(val, 1.28f, 18.7f, 1, 9) + 8;
			//UnityEngine.Debug.Log("--------------------------------------------------------> Original " + val + " mapped " + mapped);
			//UnityEngine.Debug.Log("adding key >>> " + key + " value: " + mapped);


			//  use just one type for testing things:
			// mapped = 0; // flat seabed
			// mapped = 1; // small flat islands -  "fjords" tops near sealevel
			// mapped = 2; // archipelago with rounded mountains
			// mapped = 3;   // 1+2 so islands of fjordy mountains
			// mapped = 4; // spiky mountains and water
			// mapped = 8;   // plains -very flat, some lakes

			var area = new AreaData();
			area.landMassValue = mapped;
			area.mapX = xVal;
			area.mapZ = zVal;


			AreaDict.Add(key, area);

			/*
			if (testCounter < 50)
			{
				testCounter++;
				UnityEngine.Debug.Log("Adding to AreaDict: " + key + "  " + mapped);
			}
			*/
			
			return mapped;

		}
		else
		{
			// use presetvalue
			if (! AreaDict.ContainsKey(key))
			{
				var area = new AreaData();
				area.landMassValue = presetValue;
				AreaDict.Add(key, area);
				
			}
			return presetValue;
		}


		int MapFloatValuesToInt(float s, float a1, float a2, float b1, float b2)
		{
			return  (int)Math.Floor(b1 + (s - a1) * (b2 - b1) / (a2 - a1));
		}

	}
	Vector3  SetOnGround(GameObject myObject)
	{
		Vector3 position = myObject.transform.position;

		RaycastHit hit;
	
		if (Physics.Raycast(position, transform.TransformDirection(Vector3.down), out hit, 5000))
		{
		
			return new Vector3 (myObject.transform.position.x, hit.point.y + 1, myObject.transform.position.z);
		}
		return position;
	}
}

