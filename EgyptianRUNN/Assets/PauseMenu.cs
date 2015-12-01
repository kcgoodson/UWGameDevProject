using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour {
    GameObject PauseScreen;
    GameObject WinScreen;

	// Use this for initialization
	void Start () {
        PauseScreen = GameObject.Find("Pause Screen");
        WinScreen = GameObject.Find("Win Screen");
        PauseScreen.SetActive(false);
        WinScreen.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetButtonDown("ESC")) {
            PauseScreen.SetActive(true);
        }
	}

    // Loads the Menu scene
    public void Exit() {
        Application.LoadLevel("Menu");
    }

    // Displays the given string in the victory panel
    public void End(string tx) {
        WinScreen.SetActive(true);
        GameObject.Find("End Text").GetComponent<GUIText>().text = tx;
    }
}
