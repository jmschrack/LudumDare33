using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

[TaskDescription("Finds the first GameObject in the list of GameObjects outside of neighbor range")]
[TaskCategory("Basic/GameObject")]

public class GetFirstNonNeighbor : Action
{
	public SharedGameObjectList gameObjectList;
	public SharedGameObject target;
	public SharedInt neighborRange;

	// Update is called once per frame
	public override TaskStatus OnUpdate ()
	{
		Vector3 position = transform.position;
		List<GameObject> list = gameObjectList.Value;
		

		foreach (GameObject go in list) {
			float d = Mathf.Abs (Vector3.Distance (position, go.transform.position));
			if (d > neighborRange.Value) {
				target.SetValue (go);
				break;
			}
		}
		return TaskStatus.Success;
	}
}
