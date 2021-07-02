using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldParameters : MonoBehaviour
{
	public int mapSeed;

	public float steepnessPower = 10;

	public float magicNumber = 24;

	public InputField seedInput;
	public bool OnlyMiddleTile;
 
	public static Texture2D worldMapTexture;
	public static float[,] worldMapHeights;

	public Texture2D slopeTexture;

	public static Texture2D AllTerrainsPathsTexture;

	public GameObject mapImage; //This is the UI reference
	public GameObject SlopeMapImage; //This is the UI reference

	public GameObject AllPathsImage; //This is the UI reference

	float[,] heightMap = new float[512, 512];
	float[,] steepnesstMap = new float[512, 512];


	public Gradient gradient;
	public Gradient slopeGradient;


	public int worldSizeX = 10;
	public int worldSizeY = 10;

	public static int WorldSizeX = 10;
	public static int WorldSizeY = 10;


	public Curves curves;

	public static Curves staticCurves;

	public static List<PathFind.Point> magentaPoints;

	public float[,] navigatonGrid = new float[512, 512];

	public static List<List<PathFind.Point>> AllPathsGlobalCoordinated;

	public static bool createRoads = false;
	public static bool renderTrees = true;
	public static bool renderDetails = true;



	private void Awake()
	{
		staticCurves = curves;
		magentaPoints = new List<PathFind.Point>();
		AllPathsGlobalCoordinated = new List<List<PathFind.Point>>();

		InfiniteTerrain.fallOffTable = InfiniteTerrain.GenerateFalloffTable();

		worldMapTexture = new Texture2D(512, 512);
		worldMapHeights = new float[512, 512];

		worldMapTexture.name = "Procedural Texture";
		mapImage.GetComponent<RawImage>().texture = worldMapTexture;

		slopeTexture = new Texture2D(512, 512);
		slopeTexture.name = "Slpe Texture";
		//SlopeMapImage.GetComponent<RawImage>().texture = slopeTexture;

		AllTerrainsPathsTexture = new Texture2D(512, 512);
		AllPathsImage.GetComponent<RawImage>().texture = AllTerrainsPathsTexture;


	}


	public void UpdateMap()
	{

		AllPathsGlobalCoordinated = new List<List<PathFind.Point>>();
		AllTerrainsPathsTexture = new Texture2D(512, 512);

		InfiniteTerrain.m_mapNoise = null;

		InfiniteLandscape.RandomSeed = int.Parse(seedInput.text);
		InfiniteTerrain.AreaDict.Clear();

		Debug.Log("Update Map with seed value " + InfiniteLandscape.RandomSeed);

		WorldSizeX = worldSizeX;
		WorldSizeY = worldSizeY;

		int SworldSizeX = 	worldSizeX;
		int SworldSizeY = worldSizeY;


		for (int y = 0; y < SworldSizeY +1; y++)  
		{
			for (int x = 0; x < SworldSizeX +1; x++)
			{
				string key = x.ToString() + "_" + y.ToString();
				
				//sea around map 
				if( x == 0 || y == 0 || x >= SworldSizeX || y >= SworldSizeY)
				{
					InfiniteTerrain.GetOrAssignLandMassTypes(x, y, key, 0);
				}
				else
				if(x == 1 || y == 1 || x == SworldSizeX-1 || y == SworldSizeY-1)
				{
					if (x % 3 == 0 || y % 2 == 0 ) // making coast line less straight 
					{
						InfiniteTerrain.GetOrAssignLandMassTypes(x, y, key, 0);
					}  
					else
					{
						InfiniteTerrain.GetOrAssignLandMassTypes(x, y, key);
					}
						
				}
				else
				{
					InfiniteTerrain.GetOrAssignLandMassTypes(x, y, key);	
				}
								
			}
		}

		FillTexture();
	}

	float min = 1000;
	float max = -1000;




	private void FillTexture()
	{
		
		for (int y = 0; y < 512; y++)
		{
			for (int x = 0; x < 512; x++)
			{

				int magic2 = 4; // (int)(worldSizeX / 512); //if 3072 then this is  6;

				int worldCoordinateX = Mathf.RoundToInt(x * magic2 * Mathf.Abs(Mathf.Floor(worldSizeX)));   
				int worldCoordinateY = Mathf.RoundToInt(y * magic2 * Mathf.Abs(Mathf.Floor(worldSizeY)));


				float height = TerrainPatch.getHeightByWorldPosition(worldCoordinateX, worldCoordinateY, curves);
				float heightForNavigation = height;

				//if(x % 10 == 0 && y % 10 == 0)
				//	Debug.Log(">" + heightForNavigation);

				/*
				if(height > max)
					max = height;

				if (height < min)
					min = height;
				*/
				
				worldMapHeights[x, y] = height;

				if(x== 1 && y== 1 )
				{
					//Debug.Log("WATERLEVEL: " +  height);
				}

				float mappedHeight = Map(height, -0.2f, 0.5f, 0f, 1f);

				heightMap[x, y] = mappedHeight;

				float mappedNavigationHeight = Map(heightForNavigation, -0.2f, 0.5f, 0f, 1f);


				// AVOID WATER AND MOUNTAINS
				if (mappedNavigationHeight <= 0.35f || mappedNavigationHeight > 0.6f )
					mappedNavigationHeight = 2f;


				navigatonGrid[x, y] = mappedNavigationHeight;

				// this creates "Map Lines"
				if(worldCoordinateX % InfiniteLandscape.m_landScapeSize == 0 || worldCoordinateY % InfiniteLandscape.m_landScapeSize == 0)
					mappedHeight = 1;

				Color testColor = gradient.Evaluate(mappedHeight);

				worldMapTexture.SetPixel(x, y, testColor);			
			}
		}

		
		for (int y = 0; y < 511; y++)  /// NOTE 511!
		{
			for (int x = 0; x < 511; x++)
			{

				float steepnessValue = GetSteepness(heightMap, x, y);
				steepnesstMap[x, y] = steepnessValue*steepnessPower;

				Color testColor = slopeGradient.Evaluate(steepnessValue*steepnessPower);

				slopeTexture.SetPixel(x, y, testColor);

				navigatonGrid[x, y] = navigatonGrid[x, y] + (steepnessValue*steepnessPower*steepnessPower);
			}
		}


		Debug.Log("min  " + min + " max " + max);

		if(createRoads)
			MakePathTest();

		worldMapTexture.Apply();
		slopeTexture.Apply();
	}

	// https://gamedev.stackexchange.com/questions/89824/how-can-i-compute-a-steepness-value-for-height-map-cells
	float GetSteepness(float[,] heightmap, int x, int y)
	{
		float height = heightmap[x, y];

		// Compute the differentials by stepping over 1 in both directions.
		// TODO: Ensure these are inside the heightmap before sampling.
		float dx = heightmap[x + 1, y] - height;
		float dy = heightmap[x, y + 1] - height;

		// The "steepness" is the magnitude of the gradient vector
		// For a faster but not as accurate computation, you can just use abs(dx) + abs(dy)
		return Mathf.Sqrt(dx * dx + dy * dy);
	}


	public void MakePathTest()
	{
		int width = 512;
		int height = 512;

		PathFind.Grid grid = new PathFind.Grid(width, height, navigatonGrid);

		for (int y = 0; y < 512; y++)
		{
			for (int x = 0; x < 512; x++)
			{
				AllTerrainsPathsTexture.SetPixel(x, y, Color.black);
			}
		}

// corner to corner
		PathFind.Point _from = new PathFind.Point(1, 1);  // these are values of the "image" so for example 512x512 as base dimension
		PathFind.Point _to = new PathFind.Point(510,510);
		List<PathFind.Point> path = PathFind.Pathfinding.FindPath(grid, _from, _to);
		PaintPathToMap(path);

		List<PathFind.Point> tempPath = new List<PathFind.Point>();

		foreach (var item in path)
		{
			var tempItem = new PathFind.Point();
			tempItem.x = item.x * 6 * (int)Mathf.Abs(Mathf.Floor(worldSizeX)); 
			tempItem.y = item.y * 6 * (int)Mathf.Abs(Mathf.Floor(worldSizeX));
			tempPath.Add(tempItem);
		}

		AllPathsGlobalCoordinated.Add(tempPath);
		
		_from = new PathFind.Point(1, 510);
		_to = new PathFind.Point(510, 1);
		path = PathFind.Pathfinding.FindPath(grid, _from, _to);
		PaintPathToMap(path);

		tempPath = new List<PathFind.Point>();
		foreach (var item in path)
		{
			var tempItem = new PathFind.Point();
			tempItem.x = item.x * 6 * (int)Mathf.Abs(Mathf.Floor(worldSizeX));
			tempItem.y = item.y * 6 * (int)Mathf.Abs(Mathf.Floor(worldSizeX));
			tempPath.Add(tempItem);
		}

		AllPathsGlobalCoordinated.Add(tempPath);
		
/*
// middle cross

		_from = new PathFind.Point(10, 250);
		_to = new PathFind.Point(500, 250);
		path = PathFind.Pathfinding.FindPath(grid, _from, _to);
		PaintPathToMap(path);

		_from = new PathFind.Point(250, 10);
		_to = new PathFind.Point(250,500);
		path = PathFind.Pathfinding.FindPath(grid, _from, _to);
		PaintPathToMap(path);



// COASTAL ROUTES		
		_from = new PathFind.Point(10, 10);
		_to = new PathFind.Point(10, 500);
		path = PathFind.Pathfinding.FindPath(grid, _from, _to);
		PaintPathToMap(path);

		_from = new PathFind.Point(500, 10);
		_to = new PathFind.Point(500, 500);
		path = PathFind.Pathfinding.FindPath(grid, _from, _to);
		PaintPathToMap(path);

		_from = new PathFind.Point(10, 10);
		_to = new PathFind.Point(500, 10);
		path = PathFind.Pathfinding.FindPath(grid, _from, _to);
		PaintPathToMap(path);

		_from = new PathFind.Point(10, 500);
		_to = new PathFind.Point(500, 500);
		path = PathFind.Pathfinding.FindPath(grid, _from, _to);
		PaintPathToMap(path);
*/
		// Paint paths to texture2d then split the texture to smaller parts each size of one terrain tile
	} 

	public void PaintPathToMap(List<PathFind.Point> path)
	{
		AllPathsImage.GetComponent<RawImage>().texture = AllTerrainsPathsTexture;

		float previousPathMapRedValue = -1;
		Color previousWorldMapColor = Color.cyan;
		float previousWorldMapDept = 0;

		foreach(var point in path)
		{
			// TODO: see if there is path already, then use that knowledge to find crossroads spots, then 
			// use that to place towns 	

			// Paínt the engine side path texture

			Color currentPathColor = AllTerrainsPathsTexture.GetPixel(point.x, point.y);
			
			// no path yet, set to one path
			if(currentPathColor.r == 0 )
			{
				AllTerrainsPathsTexture.SetPixel(point.x, point.y, Color.white);
				worldMapTexture.SetPixel(point.x, point.y, Color.gray);
			}
			else if(previousPathMapRedValue == 0)
			{
				AllTerrainsPathsTexture.SetPixel(point.x, point.y, Color.white);
				worldMapTexture.SetPixel(point.x, point.y, Color.red);
			}
			else
			{
				AllTerrainsPathsTexture.SetPixel(point.x, point.y, Color.white);
				worldMapTexture.SetPixel(point.x, point.y, Color.green);
			}


			// find out the waterLine depth

			float coastLineMagicNumber = 0.028f;

			if (worldMapHeights[point.x, point.y] < 0.02f) // waterLevel - the value 0.02 is acquired by testing, no logic  behind it
			{
				worldMapTexture.SetPixel(point.x, point.y, Color.blue);
			}
			

			// Find seaside tiles
			if(worldMapHeights[point.x, point.y] < coastLineMagicNumber && previousWorldMapDept >= coastLineMagicNumber
			|| worldMapHeights[point.x, point.y] >= coastLineMagicNumber && previousWorldMapDept < coastLineMagicNumber
			)
			{
				Debug.Log("Setting to Magenta: " + point);
				worldMapTexture.SetPixel(point.x, point.y, Color.magenta);

				var tempPoint = new PathFind.Point();

				tempPoint.x = point.x * 6 * (int)Mathf.Abs(Mathf.Floor(worldSizeX)); // TODO remove magic number
				tempPoint.y = point.y * 6 * (int)Mathf.Abs(Mathf.Floor(worldSizeX));

				//Debug.Log("world coordinates: " + tempItem.x + "  " + tempItem.y);

				magentaPoints.Add(tempPoint);
			}


			AllTerrainsPathsTexture.SetPixel(point.x, point.y, Color.white);
			/*
			if(previousWorldMapColor.b > 0.6)
			{
				worldMapTexture.SetPixel(point.x, point.y, Color.blue);
			}
			*/
			previousPathMapRedValue = currentPathColor.r;
			previousWorldMapColor = worldMapTexture.GetPixel(point.x, point.y);
			previousWorldMapDept = worldMapHeights[point.x, point.y];

		}

		AllTerrainsPathsTexture.Apply();
	}

	public static Vector2 WorldCoordinateToSceneCoordinate(Vector2 WorldCoordinate)
	{

		// InfiniteLandscape.initialGlobalIndexX = (int)Mathf.Floor(selectedTerrain.x) - 1;

		Debug.Log("WorldCoordinateToSceneCoordinate WorldCoordinate" + WorldCoordinate.x +"  InfiniteLandscape.initialGlobalIndexX " + InfiniteLandscape.initialGlobalIndexX);
		
		// lets assume world coordinate is from 3/3 tile
		//then it would be 0 2048 4096 ->  4096

		Vector2 sceneCoordinate = new Vector2();
		var Xdifference = InfiniteLandscape.m_landScapeSize * (InfiniteLandscape.initialGlobalIndexX);
		var Zdifference = InfiniteLandscape.m_landScapeSize * (InfiniteLandscape.initialGlobalIndexZ);

		sceneCoordinate.x = WorldCoordinate.x - Xdifference;
		sceneCoordinate.y = WorldCoordinate.y - Zdifference;

		//sceneCoordinate.x = WorldCoordinate.x - ((InfiniteLandscape.initialGlobalIndexX+1) * InfiniteLandscape.m_landScapeSize);
		//sceneCoordinate.x = WorldCoordinate.x - ((InfiniteLandscape.initialGlobalIndexX+1) * InfiniteLandscape.m_landScapeSize);
		//sceneCoordinate.y = WorldCoordinate.y - ((InfiniteLandscape.initialGlobalIndexZ+1) * InfiniteLandscape.m_landScapeSize);
		Debug.Log(" --> sceneCoordinate.x " + sceneCoordinate.x);
		return sceneCoordinate;
	} 


	float Map(float s, float a1, float a2, float b1, float b2)
	{
		return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
	}

}
