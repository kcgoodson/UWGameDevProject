using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayButton : MonoBehaviour {

    private GameObject PlayerButton2;
    private GameObject PlayerButton3;
    private GameObject PlayerButton4;
    private GameObject Player1;
    private GameObject Player2;
    private GameObject Player3;
    private GameObject Player4;
    private GameObject Playbutton;

    // Use this for initialization
    void Start () {
        PlayerButton2 = GameObject.Find("2 Player Button");
        PlayerButton3 = GameObject.Find("3 Player Button");
        PlayerButton4 = GameObject.Find("4 Player Button");
        Player1 = GameObject.Find("Player 1");
        Player2 = GameObject.Find("Player 2");
        Player3 = GameObject.Find("Player 3");
        Player4 = GameObject.Find("Player 4");
        Playbutton = GameObject.Find("Play Button");
        Player1.SetActive(false);
        Player2.SetActive(false);
        Player3.SetActive(false);
        Player4.SetActive(false);
        Playbutton.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (Player1.tag.Equals("Selected") && Player2.tag.Equals("Selected")) {
            if (PlayerButton2.tag.Equals("Selected") || (Player3.tag.Equals("Selected") && 
                (PlayerButton3.tag.Equals("Selected") || Player4.tag.Equals("Selected") && 
                PlayerButton4.tag.Equals("Selected")))) {
                Playbutton.SetActive(true);
            } else {
                Playbutton.SetActive(false);
            }
        }
	}
}
