using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDetector : MonoBehaviour, IPointerClickHandler
{

	public GameStarter gameStarter;

	public RectTransform rect;


	public void OnPointerClick(PointerEventData eventData)
	{
		Vector2 localPoint;

		RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, null, out localPoint);

		Debug.Log("------------------------------------>localPoint" + localPoint);
		gameStarter.ConvertClickToMapCordinate(localPoint);

	}

}
