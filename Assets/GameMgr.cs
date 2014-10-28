using UnityEngine;
using System.Collections;

public class GameMgr : MonoBehaviour {



	private Vector3 lastMousePosition = Vector3.zero;
	private GameObject cardMoving = null;
	private Vector3 cardOffset = Vector3.zero;

	private float dragTime = 0.3f; //time it takes after clicking to be considered dragging the card
	
	public Deck dealDeck;
	public Deck junkDeck;
	public Deck[] tableauDeck;
	public Deck[] winDeck;
	public Deck moveDeck;
	// Use this for initialization

	private ArrayList allDecks = new ArrayList();
	private ArrayList allMoveableDecks = new ArrayList(); //All decks cards can move to, for ease of auto-moving
	private double initialClickT = 0;
	private Deck previousDeck = null;

	public bool isDragging = false;

	void Start () 
	{
		//todo: ensure this doesn't get ran twice, adding multiple on scene activation start() function
		if(allDecks.Count == 0) //might work good enough
		{
			Debug.Log ("All decks being added to");
			allDecks.Add (moveDeck);
			allDecks.Add (dealDeck);
			allDecks.Add (junkDeck);

			foreach(Deck deck in winDeck)
			{
				allDecks.Add (deck);
				allMoveableDecks.Add (deck);
			}

			foreach(Deck deck in tableauDeck)
			{
				allMoveableDecks.Add (deck);
				allDecks.Add(deck);
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
		if ( Input.GetMouseButtonDown(0) == true && cardMoving == null) //left clicked
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
			if( isDragging == false && (Time.deltaTime - initialClickT) > dragTime)
			{
				isDragging = true;
			}
		}

		if (Input.GetMouseButtonUp (0) == true)
		{
			if(cardMoving != null) //otherwise clicked a button somewhere
			{
				bool wentToNewDeck = false;

				//cardMoving is the game object for purposes of motion, should rename
				Card movedCard = cardMoving.gameObject.GetComponent<Card>();

				if(isDragging == false) //if less than 0.3 seconds, tapped card, otherwise test for collision
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
					//card was dragged, find out what decks it is touching, and last cards in those decks.
					ArrayList currentlyTouchedDecks = new ArrayList();
					foreach(Deck deck in allMoveableDecks) //should have WinDecks first.
					{
						if( Deck.RectsIntersect( Deck.ColliderBoundsTo2DRect(deck.LastCardInDeck().collider as BoxCollider), Deck.ColliderBoundsTo2DRect(movedCard.collider as BoxCollider) ) )
						{
							currentlyTouchedDecks.Add (deck);
						}
					}

					//Got which cards are touching, now to see what you can move to.
					if(currentlyTouchedDecks.Count > 0)
					{
						ArrayList possibleMoveDecksArray = new ArrayList();

						foreach(Deck deck in currentlyTouchedDecks)
						{
							if( deck.CanAdd(movedCard) )
							{
								possibleMoveDecksArray.Add(deck);
							}
						}

						//If more one is possible, move there, more than one, find closest deck and move there.

						if(possibleMoveDecksArray.Count > 0)
						{
							if(possibleMoveDecksArray.Count > 1) // more than one touching
							{
								float minDistance = 9999.0f;
								float distance = 9999.0f; // large distance
								Deck closest = null;

								foreach(Deck d in possibleMoveDecksArray)
								{
									//distance = movedCard;
								
									distance = Vector2.Distance(Deck.ColliderBoundsTo2DRect(movedCard.collider as BoxCollider).center, d.lastBoundsWorldSpace().center);
									if(distance < 0)
									{
										distance *= -1; //make absolute value, not testing for left/right, just magnitude
									}

									if(distance < minDistance)
									{
										closest = d;
									}
								}
								//closest deck has been found! add it

								closest.addCard(movedCard);
								if(closest == null)
								{
									Debug.Log("Error! closest card is null! After min calc");
								}
							}
							else
							{
								Deck d = possibleMoveDecksArray[0] as Deck;
								d.addCard(movedCard);
								//auto removes from moveDeck at end of this
							}
							wentToNewDeck = true;
						}
					}


					//wentToNewDeck = true; //just b/c for testing
				}

				//always remove cards from the moveDeck, not needed in if() statements.

				layoutAllDecks();

				if(wentToNewDeck == false)
				{
					previousDeck.addCardArray ( moveDeck.GetCardsFromIndex(0));
//					previousDeck.RemoveCardsFromIndex(0);
					previousDeck.LayoutDeck();
				}
				else
				{
					//test for game win
				}

				lastMousePosition = Vector3.zero;
				moveDeck.RemoveCardsFromIndex(0);
				cardMoving = null;
				cardOffset = Vector3.zero;
				isDragging = false;
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

	public void layoutAllDecks()
	{
		foreach(Deck d in allDecks)
		{
			d.LayoutDeck();
		}
	}













}
		                  