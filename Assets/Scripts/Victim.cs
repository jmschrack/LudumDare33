using UnityEngine;
using System.Collections;

public class Victim : MonoBehaviour {
	private Transform player;
	private Villian v;
	public float fieldOfView;
	// Use this for initialization
	void Start () {
		player=GameObject.FindGameObjectsWithTag("Player")[0].transform;
		v=player.gameObject.GetComponentsInChildren<Villian>()[0];
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos=player.position-this.transform.position;
		v.isSpotted=Mathf.Abs(Vector3.Angle(this.transform.forward,pos))<fieldOfView;
		pos=v.cursor.transform.position-this.transform.position;
		v.isValidCursor=!(Mathf.Abs(Vector3.Angle(this.transform.forward,pos))<fieldOfView);
	}
}
