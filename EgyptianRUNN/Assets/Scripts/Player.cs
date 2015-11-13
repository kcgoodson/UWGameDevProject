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
			if(Input.GetButtonDown ("FLIP" + (id + 1))) {
				DealCard();
				GameManager.NextPlayer();
			} else if (Input.GetButtonDown("SLAP" + (id + 1))) {
				if(GameManager.SlapValid()) {
					GameManager.Collect(id);
				} else {
					BurnCard();
				}
			}

		}
	}

	//Recieves a Card
	public void GetCard(Card next) {
		cards.Enqueue(next);
	}

	//Gives out a Card
	void DealCard() {
		GameManager.StackCard(cards.Dequeue());
	}

	void BurnCard() {
		GameManager.BurnCard(cards.Dequeue());
	}
}
