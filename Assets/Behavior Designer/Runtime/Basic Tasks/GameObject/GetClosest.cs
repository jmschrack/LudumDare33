using UnityEngine;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskDescription("Finds the closest GameObject in the list of GameObjects")]
[TaskCategory("Basic/GameObject")]

public class GetClosest : Action
{
	public SharedGameObjectList gameObjectList;
	public SharedGameObject closestTarget;
	

	// Update is called once per frame
	public override TaskStatus OnUpdate ()
	{
		Vector3 position = transform.position;
		List<GameObject> list = gameObjectList.Value;

		float distance = -1f;
		foreach (GameObject go in list) {
			float d = Mathf.Abs (Vector3.Distance (position, go.transform.position));
			if (d < distance || distance == -1f) {
				distance = d;
				closestTarget.SetValue (go);
			}
		}
		return TaskStatus.Success;
	}
}
