using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Movement
{
	public static class MovementUtility
	{
		// Cast a sphere with the desired distance. Check each collider hit to see if it is within the field of view. Set objectFound
		// to the object that is most directly in front of the agent
		public static Transform WithinSight (Transform transform, float fieldOfViewAngle, float viewDistance, LayerMask objectLayerMask)
		{
			Transform objectFound = null;
			var hitColliders = Physics.OverlapSphere (transform.position, viewDistance, objectLayerMask);
			if (hitColliders != null) {
				float minAngle = Mathf.Infinity;
				for (int i = 0; i < hitColliders.Length; ++i) {
					float angle;
					Transform obj;
					// Call the WithinSight function to determine if this specific object is within sight
					if ((obj = WithinSight (transform, fieldOfViewAngle, viewDistance, hitColliders [i].transform, out angle)) != null) {
						// This object is within sight. Set it to the objectFound GameObject if the angle is less than any of the other objects
						if (angle < minAngle) {
							minAngle = angle;
							objectFound = obj;
						}
					}
				}
			}
			return objectFound;
		}

		// Public helper function that will automatically create an angle variable that is not used. This function is useful if the calling call doesn't
		// care about the angle between transform and targetObject
		public static Transform WithinSight (Transform transform, float fieldOfViewAngle, float viewDistance, Transform targetObject)
		{
			float angle;
			return WithinSight (transform, fieldOfViewAngle, viewDistance, targetObject, out angle);
		}

		// Determines if the targetObject is within sight of the transform. It will set the angle regardless of whether or not the object is within sight
		private static Transform WithinSight (Transform transform, float fieldOfViewAngle, float viewDistance, Transform targetObject, out float angle)
		{
			// The target object needs to be within the field of view of the current object
			var direction = targetObject.position - transform.position;
			angle = Vector3.Angle (direction, transform.forward);
			if (direction.magnitude < viewDistance && angle < fieldOfViewAngle * 0.5f) {
				RaycastHit hit;
				// The hit agent needs to be within view of the current agent
				if (Physics.Linecast (transform.position, targetObject.position, out hit)) {
					if (hit.transform.Equals (targetObject)) {
						return targetObject; // return the target object meaning it is within sight
					}
				} else {
					// If the linecast doesn't hit anything then that the target object doesn't have a collider and there is nothing in the way
					return targetObject;
				}
			}
			// return null if the target object is not within sight
			return null;
		}

		// Cast a sphere with the desired radius. Check each object's audio source to see if audio is playing. If audio is playing
		// and its audibility is greater than the audibility threshold then return the object heard
		public static Transform WithinHearingRange (Transform transform, float linearAudibilityThreshold, float hearingRadius, LayerMask objectLayerMask)
		{
			Transform objectHeard = null;
			var hitColliders = Physics.OverlapSphere (transform.position, hearingRadius, objectLayerMask);
			if (hitColliders != null) {
				float maxAudibility = 0;
				for (int i = 0; i < hitColliders.Length; ++i) {
					float audibility = 0;
					Transform obj;
					// Call the WithinSight function to determine if this specific object is within sight
					if ((obj = WithinHearingRange (transform, linearAudibilityThreshold, hitColliders [i].transform, ref audibility)) != null) {
						// This object is within hearing range. Set it to the objectHeard GameObject if the audibility is less than any of the other objects
						if (audibility > maxAudibility) {
							maxAudibility = audibility;
							objectHeard = obj;
						}
					}
				}
			}
			return objectHeard;
		}

		// Public helper function that will automatically create an audibility variable that is not used. This function is useful if the calling call doesn't
		// care about the audibility value
		public static Transform WithinHearingRange (Transform transform, float linearAudibilityThreshold, Transform targetObject)
		{
			float audibility = 0;
			return WithinHearingRange (transform, linearAudibilityThreshold, targetObject, ref audibility);
		}

		private static Transform WithinHearingRange (Transform transform, float linearAudibilityThreshold, Transform targetObject, ref float audibility)
		{
			AudioSource colliderAudioSource;
			// Check to see if the hit agent has an audio source and that audio source is playing
			if ((colliderAudioSource = targetObject.GetComponent<AudioSource> ()) != null && colliderAudioSource.isPlaying) {
				// The audio source is playing. Make sure the sound can be heard from the agent's current position
				audibility = colliderAudioSource.volume / Vector3.Distance (transform.position, targetObject.position);
				if (audibility > linearAudibilityThreshold) {
					return targetObject;
				}
			}
			return null;
		}

		// Draws the line of sight representation
		public static void DrawLineOfSight (Transform transform, float fieldOfViewAngle, float viewDistance)
		{
#if UNITY_EDITOR
			float radius = viewDistance * Mathf.Sin (fieldOfViewAngle * Mathf.Deg2Rad);
			var oldColor = UnityEditor.Handles.color;
			UnityEditor.Handles.color = Color.yellow;
			// draw a disk at the end of the sight distance.
			UnityEditor.Handles.DrawWireDisc (transform.position + transform.forward * viewDistance, transform.forward, radius);
			// draw to lines to represent the left and right side of the line of sight
			UnityEditor.Handles.DrawLine (transform.position, TransformPointIgnoreScale (transform, new Vector3 (radius, 0, viewDistance)));
			UnityEditor.Handles.DrawLine (transform.position, TransformPointIgnoreScale (transform, new Vector3 (-radius, 0, viewDistance)));
			UnityEditor.Handles.color = oldColor;
#endif
		}

		private static Vector3 TransformPointIgnoreScale (Transform transform, Vector3 point)
		{
			return transform.rotation * point + transform.position;
		}
	}
}