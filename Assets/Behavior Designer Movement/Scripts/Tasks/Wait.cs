using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskDescription("Waits for the specified amount of time")]
[TaskCategory("Movement")]

public class Wait : Action
{
	public float waitTime;
	private float timeWaited = 0f;
	// Use this for initialization
	public override void OnStart ()
	{
		timeWaited = 0f;
	}
	
	// Update is called once per frame
	public override TaskStatus OnUpdate ()
	{
		timeWaited += Time.deltaTime;
		if (timeWaited >= waitTime) {
			return TaskStatus.Success;
		} else {
			return TaskStatus.Running;
		}
	}
}
