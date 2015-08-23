using UnityEngine;
using System.Collections;

public class DebugKeys : MonoBehaviour {
	private Villian v;
	// Use this for initialization
	void Start () {
		v = GameObject.FindGameObjectWithTag ("Player").GetComponentInChildren<Villian> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.O)) {
			Debug.Log ("Debug:Shoot");
			v.takeDamage(1);
		}
	}
}
