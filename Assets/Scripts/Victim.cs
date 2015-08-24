using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;

public class Victim : MonoBehaviour
{
	private Transform player;
	private Villian v;
	public float fieldOfView;
	private bool lastSeenState = false;
	private bool lastSeenCursor = false;
	public bool isAlive = true;
	public int hitPoints = 5;
	public bool manualSight = false;
	private BehaviorTree bt;
	// Use this for initialization
	void Start ()
	{
		Debug.Log ("Searching for player");
		player = GameObject.FindGameObjectsWithTag ("Player") [0].transform;
		Debug.Log ("Acquriing villiany");
		v = player.gameObject.GetComponentsInChildren<Villian> () [0];
		Debug.Log (v != null);
		bt = GetComponent<BehaviorTree> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (isAlive) {
			Vector3 pos = player.position - this.transform.position;
			if (manualSight) {
				seePlayer (Mathf.Abs (Vector3.Angle (this.transform.forward, pos)) < fieldOfView);
			}
			pos = v.cursor.transform.position - this.transform.position;
			seeCursor ((Mathf.Abs (Vector3.Angle (this.transform.forward, pos)) < fieldOfView)&&pos.magnitude<25f);
		}
	}

	/*
	 * We do this to avoid a race condition in which a victim sees the villian, but another victim updates later and "doesn't see" the villian.
	 */ 
	public void seePlayer (bool seen)
	{
		if (seen) {
			v.isSpotted = lastSeenState = true;
		} else if (lastSeenState) {
			v.isSpotted = lastSeenState = false;
		}
	}

	void seeCursor (bool seen)
	{
		if (seen) {
			v.isValidCursor = lastSeenCursor = false;
		} else if (!lastSeenCursor) {
			v.isValidCursor = lastSeenCursor = true;

		}
	}

	public void pauseAI ()
	{

		if (bt != null) {
			bt.SendEvent ("Die");
		}
	}

	public void takeDamage (int damage)
	{
		hitPoints -= damage;
		if (hitPoints <= 0) {
			die ();
		}
	}

	void die ()
	{
		isAlive = false;
		//ded
		pauseAI ();
	}

}
