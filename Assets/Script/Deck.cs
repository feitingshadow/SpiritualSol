using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

public class Deck : MonoBehaviour {

	//public Card firstCard = null;
	public int startDecks = 0; //Number of decks of card the deck should instantiate

	public Vector3 cardOffset = Vector3.zero; //how far every card in the pile moves downward from the one above it.
	//used so the layoutCards function can be the same for every deck.

	public Card cardPrefab;

	public GameMgr gameMgr;

	//private Vector3 cardOffset = Vector3.zero;
	private ArrayList cardsArray = new ArrayList();
	//insert and remove are very expensive operations on arrays, creating 2 to reduce shuffle-sorting costs.
	private ArrayList shuffleArray = new ArrayList();

	//The below are the solitaire rules. Tableaus (the 7 piles) take decrements with alternating colors. Win piles go up, from zero
	//These public values allow you to change the rules via editor, without implementing Delegate.
	public int addCardRuleIncrement = 1; //winpiles and tableau uses, -1 and 1 respective
	public bool alternateRule; //does the color have to change or stay same
	public bool emptyAscendingRule; // Win piles ascend, so yes. Tableaus must start with King, they go downward
	public bool usesRules = true; //the deck won't use rules, it just gets added to. 

	public int numFaceUp = 0; //last few cards faceUp, 3 for klondike 3 on deck. 1 for tableau

	// Use this for initialization
	void Awake () 
	{
		if( (cardPrefab != null))
		{
			for(int i = 0; i < startDecks; i++) //create all decks for the game!
			{
				this.CreateCardDeck(); //this should be handled by GameManager instead, to allow for more
			}

			this.Shuffle(); //calls layout
			this.LayoutDeck();
		}
	}

	public Bounds lastBoundsWorldSpace() //Gets the last card, or deck rectangle.
	{
		if(cardsArray.Count == 0)
		{
			return this.collider.bounds;
		}
		else
		{
			return LastCardInDeck().collider.bounds;
		}
	}

	private void CreateCardDeck()
	{
		Card tempCard = null;
	
		for (int i = 0; i < 52; i++)
		{
			cardsArray.Add( GameObject.Instantiate(cardPrefab)); //memory hog in one spot... good until more than 1
		}

		//Cards created, now change their images and values to be real
		for (int c = 0; c < 4; c++) //suites
		{
			for (int i = 1; i <= 13; i++)
			{
				tempCard = (Card)cardsArray[(i-1) + c*13]; 
				//sRenderer = tempCard.GetComponent("SpriteRenderer") as SpriteRenderer;
				tempCard.Suite = c;
				tempCard.Val = i;
				tempCard.RefreshImage();
			}
		}
	}

	// Update is called once per frame
	void Update () {
	
	}

	//return the last bounding box, which is the card on top, or the deck itself if empty. To help test collision when deck is empty.
	public BoxCollider lastCollider()
	{ //Assumes deck has BoxCollider
		if(cardsArray.Count > 0)
		{
			return LastCardInDeck().collider as BoxCollider;
		}
		else
		{
			return this.collider as BoxCollider;
		}
	}

	//lots of todo's on ensuring i is within range everywhere on ArrayLists
	public ArrayList GetCardsFromIndex(int i)
	{
		if(i < 0)
		{
			//int ad = 3;
		//	ad = 5;
		}
		//Debug.Log("Getting Cards from Ind: " + i + "With Count: " + cardsArray.Count);

		if(cardsArray.Count > 0) 
		{
			return cardsArray.GetRange(i, cardsArray.Count - i); //todo, try/catch, might go OOBounds. 
		}
		else
		{
			Debug.Log("Getting Cards from Ind: 0, Error!");
		}

		return null;
	}

	public ArrayList GetAllCardsInDeck()
	{
		return new ArrayList(this.cardsArray);
	}

	public void RemoveCardsFromIndex(int ind)
	{
		try
		{
		//	Debug.Log ("Index: " + ind + " Count is: " + cardsArray.Count);
			if(cardsArray.Count > ind)
			{
				cardsArray.RemoveRange (ind, cardsArray.Count - ind); 
			}
		}
		catch (UnityException ex)
		{
			Debug.Log ("Remove count is out of range Ex:" + ex.Message);
		}
		finally { //nothing
		}

	}

