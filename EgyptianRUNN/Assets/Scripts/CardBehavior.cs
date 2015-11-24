using UnityEngine;
using System.Collections;

public class CardBehavior : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		Vector3 pos = transform.position;
		transform.position = new Vector3(pos.x, pos.y, pos.y);
	}
}
