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

	private double initialClickT = 0;
	private Deck previousDeck = null;

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
	void Update () 
	{
		if ( Input.GetMouseButtonDown(0) == true) //left clicked
		{
			initialClickT = Time.time;
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
					previousDeck = temp.deck;
					if(temp.deck == null)
					{
						Debug.Log ("Error, no deck to return card to, not set in Layout() function" );
					}
					int index = temp.deck.IndexOf (temp);

					moveDeck.addCardArray( temp.deck.GetCardsFromIndex(index));
					temp.deck.RemoveCardsFromIndex(index);
				}
			}
		}
		if(cardMoving != null)
		{ //todo: offset
			cardMoving.transform.position = this.transformWithZ(Camera.main.ScreenToWorldPoint(Input.mousePosition) - cardOffset, cardMoving.transform.position.z);
		}

		if (Input.GetMouseButtonUp (0) == true)
		{
			if(cardMoving != null) //otherwise clicked a button somewhere
			{
				bool wentToNewDeck = false;

				//cardMoving is the game object for purposes of motion, should rename
				Card movedCard = cardMoving.gameObject.GetComponent<Card>();

				if(Time.time - initialClickT < 0.3f) //if less than 0.3 seconds, tapped card, otherwise test for collision
				{
					ArrayList possibleCards = new ArrayList();

					Card tempCard = null;

					foreach(Deck possibleDeck in winDeck) //testing possible moves, go for wins first
					{
						tempCard = possibleDeck.LastCardInDeck();
						if(tempCard != null)
						{
							possibleCards.Add(tempCard);
						}
					}
					foreach(Deck possibleDeck in tableauDeck)
					{
						tempCard = possibleDeck.LastCardInDeck();
						if(tempCard != null)
						{
							possibleCards.Add(tempCard);
						}
					}

					//remove the deck the card came from as a possible move-to. Debugging a weird issue otherwise would do this with less code at the moment
					Card fromDeckCard = null;
					foreach(Card c in possibleCards)
					{
						if( c.deck.Equals(previousDeck))
						{
							fromDeckCard = c;
							break;
						}
					}
					if(fromDeckCard != null)
					{
						possibleCards.Remove (fromDeckCard);
					}

					foreach(Card c in possibleCards)
					{
						if(c.deck.CanAdd( movedCard ) == true ) //can add, remove from here and add to that deck.
						{
							c.deck.addCardArray ( moveDeck.GetCardsFromIndex(0));
							moveDeck.RemoveEveryCard();
							c.deck.LayoutDeck();
							wentToNewDeck = true;
							break;
						}
					}
				}
				else //card was actively dragged somewhere, place or revert to last
				{
					//wentToNewDeck = true; //just b/c for testing
				}

				if(wentToNewDeck == false)
				{
					previousDeck.addCardArray ( moveDeck.GetCardsFromIndex(0));
					moveDeck.RemoveCardsFromIndex(0);
//					previousDeck.RemoveCardsFromIndex(0);
					previousDeck.LayoutDeck();
				}
				else
				{
					//test for game win
				}

				lastMousePosition = Vector3.zero;
				cardMoving = null;
				cardOffset = Vector3.zero;
				previousDeck = null;
				initialClickT = 0.0f;
			}
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
			handSize = tableauDeck[i].CardCount();

			tableauDeck[i].faceCardsWithRange(0, handSize - 1, false);
			tableauDeck[i].faceCardsWithRange(handSize - 1, handSize - 1, true); //front face the card
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
		                  