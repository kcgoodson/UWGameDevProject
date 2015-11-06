using UnityEngine;
using System.Collections;

public class Card {

	int suit;
	int rank;

	public Card(int suit, int rank) {
		this.suit = suit;
		this.rank = rank;
	}

	public int Suit() {
		return suit;
	}

	public int Rank() {
		return rank;
	}

	public override string ToString(){
		return "Suit: " + suit + " Rank: " + rank;
	}
}
