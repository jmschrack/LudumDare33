/* 
* This code has been designed and developed by Gray Lake Studios.
* You may only use this code if you’ve acquired the appropriate license.
* To acquire such licenses you may visit www.graylakestudios.com and/or Unity Asset Store
* For all inquiries you may contact contact@graylakestudios.com
* Copyright © 2012 Gray Lake Studios
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ProD
{
	public class TurnManager : Singleton<TurnManager>
	{
		public List<TurnBasedActor> actors = new List<TurnBasedActor>();
		public List<TurnBasedActor> actorsChanged = new List<TurnBasedActor>();

		private List<TurnBasedActor>.Enumerator currentActor;
		private bool isCurrentActorDone = true;

		void Start()
		{
			currentActor = actors.GetEnumerator();
		}

		public void addActor(TurnBasedActor actor)
		{
			if (actorsChanged.Contains(actor) == false)
			{
				//Debug.Log("The Actor " + actor.ToString() + " has been added.");
				actorsChanged.Add(actor);
			}
			else
			{
				Debug.Log("The Actor " + actor.ToString() + " is already playing.");
			}
		}


		public void removeAllActors()
		{
			currentActor.Dispose();
			actorsChanged.Clear();
			actors.Clear();
			currentActor = actors.GetEnumerator();
			isCurrentActorDone = true;
		}

		public void removeActor(TurnBasedActor actor)
		{
			if (actorsChanged.Contains(actor) == true)
			{
				//Debug.Log("The Actor " + actor.ToString() + " has been removed.");
				actorsChanged.Remove(actor);
				if (actor.Equals(currentActor.Current))
				{
					isCurrentActorDone = true;
				}
			}
			else
			{
				Debug.Log("The Actor " + actor.ToString() + " is not playing.");
			}
		}


		public void endTurn(TurnBasedActor actor)
		{
			if (actor.Equals(currentActor.Current))
			{
				//Debug.Log("this one has ended his turn: " + actor.ToString());
				isCurrentActorDone = true;
			}
			else
			{
				Debug.Log("It's not your turn, " + actor.ToString());
			}
		}

		void Update()
		{
			if (isCurrentActorDone)
			{

				if (currentActor.MoveNext() == false)
				{
					currentActor.Dispose();
					actors.Clear();
					actors.AddRange(actorsChanged);
					currentActor = actors.GetEnumerator();
					currentActor.MoveNext();
				}
				if (currentActor.Current != null)
				{
					isCurrentActorDone = false;
					currentActor.Current.startTurn();
				}

			}
		}
	}

}