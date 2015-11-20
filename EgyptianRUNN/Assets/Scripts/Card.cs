using UnityEngine;
using System.Collections;

public class Card {

	int suit;
	int rank;
	int royal;

	public Card(int suit, int rank) : this(suit, rank, 0){}

	public Card(int suit, int rank, int royal) {
		this.suit = suit;
		this.rank = rank;
		this.royal = royal;
	}

	public int Suit() {
		return suit;
	}

	public int Rank() {
		return rank;
	}

	public int RoyalValue() {
		return royal;
	}

	public bool isRoyal () {
		return royal > 0;
	}

	public override string ToString(){
		return "Suit: " + suit + " Rank: " + rank + " Royal:" + royal;
	}
}
