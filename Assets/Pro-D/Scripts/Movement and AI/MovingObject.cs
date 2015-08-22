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
	public abstract class MovingObject : MonoBehaviour
	{
		public float layer = 0.5f;

		public List<string> walkableCellTypes;

		public Map currentMap { get; set; }
		public WorldMap currentWorld { get; set; }
		public Cell currentCell { get; set; }

		public bool MoveToCell(Cell targetCell)
		{
			if (currentWorld == null || currentMap == null)
			{
				Debug.Log("This moving object is in no world or map");
				return false;
			}
			bool cellIsMovableTo = false;

			foreach (string walkableType in walkableCellTypes)
			{
				if (targetCell.type.Equals(walkableType))
				{
					cellIsMovableTo = true;
					break;
				}
			}

			if (!cellIsMovableTo) return false;

			Address actualCellPosition = currentWorld.GetAddressWorldPosition(currentMap, new Address(targetCell.x, targetCell.y));

			//Put the object on the world location above map.

			float newPos_X = ProDManager.Instance.tileSpacingX * ((float)actualCellPosition.x);
			float newPos_Z = ProDManager.Instance.tileSpacingY * ((float)actualCellPosition.y);


			if (ProDManager.Instance.topDown)
			{
				gameObject.transform.position = new Vector3(newPos_X, layer, newPos_Z);
			}
			else
			{
				gameObject.transform.position = new Vector3(newPos_X, newPos_Z, -layer);
			}
			//Set his theoretical location on the array.
			currentCell = targetCell;

			return true;
		}
	}
}
