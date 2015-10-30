using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	string playerName;
	int id;
	Queue cards;

	public Player(string playerName, int id) {
		this.id = id;
		this.playerName = playerName;
		cards = new Queue();
	}



}
