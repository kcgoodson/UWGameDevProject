using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {
    static GameObject PauseScreen;
    static GameObject WinScreen;
	public static bool isPaused;

	// Use this for initialization
	void Start () {
		isPaused = false;
        PauseScreen = GameObject.Find("Pause Screen");
        WinScreen = GameObject.Find("Win Screen");
        PauseScreen.SetActive(false);
        WinScreen.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		isPaused = PauseScreen.activeInHierarchy;
	    if(Input.GetButtonDown("ESC")) {
			isPaused = !isPaused;
           	PauseScreen.SetActive(isPaused);
        }
	}

    // Loads the Menu scene
    public void Exit() {
        Application.LoadLevel("Menu");
    }

    // Displays the given string in the victory panel
    public static void End(string tx) {
        WinScreen.SetActive(true);
        GameObject.Find("End Text").GetComponent<Text>().text = tx + " Wins!";
    }
}
