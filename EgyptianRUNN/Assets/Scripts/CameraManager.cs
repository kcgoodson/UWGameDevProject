using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	public static CameraManager instance;
	public float strength;

	void Awake() {
		instance = this;
	}

	public void Shake(float time) {
		StartCoroutine(shakeRoutine(time));
	}

	IEnumerator shakeRoutine(float time) {
		Vector2 startPos = transform.position;

		while (time > 0) {
			time -= Time.deltaTime * 10;
			transform.position = startPos + Random.insideUnitCircle * strength/8;
			yield return null;
		}

		transform.position = startPos;

	}
}
