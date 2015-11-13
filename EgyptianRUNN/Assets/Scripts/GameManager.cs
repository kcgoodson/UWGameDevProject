using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public int suits;
	public int ranks;
	public int playerCount;
	public GameObject card;

	static Sprite[] cardSkins;
	static GameManager m;
	static GameObject[] players;
	static int currentPlayer;

	static Queue tableau;
	static Queue burn;
	static Card[] slapCheck;


	// Use this for initialization
	void Start () {
		m = this;
		Queue deck = Shuffle(InitialDeck(suits, ranks));
		tableau = new Queue();
		burn = new Queue();
		slapCheck = new Card[3];
		SetupSkins();
		SetupPlayers();
		DealCards(deck);

	}

	void Update() {
		if(currentPlayer >= players.Length)
			currentPlayer = 0;
		Debug.Log(SlapValid());
	}

	//Sets up a new deck with the number of Suits each with number of Ranks
	Queue InitialDeck(int suit, int rank) {
		Queue deck = new Queue();
		for(int i = 0; i < suit; i++) {
			for(int j = 0; j < rank; j++) {
				deck.Enqueue(new Card(i, j));
			}
		}
		return deck;
	}

	//Randomizes the passed Queue
	public static Queue Shuffle(Queue deck) {
		List<object> shuffledDeck = new List<object>();
		int deckLength = deck.Count;
		for(int i = 0; i < deckLength; i++) {
			shuffledDeck.Add(deck.Dequeue());
		}
		shuffledDeck.Sort ((object x, object y) => Random.value.CompareTo(Random.value));
		for(int i = 0; i < shuffledDeck.Count; i++) {
			deck.Enqueue(shuffledDeck[i]);
		}
		return deck;
	}

	//Loads all the Card Textures for later fetching
	static void SetupSkins() {
		object[] cardpics = Resources.LoadAll("Cards", typeof(Sprite));
		cardSkins = new Sprite[cardpics.Length];
		for(int i = 0; i < cardpics.Length; i++) {
			cardSkins[i] =  (Sprite) cardpics[i];
		}
	}


	//Returns the Card Texture
	public static Sprite CardSkin(Card next) {
		return CardSkin(next.Suit() * m.ranks + next.Rank());
	}

	public static Sprite CardSkin(int i) {
		return cardSkins[i];
	}
	
	//Creates and Instates Player Objects
	void SetupPlayers() {
		players = new GameObject[playerCount];
		for(int i = 0; i < players.Length; i++) {
			GameObject nextPlayer = new GameObject();
			nextPlayer.AddComponent<Player>();
			nextPlayer.GetComponent<Player>().SetupPlayer("Player " + (i + 1), i);
			nextPlayer.name = "Player: " + i;
			players[i] = nextPlayer;
		}

	}

	//Deals the Cards to the Players
	static void DealCards(Queue deck) {
		int deckLength = deck.Count;
		for(int i = 0; i < deckLength; i++) {
			players[(i%players.Length)].GetComponent<Player>().GetCard((Card) deck.Dequeue());
		}
	}

	//Returns the Current Player ID
	public static int CurrentPlayer() {
		return currentPlayer;
	}
	
	//Sets the next player to be the current player
	public static void NextPlayer() {
		int tempNext = currentPlayer + 1;
		if(tempNext > players.Length)
			tempNext = 0;
		currentPlayer = tempNext;
	}

	//Adds a card to the Tableau
	public static void StackCard(object next) {
		tableau.Enqueue(next);
		SlapCycle((Card) next);
		CreateCard((Card) next);
	}

	//Adds a card to the Burn Pile
	public static void BurnCard(object next) {
		burn.Enqueue(next);
	}

	//Updates the 
	static void SlapCycle(Card next) {
		slapCheck[2] = slapCheck[1];
		slapCheck[1] = slapCheck[0];
		slapCheck[0] = next;
	}

	//Returns True if a Double or a Sandvich
	public static bool SlapValid() {
		HashSet<int> temp = new HashSet<int>();
		foreach(Card c in slapCheck) {
			if(c == null)
				return false;
			if(temp.Contains(c.Rank()))
			   return true;
			temp.Add(c.Rank());
		}
		return false;
	}

	//Instatiates the Card in the Scene
	public static void CreateCard(Card next) {
		GameObject nextCard = (GameObject) Instantiate(m.card, new Vector2(m.card.transform.position.x, m.card.transform.position.y + tableau.Count * m.card.transform.lossyScale.y), Quaternion.identity);
		nextCard.GetComponent<SpriteRenderer>().sprite = CardSkin(next);
		m.gameObject.transform.Translate(new Vector2(0, m.card.transform.lossyScale.y));
	}

	//Gives the Stack and Burned Cards to the Passed player
	public static void Collect(int playerID) {
		currentPlayer = playerID;
		int burnCount = burn.Count;
		int stackCount = tableau.Count;

	}
}
