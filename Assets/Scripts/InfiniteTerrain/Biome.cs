using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cartographer/Biome", order = 1)]

public class Biome : ScriptableObject
{

	public GameObject[] Trees;

	public const int TerrainTextureAmount = 5;
	public Texture2D[] TerrainTextures = new Texture2D[TerrainTextureAmount];
	public float[] TerrainTileSizes;
	public Texture2D[] TerrainNormalTextures = new Texture2D[TerrainTextureAmount];
	public Color[] TerrainSpecularColor = new Color[TerrainTextureAmount];


	public const int numOfDetailPrototypes = 6;
	public Texture2D[] detailTexture = new Texture2D[numOfDetailPrototypes];
	public GameObject[] detailMesh = new GameObject[numOfDetailPrototypes];

	public int testInt = 0;

	public int PrintTestInt()
	{
		Debug.Log("TESTINT " + testInt);
		return testInt;
	}


}

