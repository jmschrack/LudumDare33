using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Tasks.Movement
{
	[TaskDescription("Find a place to hide and move to it using the Unity NavMesh.")]
	[TaskCategory("Movement")]
	[HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Movement/documentation.php?id=8")]
	[TaskIcon("Assets/Behavior Designer Movement/Editor/Icons/{SkinColor}CoverIcon.png")]
	public class Cover : Action
	{
		[Tooltip("The speed of the agent")]
		public SharedFloat
			speed;
		[Tooltip("Angular speed of the agent")]
		public SharedFloat
			angularSpeed;
		[Tooltip("The agent has arrived when the square magnitude is less than this value")]
		public float
			arriveDistance = 0.1f;
		[Tooltip("The distance to search for cover")]
		public float
			maxCoverDistance = 1000;
		[Tooltip("The maximum number of raycasts that should be fired before the agent gives up looking for an agent to find cover behind")]
		public int
			maxRaycasts = 100;
		[Tooltip("How large the step should be between raycasts")]
		public float
			rayStep = 1;
		[Tooltip("Once a cover point has been found, multiply this offset by the normal to prevent the agent from hugging the wall")]
		public float
			coverOffset = 2;
		[Tooltip("Should the agent look at the cover point after it has arrived?")]
		public bool
			lookAtCoverPoint = false;
		[Tooltip("The agent is done rotating to the cover point when the square magnitude is less than this value")]
		public float
			rotationEpsilon = 0.5f;
		[Tooltip("Max rotation delta if lookAtCoverPoint")]
		public SharedFloat
			maxLookAtRotationDelta;

		// A cache of the NavMeshAgent
		private NavMeshAgent navMeshAgent;

		private Vector3 coverPoint;

		public override void OnAwake ()
		{
			// cache for quick lookup
			navMeshAgent = gameObject.GetComponent<NavMeshAgent> ();
		}

		public override void OnStart ()
		{
			// set the speed, angular speed, and destination then enable the agent
			navMeshAgent.speed = speed.Value;
			navMeshAgent.angularSpeed = angularSpeed.Value;
			navMeshAgent.enabled = true;
			navMeshAgent.destination = Target ();
		}

		// Seek to the cover point. Return success as soon as the location is reached or the agent is looking at the cover point
		public override TaskStatus OnUpdate ()
		{
			if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < arriveDistance) {
				var rotation = Quaternion.LookRotation (coverPoint - transform.position);
				// Return success if the agent isn't going to look at hte cover point or it has completely rotated to look at the cover point
				if (!lookAtCoverPoint || Quaternion.Angle (transform.rotation, rotation) < rotationEpsilon) {
					return TaskStatus.Success;
				} else {
					// Still needs to rotate towards the target
					transform.rotation = Quaternion.RotateTowards (transform.rotation, rotation, maxLookAtRotationDelta.Value);
				}
			}

			// Return running until the agent has arrived
			return TaskStatus.Running;
		}

		public override void OnEnd ()
		{
			// Disable the nav mesh
			navMeshAgent.enabled = false;
		}

		// Find a place to hide by firing a ray 
		private Vector3 Target ()
		{
			RaycastHit hit;
			int raycastCount = 0;
			var direction = transform.forward;
			float step = 0;
			// Keep firing a ray until too many rays have been fired
			while (raycastCount < maxRaycasts) {
				var ray = new Ray (transform.position, direction);
				if (Physics.Raycast (ray, out hit, maxCoverDistance)) {
					// A suitable agent has been found. Find the opposite side of that agent by shooting a ray in the opposite direction from a point far away
					if (-hit.normal == direction && hit.collider.Raycast (new Ray (ray.GetPoint (maxCoverDistance), -direction), out hit, Mathf.Infinity)) {
						coverPoint = hit.point;
						return hit.point + hit.normal * coverOffset;
					}
				}
				// Keep sweeiping along the y axis
				step += rayStep;
				direction = Quaternion.Euler (0, transform.eulerAngles.y + step, 0) * Vector3.forward;
				raycastCount++;
			}
			// The agent wasn't found - return zero
			return Vector3.zero;
		}

		// Reset the public variables
		public override void OnReset ()
		{
			arriveDistance = 0.1f;
			maxCoverDistance = 1000;
			maxRaycasts = 100;
			rayStep = 1;
			coverOffset = 2;
			lookAtCoverPoint = false;
			rotationEpsilon = 0.5f;
		}
	}
}