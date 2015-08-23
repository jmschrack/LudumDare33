using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;

public class NPC : MonoBehaviour
{

	BehaviorTree bt;

	// Use this for initialization
	void Start ()
	{
		bt = GetComponent<BehaviorTree> ();
		bt.EnableBehavior ();

	}

	public void Seen ()
	{
		Debug.Log ("Player Seen!");
	}

	public void NotSeen ()
	{
		Debug.Log ("Player Lost!");
	}
	public void FireWeapon (Transform target)
	{
		Debug.Log ("Weapong Fired!");
	}
	// Update is called once per frame
	void Update ()
	{
	
	}
}
