//TurnBasedMovement.cs is a simple script for creating and moving your player.
//To use it place it on the player prefab in Resources folder or any other player prefab you come up with.

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
	public class TurnBasedPlayerMovement : PlayerMovement, TurnBasedActor
	{
		private bool isMyTurn = false;

		private Vector2 movementInput;

		private CameraObjectTracker myCameraTracker;

		public void OnEnable()
		{
			movementInput = new Vector2();
			SetCamera("Main Camera");
			InputManager.Instance.turnBasedMovement = this;
			if (InputManager.Instance.pathFinding != null) InputManager.Instance.pathFinding.turnBasedMovement = this;

			TurnManager.Instance.addActor(this);
		}

		/// <summary>
		/// Places the player on the map.
		/// </summary>
		/// <param name='newWorld'>
		/// The world to put the player in.
		/// </param>
		public override void SetupPlayer(WorldMap newWorld)
		{
			SetupPlayer(newWorld, new Address(0, 0), null);
		}

		/// <summary>
		/// Places the player on the map.
		/// </summary>
		/// <param name='newWorld'>
		/// The world to put the player in.
		/// </param>
		/// <param name='mapAdress'>
		/// Indicates which map of the world the player is put in.
		/// </param>
		/// <param name='spawnPoint'>
		/// The address in the map the player is spawned at. If its null, the player will spawn at a random spot.
		/// </param>
		public override void SetupPlayer(WorldMap newWorld, Address mapAdress, Address spawnPoint)
		{
			if (newWorld.maps == null || newWorld.size_X <= 0 || newWorld.size_Y <= 0)
				return;

			currentWorld = newWorld;

			//Resize player to cell size
			//if (ProDManager.Instance.topREPLACEDown)
			//{
			//    transform.localScale = new Vector3(ProDManager.Instance.tileSpacingX, 1, ProDManager.Instance.tileSpacingY); 
			//}
			//else
			//{
			//    transform.localScale = new Vector3(ProDManager.Instance.tileSpacingX, ProDManager.Instance.tileSpacingY, 1);  
			//}

			if (currentWorld == null || mapAdress.x < 0 || mapAdress.x >= currentWorld.size_X ||
				mapAdress.y < 0 || mapAdress.y >= currentWorld.size_Y)
				return;


			currentMap = currentWorld.maps[mapAdress.x, mapAdress.y];
			if (spawnPoint == null)
			{
				List<Cell> placementList = new List<Cell>();
				foreach (string walkableType in walkableCellTypes)
					placementList.AddRange(MethodLibrary.GetListOfCellType(walkableType, currentMap));
				MoveToCell(placementList[Random.Range(0, placementList.Count - 1)]);
			}
			else
				MoveToCell(currentMap.GetCell(spawnPoint.x, spawnPoint.y));

			updateDependencies();
		}

		public void SetCamera(string cameraName)
		{
			GameObject myCameraTrackerGO = GameObject.Find(cameraName);
			if (myCameraTrackerGO == null) return;
			else myCameraTracker = myCameraTrackerGO.GetComponent<CameraObjectTracker>();
			if (myCameraTracker != null)
			{
				myCameraTracker.SetTarget(this.transform);
				if (ProDManager.Instance.getFogOfWar() != null)
					myCameraTracker.SetMinimumDisplay(ProDManager.Instance.getFogOfWar().visibilityRange + 0.5f);
			}
		}

		public void startTurn()
		{
			isMyTurn = true;
			//Debug.Log("I was started");
		}

		public void SetInput(Vector2 input)
		{
			movementInput.x = Mathf.Clamp(movementInput.x + ((input.x == 0) ? 0 : Mathf.Sign(input.x)), -1, 1);
			movementInput.y = Mathf.Clamp(movementInput.y + ((input.y == 0) ? 0 : Mathf.Sign(input.y)), -1, 1);
		}

		//Set movement input here
		void Update()
		{
			if (currentWorld == null || currentMap == null || isMyTurn == false)
				return;

			if (movementInput != Vector2.zero)
			{
				bool wasMoveSuccessful = MoveToCell(currentMap.cellsOnMap[currentCell.x + (int)movementInput.x, currentCell.y + (int)movementInput.y]);
				movementInput = Vector2.zero;

				if (wasMoveSuccessful)
				{
					updateDependencies();

					isMyTurn = false;
					TurnManager.Instance.endTurn(this);
				}
			}
		}

		private void updateDependencies()
		{
			//Reposition camera on player.
			if (myCameraTracker != null)
				myCameraTracker.UpdatePosition();

			if (ProDManager.Instance.getFogOfWar() != null)
				ProDManager.Instance.getFogOfWar().UpdateFoW(currentCell.address);

			if (ProDManager.Instance.getPathfinding() != null)
				ProDManager.Instance.getPathfinding().UpdatePathfinding(currentCell.address);
		}

	}
}
