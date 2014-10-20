using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour {
	

	public Deck deck = null;
	public Card next = null;

	public SpriteRenderer spriteR = null;

	public int Suite;
	public int Val;

	public bool frontFacing = true;

	private static Sprite bg = Resources.Load("cardraw/cardBackOriginal", typeof(Sprite)) as Sprite;
	private Sprite foreground = null;

	public void RefreshImage()
	{
		if(foreground == null) //more efficient, image stays in fg
		{
			string prefix = null;
			string suffix = null;
			string tempString = null;
			string fullN = null;
			bool capitalizeName = true;
			if(Val == 1)
			{
				prefix = "a";
			}
			else if(Val == 11)
			{
				prefix = "jack";
			}
			else if(Val == 12)
			{
				prefix = "queen";
			}
			else if(Val == 13)
			{
				prefix = "king";
			}
			else
			{
				capitalizeName = false;
				prefix = Val.ToString();
			}
			
			suffix = this.getSuiteNameFromNumber(Suite);
			
			if(capitalizeName == true)
			{
				tempString = suffix; //giving example with "clubs"
				suffix = suffix.Remove(1); // suffix = "c"
				suffix = suffix.ToUpper();  //suffix = "C"
				suffix = suffix + tempString.Substring(1); //suffix = "Clubs", appends tempString from 2nd letter (1 in array)
				//convert first letter to capital.
			}
			fullN = prefix + suffix;
			//Debug.Log (fullN);
			
			foreground = Resources.Load("cardraw/" + fullN, typeof(Sprite)) as Sprite;
			if(foreground == null)
			{
				Debug.Log ("Failed to load card: " + fullN);
			}
			else
			{
				spriteR.sprite = foreground;
			}
		}
		else
		{

		}
		doCardDisplay ();
	}

	void Awake ()
	{
		//spriteR = this.GetComponent<SpriteRenderer>() as SpriteRenderer; 	
	}
	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void flip()
	{
		frontFacing = !frontFacing;
		doCardDisplay();
	}

	public void doCardDisplay()
	{
		if(frontFacing == true)
		{
			spriteR.sprite = foreground;
		}
		else
		{
			spriteR.sprite = bg;
		}

		this.collider.enabled = frontFacing; //don't allow bounds calcs on non front-facing images, requires buttons on top of decks since they normally are back-face
	}

	public string getSuiteNameFromNumber(int num)
	{
		switch(num)
		{
		case 0:
		{
			return "hearts";
		}
		case 1:
		{
			return "spades";
		}
		case 2:
		{
			return "diamonds";
		}
		case 3:
		{
			return "clubs";
		}
		default:
		{
			Debug.Log ("Error in suiteNameFromNumber, returning NULL!");
			return null;
		}
		}
	}
	
	public int getSuiteNumberFromName(string suiteName)
	{
		switch(suiteName)
		{
		case "hearts":
		{
			return 0;
		}
		case "spades":
		{
			return 1;
		}
		case "diamonds":
		{
			return 2;
		}
		case "clubs":
		{
			return 3;
		}
		default:
		{
			Debug.Log ("Failed to get Number from name " + suiteName);
			return 0;
		}
		}
	}
	 
}
