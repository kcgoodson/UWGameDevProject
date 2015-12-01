using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Dummy : MonoBehaviour {
    public static Dummy d;
    public Image screen;
    public bool test;

    //
    void Awake() {
        if (d == null) {
            d = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
        test = true;
    }

    //
    void update()
    {
        if (Application.loadedLevel.Equals("Main Game"))
        {
            Pause();
        }
    }

    //
    public void NextLevel() {
        string[] names;
        string playertag2 = GameObject.Find("3 Player Button").tag;
        if (GameObject.Find("2 Player Button").tag.Equals("Selected")) {
            names = new string[2];
        } else if (playertag2.Equals("Selected")) {
            names = new string[3];
        } else {
            names = new string[4];
        }
        names[0] = GameObject.FindGameObjectWithTag("Player1").name;
        names[1] = GameObject.FindGameObjectWithTag("Player2").name;
        if (playertag2.Equals("Selected")) {
            names[2] = GameObject.FindGameObjectWithTag("Player3").name;
        } else if (GameObject.Find("4 Player Button").tag.Equals("Selected")) {
            names[3] = GameObject.FindGameObjectWithTag("Player4").name;
        }
		Application.LoadLevel(1);
		d.StartCoroutine(GoToGame(names));
    }

	IEnumerator GoToGame(string[] names){
		while(Application.loadedLevel != 1)
			yield return null;
		GameManager.BeginGame(names);
	}

    //
    public void Pause() {
        GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");
        Image g = Instantiate(screen) as Image;
        g.name = "test";
        RectTransform rectTrans = g.GetComponent<RectTransform>();
        rectTrans.position = new Vector2((Screen.width - rectTrans.rect.width) / 2, ((Screen.height - rectTrans.rect.height) / 2));
        g.transform.SetParent(canvas.transform);
        Debug.Log("test");
        test = false;
    }
}
