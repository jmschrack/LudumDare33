using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

[TaskDescription("Finds the closest GameObject in the list of GameObjects")]
[TaskCategory("Movement")]

public class ElectLeader : Action
{
	public SharedTransformList currentLeaders;
	public float neighborRange;

	// Use this for initialization
	public override void OnStart ()
	{

	}
	
	// Update is called once per frame
	public override TaskStatus OnUpdate ()
	{
		GameObject[] goList = GameObject.FindGameObjectsWithTag ("NPC");
		List<GameObject> filteredList = new List<GameObject> ();

		//Filter the list down to those within range
		for (int i = 0; i < goList.Length; i++) {
			if (Mathf.Abs (Vector3.Distance (transform.position, goList [i].transform.position)) <= neighborRange) {
				filteredList.Add (goList [i]);
			}
		}

		//Remove any current leaders.
		foreach (GameObject go in filteredList) {
			currentLeaders.Value.Remove (go.transform);
		}

		//Elect a new leader.
		int choice = Random.Range (0, filteredList.Count);
		GameObject theLeader = filteredList [choice];
		currentLeaders.Value.Add (theLeader.transform);
		BehaviorTree bt = theLeader.GetComponent<BehaviorTree> ();
		SharedBool isLeader = (SharedBool)bt.GetVariable ("isLeader");
		isLeader.Value = true;

		//Tell others to follow you from now on
		foreach (GameObject go in filteredList) {
			Debug.Log ("test1");
			if (go == theLeader) {
				continue;
			}
			Debug.Log ("test2");
			BehaviorTree tree = go.GetComponent<BehaviorTree> ();
			SharedTransform leaderT = (SharedTransform)tree.GetVariable ("leader");
			leaderT.Value = filteredList [choice].transform;

		}
		return TaskStatus.Success;
	}
}
