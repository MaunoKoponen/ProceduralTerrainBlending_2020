using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

public static class PatchManager
{
    private static int terrainPatchRes = 96;
    private static int splatDetailPatchRes = 32;
    private static int treePatchRes = 8;
	private static int tempCounter = 0;

	public class TerrainInfo
    {
		public bool HasHills; // temp solution, need betterr way to query landmass types of a terrain

		public Vector3 newPosition;
		public int globalX;
		public int globalZ;
		public Terrain terrain;

		public Texture2D PathImage;

		public int landmassTypes;  //  1,2,4,8... 
		public Biome biome;

		public TerrainInfo(int globX, int globZ, Terrain ter, Vector3 newPos)
        {
            newPosition = newPos;
            globalX = globX;
            globalZ = globZ;
            terrain = ter;
			string key = globalX.ToString() + "_" + globalZ.ToString();
			landmassTypes = InfiniteTerrain.GetOrAssignLandMassTypes(globalX,globalZ,key);
			
			SetParameters();
		}


		private void SetParameters()
		{
				if (((landmassTypes & 1) > 0)) // hills
				HasHills = true;


		}
    }

    public static Queue<IPatch> patchQueue = new Queue<IPatch>();
    private static List<TerrainInfo> patchList = new List<TerrainInfo>();

    public static void AddTerrainInfo(int globX, int globZ, Terrain terrain, Vector3 pos)
    {

		string xName = globX.ToString();
		string zName = globZ.ToString();
		terrain.name = xName + "_" + zName;

		TerrainInfo ti = new TerrainInfo(globX, globZ, terrain, pos);

		if(WorldParameters.startedFromMenu)
			GeneratePathImage(ti);

		//Debug.Log("Adding new terrainInfo to PatchList: globX: " + globX + " globZ: " + globZ);
		patchList.Add(ti);
		
	}


	private static void GeneratePathImage(TerrainInfo ti)
	{
		// generate path image for this terrain from allterrainsPath image
		var allterrainsPathsPic = WorldParameters.AllTerrainsPathsTexture;

		var worldSizeX = WorldParameters.WorldSizeX;
		var worldSizeY = WorldParameters.WorldSizeY;


		Debug.Log(" DDD Creating thisTerrainsAreaInAllTerrainsPic from " + ti.globalX + " " + ti.globalZ + " " + 512 / worldSizeX + "  " + 512 / worldSizeX);

		int testVal = 512;

		Rect thisTerrainsAreaInAllTerrainsPic = new Rect(ti.globalX* (testVal / worldSizeX), ti.globalZ* (testVal / worldSizeY), testVal / worldSizeX, testVal / worldSizeY); // 0,0 temp testvalue

		Texture2D testTex = ExtractFromTexture(allterrainsPathsPic, thisTerrainsAreaInAllTerrainsPic);

		/*
		for (int y = 0; y < 64; y++)
		{
			for (int x = 0; x < 64; x++)
			{

				
				if(x > 20 &&  x < 24 || y > 50 && y < 52 )
				{
					testTex.SetPixel(x, y, Color.white);
				}
				else
				{
					testTex.SetPixel(x, y, Color.black);
				}
						
			}
		}
		*/
		
		testTex.Apply();
		Texture2D testTex2 = Resize(testTex, 512, 512); // given size depends on alphamap size: public const int m_alphaMapSize = (InfiniteTerrain.m_heightMapSize - 1) * 1;

		ti.PathImage = testTex2;
		ti.PathImage.Apply();
		
	}



	private static Texture2D ExtractFromTexture(Texture2D sourceTex, Rect sourceRect)
	{

        
		Debug.Log("ExtractFromTexture params: " + sourceTex + "  " + sourceRect);

		int x = Mathf.FloorToInt(sourceRect.x);
		int y = Mathf.FloorToInt(sourceRect.y);
		int width = Mathf.FloorToInt(sourceRect.width);
		int height = Mathf.FloorToInt(sourceRect.height);

		Color[] pix = sourceTex.GetPixels(x, y, width, height);
		Texture2D destTex = new Texture2D(width, height);

		destTex.SetPixels(pix);
		return destTex;
	}


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



    public static void MakePatches()
    {

		// TODO Calculate here the area that needs to be clear of trees etc, and pass xmin xmax, zmin z max to Patchs

		foreach (TerrainInfo tI in patchList)
        {
            for (int i = 0; i < terrainPatchRes; i++)
			{
				patchQueue.Enqueue(new TerrainPatch(tI.globalX, tI.globalZ, tI.terrain,
					InfiniteTerrain.m_heightMapSize * i / terrainPatchRes, InfiniteTerrain.m_heightMapSize * (i + 1) / terrainPatchRes, tI.newPosition, tI));
			}
                
        }
        
        foreach (TerrainInfo tI in patchList)
        {
            for (int i = 0; i < splatDetailPatchRes; i++)
            {
				patchQueue.Enqueue(new SplatDetailPatch(tI.globalX, tI.globalZ, tI.terrain,
					InfiniteTerrain.m_alphaMapSize * i / splatDetailPatchRes, InfiniteTerrain.m_alphaMapSize * (i + 1) / splatDetailPatchRes, tI));
			}    
        }

		foreach (TerrainInfo tI in patchList)
		{
			// todo reconsider if this shoul be done in one go for each terrain, not inside foreach loop three times
			AreaData aData = InfiniteTerrain.GetAreaData(tI.globalX, tI.globalZ);
			if(aData.castleData != null)
			{
				Debug.Log("Castle exists, x: " + aData.castleData.coordX + " " + aData.castleData.coordZ + " size: " + aData.castleData.size);
			}

			for (int i = 0; i < treePatchRes; i++)
			{
				patchQueue.Enqueue(new TreePatch(tI.globalX, tI.globalZ, tI.terrain,
					InfiniteTerrain.numOfTreesPerTerrain * i / treePatchRes, InfiniteTerrain.numOfTreesPerTerrain * (i + 1) / treePatchRes, tI));
			}
		}
		patchList.Clear();
    }
}
