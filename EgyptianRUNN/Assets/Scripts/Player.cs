using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	string playerName;
	int id;
	Queue cards;

	public void SetupPlayer(string playerName, int id) {
		this.id = id;
		this.playerName = playerName;
		cards = new Queue();
	}

	void Update() {
		if(GameManager.CurrentPlayer() == id) {
			Debug.Log (playerName);
			if(Input.GetButtonDown("SWITCH" + id))
				GameManager.NextPlayer();
			if(Input.GetKeyDown (KeyCode.Space)) {
				int length = cards.Count;
				for(int i = 0; i < length; i++) {
					GameManager.StackCard(cards.Dequeue());
				}
			}

		}
	}

	//Recieves a Card
	public void GetCard(Card next) {
		cards.Enqueue(next);
	}

	//Gives out a Card
	public void DealCard() {
		GameManager.StackCard(cards.Dequeue());
	}

	public void BurnCard() {
		GameManager.BurnCard(cards.Dequeue());
	}
}
