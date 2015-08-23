using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Tasks.Movement
{
	[TaskDescription("Follow the leader using the Unity NavMesh.")]
	[TaskCategory("Movement")]
	[HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Movement/documentation.php?id=14")]
	[TaskIcon("Assets/Behavior Designer Movement/Editor/Icons/{SkinColor}LeaderFollowIcon.png")]
	public class LeaderFollow : Action
	{
		[Tooltip("Agents less than this distance apart are neighbors")]
		public float
			neighborDistance = 10;
		[Tooltip("How far behind the leader the agents should follow the leader")]
		public float
			leaderBehindDistance = 2;
		[Tooltip("The distance that the agents should be separated")]
		public float
			separationDistance = 2;
		[Tooltip("The agent is getting too close to the front of the leader if they are within the aheadDistance")]
		public float
			aheadDistance = 2;
		[Tooltip("The leader to follow")]
		public SharedTransform
			leaderT = null;
		NavMeshAgent leader;
		// A cache of the NavMeshAgent
		private NavMeshAgent navMeshAgent;

		// The transform of the leader
		private Transform leaderTransform;
		// The corresponding transforms of the agents
		private Transform agentTransform;

		public override void OnAwake ()
		{
			// cache for quick lookup
			navMeshAgent = gameObject.GetComponent<NavMeshAgent> ();
		}

		public override void OnStart ()
		{
			leader = leaderT.Value.gameObject.GetComponent<NavMeshAgent> ();
			leaderTransform = leader.transform;	

			// Enable the nav mesh
			navMeshAgent.enabled = true;
			agentTransform = navMeshAgent.transform;
		}

		// The agents will always be flocking so always return running
		public override TaskStatus OnUpdate ()
		{
			var behindPosition = LeaderBehindPosition ();
			// Determine a destination for each agent

			// Get out of the way of the leader if the leader is currently looking at the agent and is getting close
			if (LeaderLookingAtAgent () && Vector3.SqrMagnitude (leaderTransform.position - agentTransform.position) < aheadDistance) {
				navMeshAgent.destination = transform.position + (transform.position - leaderTransform.position).normalized * aheadDistance;
			} else {
				// The destination is the behind position added to the separation vector
				navMeshAgent.destination = behindPosition;
			}

			return TaskStatus.Running;
		}

		public override void OnEnd ()
		{
			// Disable the nav mesh
			navMeshAgent.enabled = false;

		}

		private Vector3 LeaderBehindPosition ()
		{
			// The behind position is the normalized inverse of the leader's velocity multiplied by the leaderBehindDistance
			return leaderTransform.position + (-leader.velocity).normalized * leaderBehindDistance;
		}
		

		// Use the dot product to determine if the leader is looking at the current agent
		public bool LeaderLookingAtAgent ()
		{
			return Vector3.Dot (leaderTransform.forward, agentTransform.forward) < -0.5f;
		}

		// Reset the public variables
		public override void OnReset ()
		{
			neighborDistance = 10;
			leaderBehindDistance = 2;
			separationDistance = 2;
			aheadDistance = 2;
			leader = null;

		}
	}
}