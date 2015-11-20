using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

	public static AudioManager instance;
	public AudioClip[] sfx;
	AudioSource source;

	void Awake() {
		instance = this;
		source = gameObject.GetComponent<AudioSource>();
	}

	public void playSound(int i) {
		source.pitch = 1 + Random.value;
		source.PlayOneShot(sfx[i]);
	}
}
