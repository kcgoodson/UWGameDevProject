using UnityEngine;
using System.Collections;

public class RandomDeck : MonoBehaviour {
	private ArrayList deck;
	private Queue p1Cards;
	private Queue p2Cards;
	
	// Use this for initialization
	void Start () {
		deck = new ArrayList ();
		p1Cards = new Queue ();
		p2Cards = new Queue ();
		
		//populates the deck with cards
		for (int i = 2; i < 15; i++) {
			for(int j = 0; j < 4; j++) {
				deck.Add(i);
			}
		}

		//shuffles the deck into two random player queues
		while (deck.Count > 0) {
			int num = Random.Range(0, deck.Count);
			if(deck.Count % 2 == 0) {
				p1Cards.Enqueue(deck[num]);
			} else {
				p2Cards.Enqueue(deck[num]);
			}
			deck.RemoveAt(num);
		}
	}
}
