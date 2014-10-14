using UnityEngine;
using System.Collections;

public class GameMgr : MonoBehaviour {



	private Vector3 lastMousePosition = Vector3.zero;
	private GameObject cardMoving = null;
	private Vector3 cardOffset = Vector3.zero;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if ( Input.GetMouseButtonDown(0) == true) //left clicked
		{

			lastMousePosition = Input.mousePosition;
			//Vector2 ray = new Ray2D( lastMousePosition, 0 );
			Vector3 worldPt = Camera.main.ScreenToWorldPoint( lastMousePosition);
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint( lastMousePosition), Vector2.zero);

			if(hit.collider != null)
			{
				Card temp = hit.collider.gameObject.GetComponent<Card>();
				//temp = temp.GetComponent<Card>() as Card; //ensure it's a card
				if(temp != null)
				{
					cardMoving = temp.gameObject;
					cardOffset = worldPt - cardMoving.transform.position;
				}
			}
		}
		if(cardMoving != null)
		{ //todo: offset
			cardMoving.transform.position = this.transformWithZ(Camera.main.ScreenToWorldPoint(Input.mousePosition) - cardOffset,0);
		}

		if (Input.GetMouseButtonUp (0) == true)
		{
			lastMousePosition = Vector3.zero;
			cardMoving = null;
			cardOffset = Vector3.zero;
		}

	}

	//TODO: move to helper script with input helpers
	public Vector3 transformWithZ(Vector3 inTransform, float desiredZ)
	{
		return new Vector3(inTransform.x, inTransform.y, desiredZ);	                  
	}















}
		                  