/* 
* This code has been designed and developed by Gray Lake Studios.
* You may only use this code if you’ve acquired the appropriate license.
* To acquire such licenses you may visit www.graylakestudios.com and/or Unity Asset Store
* For all inquiries you may contact contact@graylakestudios.com
* Copyright © 2012 Gray Lake Studios
*/

using UnityEngine;
using System.Collections;

namespace ProD
{
	public class AI_Goblin : MovingObject, TurnBasedActor
	{


		public void OnEnable()
		{
			TurnManager.Instance.addActor(this);
		}

		public void startTurn()
		{
			//Debug.Log("goblin was started ");

			//Random movement
			bool wasMoveSuccessful = false;
			do
			{
				int moveX;
				int moveY;

				do
				{
					moveX = Random.Range(-1, 2);
					moveY = Random.Range(-1, 2);
				} while ((moveX == 0) == (moveY == 0));

				wasMoveSuccessful = MoveToCell(currentMap.cellsOnMap[currentCell.x + moveX, currentCell.y + moveY]);
			} while (!wasMoveSuccessful);
			


			TurnManager.Instance.endTurn(this);
		}

	} 
}
