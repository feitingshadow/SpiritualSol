using UnityEngine;
using System.Collections;

public class GameMgr : MonoBehaviour {



	private Vector3 lastMousePosition = Vector3.zero;
	private GameObject cardMoving = null;
	private Vector3 cardOffset = Vector3.zero;

	public Deck dealDeck;
	public Deck junkDeck;
	public Deck[] tableauDeck;
	public Deck[] winDeck;
	public Deck moveDeck;
	// Use this for initialization

	private ArrayList allDecks = new ArrayList();

	void Start () 
	{
		//todo: ensure this doesn't get ran twice, adding multiple on scene activation start() function
		if(allDecks.Count == 0) //might work good enough
		{
			Debug.Log ("All decks being added to");
			allDecks.Add (moveDeck);
			allDecks.Add (dealDeck);
			allDecks.Add (junkDeck);
			foreach(Deck deck in tableauDeck)
			{
				allDecks.Add(deck);
			}

			foreach(Deck deck in winDeck)
			{
				allDecks.Add (deck);
			}
		}
		else
		{
			Debug.Log ("Error, code should only run once, ensure dupes don't exist other places");
		}

		this.RefreshGame();
	}

	void LateStart()
	{

	}
	
	// Update is called once per frame
	void Update () {
	
		if ( Input.GetMouseButtonDown(0) == true) //left clicked
		{

			lastMousePosition = Input.mousePosition;
			//Vector2 ray = new Ray2D( lastMousePosition, 0 );
			Vector3 worldPt = Camera.main.ScreenToWorldPoint( lastMousePosition);
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
				//new Ray(Camera.main.ScreenToWorldPoint( lastMousePosition), Vector3.back);
			RaycastHit hit; // = Physics.Raycast(Camera.main.ScreenToWorldPoint( lastMousePosition), Vector3.back, out ray); //distance 350 for z positioning in 2d layers

			if(Physics.Raycast(ray, out hit) && hit.collider != null)
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
			cardMoving.transform.position = this.transformWithZ(Camera.main.ScreenToWorldPoint(Input.mousePosition) - cardOffset, cardMoving.transform.position.z);
		}

		if (Input.GetMouseButtonUp (0) == true)
		{
			lastMousePosition = Vector3.zero;
			cardMoving = null;
			cardOffset = Vector3.zero;
		}

	}

	public void RefreshGame()
	{
		dealDeck.addCardArray( this.GetAllCards());

		int handSize = 0;
		for(int i = 0; i < tableauDeck.Length; i++)
		{
			this.MoveCardsFromIndexInDeckToIndexInDeck( dealDeck.CardCount()-i - 1, dealDeck, 0, tableauDeck[i]);
			tableauDeck[i].LayoutDeck();
		}

		dealDeck.LayoutDeck();
	}

	public ArrayList GetAllCards()
	{
		ArrayList everyCard = new ArrayList();
		foreach(Deck tempD in allDecks)
		{
			Debug.Log ("temp count: " + tempD.CardCount());
			everyCard.AddRange (tempD.GetAllCardsInDeck());

			tempD.RemoveCardsFromIndex(0); //all cards in deck
		}

		return everyCard;
	}

	public void MoveCardsFromIndexInDeckToIndexInDeck(int ind1, Deck deck1, int ind2, Deck deck2)
	{
		ArrayList tempList = new ArrayList();
		tempList.AddRange(deck1.GetCardsFromIndex(ind1));
		deck2.addCardArrayAtIndex( tempList, deck2.CardCount() ); //might be oo range here.
		deck1.RemoveCardsFromIndex (ind1);
		deck2.LayoutDeck();
	}

	//TODO: move to helper script with input helpers
	public Vector3 transformWithZ(Vector3 inTransform, float desiredZ)
	{
		return new Vector3(inTransform.x, inTransform.y, desiredZ);	                  
	}















}
		                  