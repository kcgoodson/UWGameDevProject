using UnityEngine;
using System.Collections;

public class CardBehavior : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if(transform.position.x != 0)
			transform.position = new Vector2(0, transform.position.y);
	}
}
