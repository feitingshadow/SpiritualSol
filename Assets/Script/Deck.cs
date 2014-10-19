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
	
	//private Vector3 cardOffset = Vector3.zero;
	private ArrayList cardsArray = new ArrayList();
	//insert and remove are very expensive operations on arrays, creating 2 to reduce shuffle-sorting costs.
	private ArrayList shuffleArray = new ArrayList();
	
	// Use this for initialization
	void Start () 
	{
		if( (cardPrefab != null))
		{
			for(int i = 0; i < startDecks; i++) //create all decks for the game!
			{
				this.CreateCardDeck();
			}
			this.Shuffle(); //calls layout
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

//	public void layoutCards()
//	{
//		//physically attach cards
//		if(firstCard != null)
//		{
//			firstCard.gameObject.transform.position = this.transform.position;
//			Card tempCard = firstCard.next;
//
//			//Todo: Deck position never changes, so create a static Transform instead of using getter through this.tran.pos...
//			Vector3 pos = new Vector3( this.transform.position.x, this.transform.position.y, this.transform.position.z);
//
//			while (tempCard.next != null)
//			{
//				tempCard = tempCard.next;
//				pos = new Vector3(pos.x + laidCardOffset.x, pos.y + laidCardOffset.y, pos.z);
//				tempCard.transform.position = pos;
//			}
//		}
//	}

	public bool isValidCard(Card card) //must be overridden (Convert to interface) or del  
	{
		return false;
	}

	public void addCard(Card card)
	{
//		if(firstCard == null)
//		{
//			firstCard = card;
//		}
//
//		this.layoutCards();
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
			Debug.Log ("RandNum = " + randNumber);
			tempSwapping = cardsArray[randNumber] as Card;
			cardsArray.RemoveAt(randNumber);
			shuffleArray.Add (tempSwapping);
		}
	//	Debug.Log ("Cards Arr C: " + cardsArray.Count);
		cardsArray.AddRange (shuffleArray);
//		for(int i = 0; i < maxC; i++)
//		{
//			cardsArray.Add(shuffleArray[i]);
//		}
//		Debug.Log ("Shuff Arr C: " + shuffleArray.Count);
		shuffleArray.Clear ();
		this.LayoutDeck();
	}


	public void LayoutDeck()
	{
		Card tempCard = null;
		Card lastCard = null;
		for(int i = 0; i < cardsArray.Count; i++)
		{
			tempCard = (Card)cardsArray[i];
			tempCard.spriteR.sortingOrder = i;
			if(i == 0)
			{
				tempCard.transform.parent = this.transform;
				tempCard.transform.localPosition = Vector3.zero;
			}
			else
			{
				tempCard.transform.parent = lastCard.transform;
				tempCard.transform.localPosition = new Vector3(cardOffset.x, cardOffset.y, -i/30.0f);
			}

			lastCard = tempCard;
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