	public void RemoveEveryCard()
	{
		cardsArray.Clear (); 
	}

	//Todo: make local
	public static Rect ColliderBoundsTo2DRect(BoxCollider collider)
	{ 

		float halfXSize = collider.size.x/2.0f;
		float halfYSize = collider.size.y/2.0f;

		Vector3 worldPos = collider.transform.position;

		return new Rect( worldPos.x - halfXSize, worldPos.y - halfYSize, collider.size.x, collider.size.y);

		//Code below is for bounds, from Unity3D site (unused), I only need 2D coordinates for this

		// Get mesh origin and farthest extent (this works best with simple convex meshes)
//		Vector3 origin = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, 0f));
//		Vector3 extent = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, 0f));
//		// Create rect in screen space and return - does not account for camera perspective
//		return new Rect(origin.x, Screen.height - origin.y, extent.x - origin.x, origin.y - extent.y);
	}

	//Todo: move all these geo helper functions to common area
	public static bool RectsIntersect(Rect r1, Rect r2)
	{
		float right1 = r1.x + r1.width;
		float right2 = r2.x + r2.width;

		float bottom1 = r1.y + r1.height;
		float bottom2 = r2.y + r2.height;

		if( r1.x < right2 &&  right1 > r2.x && r1.y < bottom2 && bottom1 > r2.y)
		{
			return true;
		}
		return false;
	}

	/* helper utility for bounds to screen rect, from Unity Help forum
	public static Rect GUIRectWithObject(GameObject go)
	{
		Vector3 cen = go.renderer.bounds.center;
		Vector3 ext = go.renderer.bounds.extents;
		Vector2[] extentPoints = new Vector2[8]
		{
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
			
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
		};
		
		Vector2 min = extentPoints[0];
		Vector2 max = extentPoints[0];
		
		foreach(Vector2 v in extentPoints)
		{
			min = Vector2.Min(min, v);
			max = Vector2.Max(max, v);
		}
		
		return new Rect(min.x, min.y, max.x-min.x, max.y-min.y);
	}
	*/


	//below is replaced by CanAdd, instead of interface I decided to use the IDE's properties, normally I'd implement a protocol
	public bool isValidCard(Card card) //must be overridden (Convert to interface) or del  
	{
		return false;
	}

	public void addCard(Card card)
	{
		cardsArray.Add(card);
	}

	public int CardCount() //this is a safe function, need more of these to address error checks
	{ //protects the property from being direclty changed
		return cardsArray.Count;
	}

	public void addCardArray(ArrayList cards)
	{
		if(cards != null)
		{
			ArrayList newList = new ArrayList(cards);
			cardsArray.AddRange(newList); //todo, check for errors
		}
		else
		{
			Debug.Log ("Adding null array of cards!");
		}
	}

	public void addCardArrayAtIndex(ArrayList cards, int ind)
	{
		cardsArray.InsertRange(ind, cards); //todo, check for within bounds
	}

	public Card LastCardInDeck()
	{
		if(cardsArray.Count > 0) 
		{
			return cardsArray[cardsArray.Count - 1] as Card;
		}
		return null;
	}

//	public ArrayList GetAndRemoveCardsAtIndex(int i)
//	{
//		ArrayList cards = this.GetCardsFromIndex (i);
//		this.RemoveCardsFromIndex ( i );
//		return cards;
//	}

	public int IndexOf(Card c)
	{
		return cardsArray.IndexOf(c); //might fail if card isn't in, not worrying since only tested on decks with definite existence so far
	}

