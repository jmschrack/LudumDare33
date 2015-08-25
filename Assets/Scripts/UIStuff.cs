using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UIStuff : MonoBehaviour {
	public int targetsLeft;
	public int respawn;
	public Villian vil;
	public Text detection;
	public Text targetText;
	public Text respawnText;
	public GameObject gameOverScreen;
	public GameObject gameWinScreen;
	// Use this for initialization
	void Start () {

		targetsLeft=GameObject.FindGameObjectsWithTag("NPC").Length;
	}
	
	// Update is called once per frame
	void Update () {
		if (targetsLeft < 1) {
			gameWin ();
		}
		targetText.text = "Bugs in code:" + targetsLeft;
		if (respawn < 1) {

		}
		if (!vil.isAlive) {
			death ();
		}
		if (vil.isSpotted) {
			detection.text = "!";
		} else {
			detection.text = "?";
		}
	}

	void death(){
		respawnText.text = vil.lives+"";
		gameOverScreen.active = true;
	}

	public void rekill(){
		if (vil.lives < 1) {
			return;
		}
		gameOverScreen.active = false;
		GameObject[] npcs=GameObject.FindGameObjectsWithTag ("NPC");
		GameObject npc=null;
		for (int i=0; i<npcs.Length; i++) {
			if(npcs[i].GetComponent<Victim>().isAlive){
				npc=npcs[i];
				break;
			}
		}

		Debug.Log ("Attempting to Rekill");
		if (npc != null) {
			npc.transform.position=vil.transform.position;
			vil.respawn();
			npc.GetComponent<Victim>().takeDamage(5);
		}
	}

	void gameWin(){
		gameWinScreen.SetActive (true);
	}

	public void startGame(){
		Application.LoadLevel ("Slasher");
	}
}
