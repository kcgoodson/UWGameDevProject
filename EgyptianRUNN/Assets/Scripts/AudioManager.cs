using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour {

	public static AudioManager m;
	public string[] audioClipNames;
	public AudioClip[] soundClips;
	static Dictionary<string, AudioClip> sfx;
	public Dictionary<string, int> d;
	static AudioSource source;

	void Awake() {
		m = this;
		source = m.GetComponent<AudioSource>();
		SetupDictionary();
	}

	static void SetupDictionary() {
		sfx = new Dictionary<string, AudioClip>();
		for(int i = 0; i < m.audioClipNames.Length; i++) {
			sfx.Add(m.audioClipNames[i], m.soundClips[i]);
		}
	}

	public static void playSound(string name) {
		source.PlayOneShot(sfx[name]);
	}

	//Deal Sound (x4)?
	//Collect Sound (x4)?
	//Burn Sound (x4)?
	//Win Sound
	//Lose Sound

}