	public bool CanAdd(Card card) //tests with rules to see if can add to current pile(s)
	{
		bool retVal = true;

		if(card != null)
		{
			Card lastCard = this.LastCardInDeck();
			if(lastCard == null) //empty
			{
				if(emptyAscendingRule == true) //winDeck, starts ace
				{
					if(card.Val != 1)
					{
						retVal = false;
					}
				}
				else 
				{
					if(card.Val != 13) //descending, returns true if King
					{
						retVal = false;
					}
				}
				retVal = false;
			}
			else
			{
				if(usesRules)
				{
					//bool ret = true; //easier to assume truth and prove false, than to test if true down the line

					if(alternateRule == true && ( card.IsSuiteEven() == lastCard.IsSuiteEven() ) )
					{
						retVal = false;
					}
					Debug.Log ("Testing addable val: " + card.Val + " against Last: " + lastCard.Val + " Increment v: " + addCardRuleIncrement);
					if( (lastCard.Val + addCardRuleIncrement) != card.Val)
					{
						retVal = false;
					}
				}
			}
		}
		Debug.Log ("Retval is giving: " + retVal);
		return retVal;
	}

	public void Shuffle()
	{
		//Todo, sort randomly the cards in the deck

		int maxC = cardsArray.Count;
		Card tempSwapping = null;
		int randNumber = 0;

		for(int i = 0; i < maxC; i++)
		{
			randNumber = Random.Range(0, cardsArray.Count); //int = exclusive, float = inclusive - unity forums, but precision gives it away
//			Debug.Log ("RandNum = " + randNumber);
			tempSwapping = cardsArray[randNumber] as Card;
			cardsArray.RemoveAt(randNumber);
			shuffleArray.Add (tempSwapping);
		}
	//	Debug.Log ("Cards Arr C: " + cardsArray.Count);
		cardsArray.AddRange (shuffleArray);
		shuffleArray.Clear ();
		this.LayoutDeck();
	}

	public void flipAllCards(bool frontFacing)
	{
		foreach(Card c in cardsArray)
		{
			if(c.frontFacing != frontFacing)
			{
				c.flip();
			}
		}
	}

	public void LayoutDeck()
	{
		Card tempCard = null;
		Card lastCard = null;
		for(int i = 0; i < cardsArray.Count; i++)
		{
			tempCard = (Card)cardsArray[i];
			tempCard.deck = this;

			tempCard.spriteR.sortingOrder = i;
			if(i == 0)
			{
				tempCard.transform.parent = this.transform;
				tempCard.transform.localPosition = Vector3.zero;
			}
			else
			{
				tempCard.transform.parent = lastCard.transform;
				tempCard.transform.localPosition = new Vector3(cardOffset.x, cardOffset.y, i/30.0f);
			}

			if(i >= (cardsArray.Count - numFaceUp))
			{
				if(tempCard.frontFacing == false)
				{
					tempCard.flip();
				}
			}

			lastCard = tempCard;
		}
	}

	public void faceCardsWithRange(int startIndex, int endIndex, bool isFrontFacing) //lays out deck, ensure first to last index faces direction
	{
		//ensure safe handling
		if(startIndex >= 0 && endIndex >= 0 && startIndex < cardsArray.Count && endIndex >= startIndex && endIndex < cardsArray.Count)
		{
			Card c = null;
			for(int i = startIndex; i <= endIndex; i++)
			{
				c = cardsArray[i] as Card;
				if(c.frontFacing != isFrontFacing)
				{
					c.flip(); //change if it doesn't match the desired direction/facing.
				}
			}
		}
		else
		{
			//trace and eliminate logic flaws if this happens to be called
			Debug.Log ("Out of range for facing cards in deck. Start: " + startIndex + " end: " + endIndex);
		}
	}


//	public string getSuiteNameFromNumber(int num)
//	{
//		switch(num)
//		{
//			case 1:
//			{
//				return "hearts";
//			}
//			case 2:
//			{
//				return "spades";
//			}
//			case 3:
//			{
//				return "diamonds";
//			}
//			case 4:
//			{
//				return "clubs";
//			}
//			default:
//			{
//				System.Console.WriteLine ("Error in suiteNameFromNumber, returning NULL!");
//				return null;
//			}
//		}
//	}
//	
//	public int getSuiteNumberFromName(string suiteName)
//	{
//		switch(suiteName)
//		{
//			case "hearts":
//			{
//				return 1;
//			}
//			case "spades":
//			{
//				return 2;
//			}
//			case "diamonds":
//			{
//				return 3;
//			}
//			case "clubs":
//			{
//				return 4;
//			}
//			default:
//			{
//				return 0;
//			}
//		}
//	}
}
