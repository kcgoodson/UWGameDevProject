using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public int suits; //Default 4
	public int ranks; //Default 13
	public string[] playerNames; //2 - 4
	public int royalRank; //Default 8
	public GameObject card;
	public GameObject playerObject;

	static Sprite[] cardSkins;
	static Dictionary<string, Sprite> faces;
	static GameManager m;
	static GameObject[] players;
	static int currentPlayer;

	static Queue tableau;
	static Stack burn;
	static Queue royalBurn;
	static Card[] slapCheck;

	static bool gameOver;
	static bool collecting;
	static bool royalCollecting;

	static int royalPlayer;
	static int royalCount;

	public float breathTime;

	static Vector3 startPos;
	static Vector3 targetPos;
	public int stackHeightSize;

	void Start() {
		LoadGame(playerNames);
	}

	// Use this for initialization
	void LoadGame (string[] playerLabels) {
		this.playerNames = playerLabels;
		m = this;
		Queue deck = Shuffle(InitialDeck(suits, ranks));
		tableau = new Queue();
		burn = new Stack();
		royalBurn = new Queue();
		SetupSkins();
		SetupPlayers();
		SetupFaces();
		DealCards(deck);
		startPos = transform.position;
		ClearRound();
		currentPlayer = (int) Random.Range(0, playerNames.Length);
		gameOver = false;
	}

	void Update() {
		LabelPlayers();
		CheckEndGame();
		transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10);
		if(royalCount == 0)
			m.StartCoroutine(CollectRoyal());
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

	//Sets up PlayerUI images
	static void SetupFaces() {
		faces = new Dictionary<string, Sprite>();
		object[] facepics = Resources.LoadAll("PlayerUI", typeof(Sprite));
		for(int i = 0; i < facepics.Length; i++) {
			Sprite nextPic = (Sprite) facepics[i];
			faces.Add(nextPic.name, nextPic);
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
		players = new GameObject[playerNames.Length];
		for(int i = 0; i < players.Length; i++) {
			GameObject nextPlayer = Instantiate(playerObject);
			nextPlayer.GetComponent<Player>().SetupPlayer(i, playerNames[i]);
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
		for(int i = 0; i < deckLength - deckLength%players.Length; i++) {
			players[(i%players.Length)].GetComponent<Player>().GetCard((Card) deck.Dequeue());
		}
		deckLength = deck.Count;
		for(int i = 0; i < deckLength; i++)
			BurnCard((Card) deck.Dequeue());
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
		Card c = (Card) next;
		SlapCycle(c);
		CreateCard(c);
		if(c.isRoyal()) {
			royalPlayer = id;
			royalCount = c.RoyalValue();
		}

	}

	//Adds a card to the Burn Pile
	public static void BurnCard(object next) {
		if(royalCollecting)
			royalBurn.Enqueue(next);
		else
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
		float height = m.card.GetComponent<BoxCollider2D>().transform.lossyScale.y; // m.card.transform.lossyScale.y
		GameObject nextCard = (GameObject) Instantiate(m.card, new Vector2(m.card.transform.position.x, (tableau.Count - 1) * height), Quaternion.identity);
		nextCard.GetComponent<SpriteRenderer>().sprite = CardSkin(next);
		targetPos = (new Vector2(0, stackHeight() * height + startPos.y));
	}

	static int stackHeight() {
		int height = tableau.Count - m.stackHeightSize;
		if(height < 0)
			return 0;
		return height;
	}

	//Gives the Stack and Burned Cards to the Royal Player after a period of time
	static IEnumerator CollectRoyal() {
		royalCollecting = true;
		royalCount = -1;
		yield return new WaitForSeconds(m.breathTime);
		if(HasRoyalPlayer()) {
			if(!PlayerAt(royalPlayer).isAlive())
				Distribute();
			else
				Collect(royalPlayer);
		}
		while(collecting)
			yield return null;
		royalCollecting = false;
	}

	//Gives the Stack and Burned Cards to the Passed player
	public static void Collect(int playerID) {
		collecting = true;
		ClearRound();
		int burnCount = burn.Count;
		int stackCount = tableau.Count;
		int royalBurnCount = royalBurn.Count;
		for(int i = 0; i < burnCount; i++) {
			GiveCard(playerID, burn.Pop());
		}
		for(int i = 0; i < stackCount; i++) {
			GiveCard(playerID, tableau.Dequeue());
		}
		for(int i = 0; i < royalBurnCount; i++) {
			burn.Push(royalBurn.Dequeue());
		}
		for(int i = 0; i < players.Length; i++) {
			if(!PlayerAt(i).HasCards())
				PlayerAt(i).Lose();
		}
		GameObject[] allCards = GameObject.FindGameObjectsWithTag("Card");
		foreach(GameObject o in allCards)
			Destroy(o);
		currentPlayer = playerID;
		collecting = false;
	}

	//Clears information for the next round
	static void ClearRound() {
		currentPlayer = -1;
		royalPlayer = -1;
		royalCount = -1;
		slapCheck = new Card[] {new Card(-1, -1), new Card(-1, -2), new Card(-1, -3)};
		targetPos = startPos;
	}

	//Deals All Stack and Burn cards to Alive Players
	static void Distribute() {
	}

	//Checks if the Game is Ended
	static void CheckEndGame() {
		if(PlayerHasWon() != -1)
			Win(PlayerHasWon());
		else if(!PlayersHaveCards() && !SlapValid() && !royalCollecting)
			Tie();
	}

	//Signals a Tie Game
	static void Tie() {
		currentPlayer = -1;
		gameOver = true;
		Debug.Log("Tie");
	}

	//Signals a Win for a Player
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

	//Displays the UI Labels for the Players
	static void LabelPlayers() {
		for(int i = 0; i < players.Length; i++) {
			ImageLabel(i);
			TextLabel(i);
		}
	}

	//UI Player Image
	static void ImageLabel(int i) {
		Player current = PlayerAt(i);
		string key = current.Label();
		if(!current.isAlive())
			key += "Dead";
		else if(!current.HasCards())
			key += "Weak";
		current.GetComponent<Image>().sprite = faces[key];
	}                            

	//UI Player Text
	static void TextLabel(int i) {
		string text = "";
		if(currentPlayer == i && PlayerAt(i).HasCards() && PlayerAt(i).isAlive())
			text += "*";
		if(PlayerAt(i).isAlive())
			text += " Cards: " + PlayerAt(i).CardCount();
		else
			text += " X_X";
		Text t = (Text) PlayerAt(i).gameObject.GetComponentInChildren(typeof(Text));
		t.text = text;
	}

	//Returns if the Game is Over
	public static bool GameOver() {
		return gameOver;
	}

	//Returns if there is a current Royal Player
	public static bool HasRoyalPlayer() {
		return royalPlayer != -1;
	}

	//Decreases the royal count quota
	public static void DecreaseRoyalCount() {
		royalCount = royalCount - 1;
	}

	//Returns true if in collecting phase
	public static bool isCollecting() {
		return collecting;
	}

	//Returns true if waithing for royal collecting
	public static bool isRoyalCollecting() {
		return royalCollecting;
	}
}
