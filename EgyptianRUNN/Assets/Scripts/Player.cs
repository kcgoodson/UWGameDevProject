using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	
	int id;
	Queue cards;
	bool alive;

	public void SetupPlayer(int id) {
		this.id = id;
		alive = true;
		cards = new Queue();
	}
	
	void Update() {
		if(GameManager.CurrentPlayerID() == id) {
			if(!HasCards())
				GameManager.NextPlayer();
			if(Input.GetButtonDown ("FLIP" + (id + 1))) {
				DealCard();
				GameManager.NextPlayer();
			}
		}
		if(isAlive() && !GameManager.GameOver() && Input.GetButtonDown("SLAP" + (id + 1))) {
			if(GameManager.SlapValid()) {
				GameManager.Collect(id);
			} else if(HasCards()){
				BurnCard();
			} else {
				Lose();
			}
		}
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
	}

	public bool isAlive() {
		return alive;
	}
}
