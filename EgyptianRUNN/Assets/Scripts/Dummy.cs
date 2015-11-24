using UnityEngine;
using System.Collections;

public class Dummy : MonoBehaviour {

    //calls the loadgame function in the GameManager script
    public void getGameManager() {
        GameManager.instance.LoadGame();
    }
}
