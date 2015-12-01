using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public int suits; //Default 4
	public int ranks; //Default 13
	public static string[] playerNames; //2 - 4
	public int royalRank; //Default 8
	public GameObject card; //Card Object
	public GameObject playerObject; //Player Objects
	public string[] playerTypes;//Define Selectable Characters
	public Color[] playerColors; //Define Corresponding Colors
	public float fadeSpeed; // For Cards Disappearing

	static Sprite[] cardSkins;
	static Dictionary<string, Sprite> faces;
	static Dictionary<string, Color> colors;

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

	public float inactiveFade;
	public float weakFadeMin;
	public float weakFadeSpeed;

	static float weakFade = 0.3f;
	static bool weakUp = false;

	public float playerImageSize;
	public Vector2 barSize;
	public float[] barLocInfo;
	static Texture2D staticRectTexture;

	static bool gameNotStarted;

	void Awake() {
		m = this.gameObject.GetComponent<GameManager>();
		gameNotStarted = true;
	}

	public static void BeginGame(string[] playerLabels) {
		AudioManager.Stop();
		m.LoadGame(playerLabels);
	}

	// Use this for initialization
	public void LoadGame (string[] playerLabels) {
		staticRectTexture = new Texture2D( 1, 1 );
		playerNames = playerLabels;
		Queue deck = Shuffle(InitialDeck(suits, ranks));
		tableau = new Queue();
		burn = new Stack();
		royalBurn = new Queue();
		SetupSkins();
		SetupFaces();
		SetupPlayers();
		DealCards(deck);
		if(startPos == Vector3.zero)
			startPos = transform.position;
		ClearRound();
		currentPlayer = (int) Random.Range(0, playerNames.Length);
		gameOver = false;
		gameNotStarted = false;
	}

	void OnGUI() {
		if(gameNotStarted)
			return;
		GUI.skin.box = new GUIStyle();
		LabelPlayers();
	}

	void Update() {
		if(gameNotStarted)
			return;
		CheckEndGame();
		transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10);
		WeakFadeUpdate();
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
		colors = new Dictionary<string, Color>();
		for(int i = 0; i < m.playerColors.Length; i++) {
			Color pColor = m.playerColors[i];
			pColor.a = 1;
			colors.Add(m.playerTypes[i], pColor);
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
		AudioManager.playSound("burn");
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
		float height = m.card.GetComponent<BoxCollider2D>().transform.lossyScale.y * .965f;
		GameObject nextCard = (GameObject) Instantiate(m.card, new Vector2(m.card.transform.position.x, (tableau.Count - 1) * (height)), Quaternion.identity);
		AudioManager.playSound("deal");
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
		m.StartCoroutine(CollectProcess(playerID));
	}

	//Collect IEnumerator
	static IEnumerator CollectProcess(int playerID) {
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
			Player n = PlayerAt(i);
			if(!n.HasCards() && n.isAlive())
				PlayerAt(i).Lose();
		}
		GameObject[] allCards = GameObject.FindGameObjectsWithTag("Card");
		Color winColor = colors[PlayerAt(playerID).Label()];
		winColor.a = 1;
		AudioManager.playSound("collect");
		while(winColor.a > 0) {
			winColor.a -= Time.deltaTime * m.fadeSpeed;
			foreach(GameObject o in allCards) {
				o.GetComponent<SpriteRenderer>().color = winColor;
			}
			yield return null;
		}
		foreach(GameObject o in allCards) {
			Destroy(o);
		}
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
		if(!collecting && !gameOver) {
			if(PlayerHasWon() != -1)
				Win(PlayerHasWon());
			else if(!PlayersHaveCards() && !SlapValid() && !isRoyalCollecting() && royalCount != 0)
				Tie();
		}
	}

	//Signals a Tie Game
	static void Tie() {
		currentPlayer = -1;
		gameOver = true;
		AudioManager.playSound("tie");
		PauseMenu.End("Tie");
	}

	//Signals a Win for a Player
	static void Win(int playerID) {
		m.StartCoroutine(WinProcess(playerID));
	}

	static IEnumerator WinProcess(int ID) {
		currentPlayer = -1;
		gameOver = true;
		yield return new WaitForSeconds(1);
		AudioManager.playSound("win");
		PauseMenu.End(PlayerAt(ID).Label());
	}

	//Returns true if at least on player has cards
	public static bool PlayersHaveCards() {
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
			//TextLabel(i);
		}
	}

	//UI Player Image
	static void ImageLabel(int i) {
		Player current = PlayerAt(i);
		Image image = current.GetComponent<Image>();
		RectTransform rectTrans = current.GetComponent<RectTransform>();
		rectTrans.sizeDelta = GUIHelper(m.playerImageSize, m.playerImageSize);
		rectTrans.position = new Vector2((Screen.width - rectTrans.rect.width) * (i % 2), ((Screen.height - rectTrans.rect.height) * ((3 - i) / 2)));
		string key = current.Label();
		Color color = colors[key];
		color.a = m.inactiveFade;
		if(!current.isAlive()) {
			color = colors["Dead"];
			color.a = 0.3f;
		} else if(!current.HasCards()) {
			//color = new Color(color.r, color.g, color.b, weakFade);
			color.a = weakFade;
		} else if(i == currentPlayer || i == PlayerHasWon()) {
			//color = new Color(color.r, color.g, color.b, 1);
			color.a = 1;
		}
		image.sprite = faces[key];
		image.color = new Color(255, 255, 255, color.a);
		GUI.color = color;
		Vector2 rLoc = new Vector2((rectTrans.position.x), sw(1) - rectTrans.position.y);
		Vector2 rSize = (Vector2) rectTrans.sizeDelta;
		for(int j = 0; j < current.CardCount(); j++) {
			float x = (rLoc.x + rSize.x) + sw(m.barLocInfo[0]) + (j/26) * (sw(m.barSize.x + m.barLocInfo[0]));
			if(i%2 == 1) {
				x = rLoc.x - (1 + j/26) * (sw(m.barSize.x + m.barLocInfo[0]));
			}
			float y = rLoc.y + sw(m.barLocInfo[1]) - sw(m.barLocInfo[2] / 100 * j) + (j/26) * rSize.y;
			float w = sw(m.barSize.x);
			float h = sw(m.barSize.y);
			GUI.DrawTexture(new Rect(x, y, w, h), staticRectTexture);
		}
	}   

	//UI Player Text
	static void TextLabel(int i) {
		string text = "";
		if(PlayerAt(i).isAlive())
			text += " Cards: " + PlayerAt(i).CardCount();
		Text t = (Text) PlayerAt(i).gameObject.GetComponentInChildren(typeof(Text));
		t.text = text;
	}

	static void WeakFadeUpdate() {
		if(weakFade >= m.inactiveFade) {
			weakUp = false;
			weakFade = m.inactiveFade;
		}
		else if(weakFade <= m.weakFadeMin) {
			weakUp = true;
			weakFade = m.weakFadeMin;
		}
		if(weakUp)
			weakFade += Time.deltaTime * m.weakFadeSpeed;
		else
			weakFade -= Time.deltaTime * m.weakFadeSpeed;
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

	static Rect GUIHelper(float x, float y, float w, float h) {
		float s = Screen.width / 100;
		return new Rect(x * s, y * s, w * s, h * s);
	}

	static float sw(float n) {
		return n * Screen.width;
	}

	static Vector2 GUIHelper(float x, float y) {
		return new Vector2(x * Screen.width, y * Screen.width);
	}

	public void EndGame() {
		Application.LoadLevel(0);
	}

	public void RestartGame() {
		GameObject[] allCards = GameObject.FindGameObjectsWithTag("Card");
		foreach(GameObject o in allCards) {
			Destroy(o);
		}
		ClearRound();
		BeginGame(playerNames);
		PauseMenu.HideScreens();

	}
	
}
