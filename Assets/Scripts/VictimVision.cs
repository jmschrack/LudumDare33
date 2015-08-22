using UnityEngine;
using System.Collections;

public class VictimVision : MonoBehaviour {
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
		if (Mathf.Abs(Vector3.Angle(this.transform.forward,pos))<fieldOfView)
		{
			//Debug.Log("Spotted");
		}else{
			//Debug.Log("Invis");
		}
		v.isSpotted=Mathf.Abs(Vector3.Angle(this.transform.forward,pos))<fieldOfView;
			pos=v.cursor.transform.position-this.transform.position;
		v.isValidCursor=!(Mathf.Abs(Vector3.Angle(this.transform.forward,pos))<fieldOfView);
	}
}
