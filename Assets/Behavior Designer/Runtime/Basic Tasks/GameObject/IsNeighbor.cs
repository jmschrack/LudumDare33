using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskDescription("Finds the first GameObject in the list of GameObjects outside of neighbor range")]
[TaskCategory("Basic/GameObject")]

public class IsNeighbor : Conditional
{

	public SharedTransform target;
	public float neighborRange;

	// Use this for initialization
	// Update is called once per frame
	public override TaskStatus OnUpdate ()
	{
		if (Mathf.Abs (Vector3.Distance (transform.position, target.Value.position)) <= neighborRange) {
			return TaskStatus.Success;
		} else {
			return TaskStatus.Failure;
		}
	}
}
