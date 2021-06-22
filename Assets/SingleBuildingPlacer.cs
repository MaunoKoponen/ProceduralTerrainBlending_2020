using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SingleBuildingPlacer : MonoBehaviour
{


	[SerializeField] public Texture2D HeightMask; // 128x128 image

	[SerializeField] public GameObject Tower;

	[SerializeField] public GameObject TooSteep;


/*
		public void FindBuildingPositions()
		{
			for (int i = 0; i < 20; i++)
			{

				Vector2 random2DPos = Random.insideUnitCircle * 100;


			}

		}

		public void InstantiateTower(Vector3 position)
		{
			GameObject building;
			building = Instantiate(Tower,position, Quaternion.identity);
			building.name = "Tower" + (int)Mathf.Floor(position.x) + "_" + (int)Mathf.Floor(position.z);
		}
*/
}
