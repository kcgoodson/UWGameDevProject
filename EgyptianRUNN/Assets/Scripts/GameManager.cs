using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public int suits;
	public int ranks;
	static int totalCardCount;
	public int playerCount;
	public int royalRank;
	public GameObject card;
	public GameObject playerObject;

	static Sprite[] cardSkins;
	static GameManager m;
	static GameObject[] players;
	static int currentPlayer;

	static Queue tableau;
	static Stack burn;
	static Card[] slapCheck;

	static bool gameOver;
	static int royalPlayer;
	static int royalCount;


	// Use this for initialization
	void Start () {
		m = this;
		Queue deck = Shuffle(InitialDeck(suits, ranks));
		totalCardCount = deck.Count;
		tableau = new Queue();
		burn = new Stack();
		slapCheck = new Card[] {new Card(-1, -1), new Card(-1, -2), new Card(-1, -3)};
		SetupSkins();
		SetupPlayers();
		DealCards(deck);
		currentPlayer = (int) Random.Range(0, playerCount);
		gameOver = false;
		royalPlayer = -1;
		royalCount = -1;
	}

	void Update() {
		Debug.Log(SlapValid());
		LabelPlayers();
		CheckEndGame();
	}

	//Sets up a new deck with the number of Suits each with number of Ranks
	Queue InitialDeck(int suit, int rank) {
		Queue deck = new Queue();
		for(int i = 0; i < suit; i++) {
			for(int j = 0; j < rank; j++) {
				if(j > royalRank)
					deck.Enqueue(new Card(i, j, j - royalRank));
				else
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
			GameObject nextPlayer = Instantiate(playerObject);
			nextPlayer.GetComponent<Player>().SetupPlayer(i);
			nextPlayer.name = "Player: " + i;
			RectTransform rectTrans = nextPlayer.GetComponent<RectTransform>();
			rectTrans.position = new Vector2((Screen.width - rectTrans.rect.width) * (i % 2), ((Screen.height - rectTrans.rect.height) * ((3 - i) / 2)));
			players[i] = nextPlayer;
		}
		GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");
		foreach(GameObject p in players) {
			p.transform.SetParent(canvas.transform);
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
	public static int CurrentPlayerID() {
		return currentPlayer;
	}

	//Sets the next player to be the current player
	public static void NextPlayer() {
		if(!PlayersHaveCards()) {
			currentPlayer = -1;
			return;
		}
		int tempNext = currentPlayer + 1;
		if(tempNext >= players.Length)
			tempNext = 0;
		currentPlayer = tempNext;

	}

	//Adds a card to the Tableau
	public static void StackCard(object next, int id) {
		tableau.Enqueue(next);
		SlapCycle((Card) next);
		CreateCard((Card) next);

	}

	//Adds a card to the Burn Pile
	public static void BurnCard(object next) {
		burn.Push(next);
	}

	//Returns the Current Player object
	public static Player CurrentPlayer() {
		if(currentPlayer < players.Length && currentPlayer >= 0)
			return players[currentPlayer].GetComponent<Player>();
		return null;
	}

	//Retuns Indexed Player Object
	static Player PlayerAt(int i) {
		return players[i].GetComponent<Player>();
	}

	//Gives the card to the specified player
	static void GiveCard(int playerID, object nextCard) {
		players[playerID].GetComponent<Player>().GetCard((Card)nextCard);
	}

	//Updates the Valid Slap Stack
	static void SlapCycle(Card next) {
		slapCheck[2] = slapCheck[1];
		slapCheck[1] = slapCheck[0];
		slapCheck[0] = next;
	}

	//Returns True if a Double or a Sandvich
	public static bool SlapValid() {
		return (slapCheck[0].Rank() == slapCheck[1].Rank() ||
		        slapCheck[0].Rank() == slapCheck[2].Rank());
	}

	//Instatiates the Card in the Scene
	public static void CreateCard(Card next) {
		GameObject nextCard = (GameObject) Instantiate(m.card, new Vector2(m.card.transform.position.x, m.card.transform.position.y + tableau.Count * m.card.transform.lossyScale.y), Quaternion.identity);
		nextCard.GetComponent<SpriteRenderer>().sprite = CardSkin(next);
		m.gameObject.transform.Translate(new Vector2(0, m.card.transform.lossyScale.y));
	}

	//Gives the Stack and Burned Cards to the Royal Player
	public static void CollectRoyal() {
		Collect(royalPlayer);
	}

	//Gives the Stack and Burned Cards to the Passed player
	public static void Collect(int playerID) {
		currentPlayer = -1;
		royalPlayer = -1;
		royalCount = -1;
		int burnCount = burn.Count;
		int stackCount = tableau.Count;
		for(int i = 0; i < burnCount; i++) {
			GiveCard(playerID, burn.Pop());
		}
		for(int i = 0; i < stackCount; i++) {
			GiveCard(playerID, tableau.Dequeue());
		}
		for(int i = 0; i < players.Length; i++) {
			if(!PlayerAt(i).HasCards())
				PlayerAt(i).Lose();
		}
		slapCheck = new Card[] {new Card(-1, -1), new Card(-1, -2), new Card(-1, -3)};
		m.gameObject.transform.position = Vector2.zero;
		GameObject[] allCards = GameObject.FindGameObjectsWithTag("Card");
		foreach(GameObject o in allCards)
			Destroy(o);
		currentPlayer = playerID;
	}

	//Checks if the Game is Ended
	static void CheckEndGame() {
		if(PlayerHasWon() != -1)
			Win(PlayerHasWon());
		else if(!PlayersHaveCards() && !SlapValid())
			Draw();
	}

	static void Draw() {
		currentPlayer = -1;
		gameOver = true;
		Debug.Log("DRAW");
	}

	static void Win(int playerID) {
		currentPlayer = -1;
		gameOver = true;
		Debug.Log ("WINNER! : " + playerID);
	}

	//Returns true if at least on player has cards
	static bool PlayersHaveCards() {
		for(int i = 0; i < players.Length; i++) {
			if(PlayerAt(i).HasCards())
				return true;
		}
		return false;
	}

	//Returns the Last Player Standing. Otherwise returns -1;
	static int PlayerHasWon() {
		bool win = false;
		int winPlayer = -1;
		for(int i = 0; i < players.Length; i++) {
			if(PlayerAt(i).isAlive()) {
				if(!win) {
					win = true;
					winPlayer = i;
				} else {
					return -1;
				}
			}
		}
		return winPlayer;
	}

	public static int[] PlayerSummary() {
		int[] count = new int[players.Length];
		for(int i = 0; i< count.Length; i++)
			count[i] = PlayerAt(i).CardCount();
		return count;
	}

	static void LabelPlayers() {
		for(int i = 0; i < players.Length; i++) {
			string text = "";
			if(currentPlayer == i && PlayerAt(i).HasCards() && PlayerAt(i).isAlive())
				text += "*";
			text += "Player "+ (i + 1);
			if(PlayerAt(i).isAlive())
				text += " Cards: " + PlayerAt(i).CardCount();
			else
				text += " X_X";
			PlayerAt(i).GetComponent<Text>().text = text;
		}
	}

	public static bool GameOver() {
		return gameOver;
	}
}
