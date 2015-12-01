using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	
	int id;
	string label;
	Queue cards;
	bool alive;

	public void SetupPlayer(int id, string label) {
		this.id = id;
		this.label = label;
		alive = true;
		cards = new Queue();
	}
	
	void Update() {
		if(PauseMenu.isPaused)
			return;
		if(GameManager.CurrentPlayerID() == id) {
			if(!HasCards())
				GameManager.NextPlayer();
			if(Input.GetButtonDown("FLIP" + (id + 1)) && !GameManager.isRoyalCollecting() && !GameManager.GameOver()) {
				Card next = (Card) cards.Peek();
				DealCard();
				if(!GameManager.HasRoyalPlayer() || next.isRoyal()) {
					GameManager.NextPlayer();
				} else {
					GameManager.DecreaseRoyalCount();
				}
			}
		}
		if(isAlive() && !GameManager.GameOver() && !GameManager.isCollecting() && Input.GetButtonDown("SLAP" + (id + 1))) {
			if(GameManager.SlapValid()) {
				GameManager.Collect(id);
			} else if(HasCards()){
				BurnCard();
			} else{
				Lose();
			}
		}
	}

	//Returns Player Label
	public string Label() {
		return label;
	}
	
	//Recieves a Card
	public void GetCard(Card next) {
		cards.Enqueue(next);
	}

	//Gives out a Card to the Game Stack
	void DealCard() {
		if(HasCards())
			GameManager.StackCard(cards.Dequeue(), id);
	}

	//Gives a Card to the Burn Stack
	void BurnCard() {
		if(HasCards())
			GameManager.BurnCard(cards.Dequeue());
	}

	//The Player's Deck Count
	public int CardCount() {
		return cards.Count;
	}

	public bool HasCards() {
		return cards.Count > 0;
	}

	public void Lose() {
		alive = false;
		AudioManager.playSound("lose");
	}

	public bool isAlive() {
		return alive;
	}
}
