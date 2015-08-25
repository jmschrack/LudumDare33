using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;

public class NPC : MonoBehaviour
{

	BehaviorTree bt;
	Victim victim;

	// Use this for initialization
	void Start ()
	{
		bt = GetComponent<BehaviorTree> ();
		bt.EnableBehavior ();
		victim = GetComponent<Victim> ();

	}

	public void Seen ()
	{
		//victim.seePlayer (true);
	}

	public void NotSeen ()
	{
		//victim.seePlayer (false);
	}
	public void FireWeapon (Transform target)
	{
		Debug.Log ("Weapong Fired!");
		victim.shoot ();
	}
	// Update is called once per frame
	void Update ()
	{
	
	}
}
