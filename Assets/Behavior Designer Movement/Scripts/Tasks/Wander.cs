using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Tasks.Movement
{
	[TaskDescription("Wander using the Unity NavMesh.")]
	[TaskCategory("Movement")]
	[HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Movement/documentation.php?id=9")]
	[TaskIcon("Assets/Behavior Designer Movement/Editor/Icons/{SkinColor}WanderIcon.png")]
	public class Wander : Action
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
		[Tooltip("How far ahead of the current position to look ahead for a wander")]
		public float
			wanderDistance = 20;
		[Tooltip("The amount that the agent rotates direction")]
		public float
			wanderRate = 2;
		public float walkRadius;
		private Vector3 position;

		// A cache of the NavMeshAgent
		private NavMeshAgent navMeshAgent;

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
			position = Target ();
			Debug.DrawLine (transform.position, position, Color.green);
			navMeshAgent.destination = position;
		}

		// There is no success or fail state with wander - the agent will just keep wandering
		public override TaskStatus OnUpdate ()
		{
			if (Mathf.Abs (Vector3.Distance (transform.position, position)) <= arriveDistance) {
				position = Target ();
			}

			Debug.DrawLine (transform.position, position, Color.green);
			navMeshAgent.destination = position;
			return TaskStatus.Running;
		}

		public override void OnEnd ()
		{
			// Disable the nav mesh
			navMeshAgent.enabled = false;
		}

		// Return targetPosition if targetTransform is null
		private Vector3 Target ()
		{

			// point in a new random direction and then multiply that by the wander distance
			Vector3 r = Random.insideUnitSphere;
			r.y = 0;
			Vector3 direction = transform.forward + r * wanderRate;
			Vector3 p = transform.position + direction.normalized * wanderDistance;

			NavMeshHit hit;
			NavMesh.SamplePosition (p, out hit, walkRadius, 1);
			Vector3 finalPosition = hit.position;
			return finalPosition;
		}

		// Reset the public variables
		public override void OnReset ()
		{
			arriveDistance = 0.1f;
			wanderDistance = 20;
			wanderRate = 2;
		}
	}
}