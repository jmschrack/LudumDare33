using UnityEngine;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Tasks.Movement
{
	[TaskDescription("Check to see if the any objects are within sight of the agent.")]
	[TaskCategory("Movement")]
	[HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Movement/documentation.php?id=11")]
	[TaskIcon("Assets/Behavior Designer Movement/Editor/Icons/{SkinColor}CanSeeObjectIcon.png")]
	public class CanSeeObject : Conditional
	{
		[Tooltip("The object that we are searching for. If this value is null then the objectLayerMask will be used")]
		public SharedTransform
			targetObject;
		[Tooltip("The LayerMask of the objects that we are searching for")]
		public LayerMask
			objectLayerMask;
		[Tooltip("The field of view angle of the agent (in degrees)")]
		public float
			fieldOfViewAngle = 90;
		[Tooltip("The distance that the agent can see ")]
		public float
			viewDistance = 1000;
		[Tooltip("The object that is within sight")]
		public SharedTransform
			objectInSight;

		// Returns success if an object was found otherwise failure
		public override TaskStatus OnUpdate ()
		{
			// If the target object is null then determine if there are any objects within sight based on the layer mask
			if (targetObject.Value == null) {
				objectInSight.Value = MovementUtility.WithinSight (transform, fieldOfViewAngle, viewDistance, objectLayerMask);
			} else { // If the target is not null then determine if that object is within sight
				objectInSight.Value = MovementUtility.WithinSight (transform, fieldOfViewAngle, viewDistance, targetObject.Value);
			}
			if (objectInSight.Value != null) {
				// Return success if an object was found
				return TaskStatus.Success;
			}
			// An object is not within sight so return failure
			return TaskStatus.Failure;
		}

		// Reset the public variables
		public override void OnReset ()
		{
			fieldOfViewAngle = 90;
			viewDistance = 1000;
		}


	}
}