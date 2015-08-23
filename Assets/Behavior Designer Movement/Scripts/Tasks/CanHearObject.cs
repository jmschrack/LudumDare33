using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Tasks.Movement
{
	[TaskDescription("Check to see if the any objects are within hearing range of the current agent.")]
	[TaskCategory("Movement")]
	[HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Movement/documentation.php?id=12")]
	[TaskIcon("Assets/Behavior Designer Movement/Editor/Icons/{SkinColor}CanHearObjectIcon.png")]
	public class CanHearObject : Conditional
	{
		[Tooltip("The object that we are searching for. If this value is null then the objectLayerMask will be used")]
		public SharedTransform
			targetObject;
		[Tooltip("The LayerMask of the objects that we are searching for")]
		public LayerMask
			objectLayerMask;
		[Tooltip("How far away the unit can hear")]
		public float
			hearingRadius = 50;
		[Tooltip("The furtuer away a sound source is the less likely the agent will be able to hear it. " +
                 "Set a threshold for the the minimum audibility level that the agent can hear")]
		public float
			linearAudibilityThreshold = 0.05f;
		[Tooltip("The returned object that is heard")]
		public SharedTransform
			objectHeard;

		// Returns success if an object was found otherwise failure
		public override TaskStatus OnUpdate ()
		{
			// If the target object is null then determine if there are any objects within hearing range based on the layer mask
			if (targetObject.Value == null) {
				objectHeard.Value = MovementUtility.WithinHearingRange (transform, linearAudibilityThreshold, hearingRadius, objectLayerMask);
			} else { // If the target is not null then determine if that object is within sight
				objectHeard.Value = MovementUtility.WithinHearingRange (transform, linearAudibilityThreshold, targetObject.Value);
			}
			if (objectHeard.Value != null) {
				// Return success if an object was heard
				return TaskStatus.Success;
			}
			// An object is not within heard so return failure
			return TaskStatus.Failure;
		}

		// Reset the public variables
		public override void OnReset ()
		{
			hearingRadius = 50;
			linearAudibilityThreshold = 0.05f;
		}
		
	}
}