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
	public class Room
	{
		public enum RoomType { Square, Freeform, NONE };

		private RoomType roomType = RoomType.Square;
		private int roomStart_X = 0;
		private int roomEnd_X = 0;
		private int roomStart_Y = 0;
		private int roomEnd_Y = 0;
		private int roomWidth = 0; //Includes walls.
		private int roomHeight = 0; //Includes walls.

		public RoomType Type { get { return roomType; } }
		public int RoomStart_X { get { return roomStart_X; } }
		public int RoomEnd_X { get { return roomEnd_X; } }
		public int RoomStart_Y { get { return roomStart_Y; } }
		public int RoomEnd_Y { get { return roomEnd_Y; } }
		public int RoomWidth { get { return roomWidth; } }
		public int RoomHeight { get { return roomHeight; } }

		private List<Cell> cellsInRoom = new List<Cell>();

		public List<Cell> GetCellsInRoom(Map map)
		{
			if (cellsInRoom != null && cellsInRoom.Count > 0)
				return cellsInRoom;
			else if (Type == RoomType.Square)
			{
				cellsInRoom = new List<Cell>();
				for (int j = roomStart_Y + 1; j < roomEnd_Y; j++)
					for (int i = roomStart_X + 1; i < roomEnd_X; i++)
						cellsInRoom.Add(map.cellsOnMap[i, j]);
				return cellsInRoom;
			}
			else
				return new List<Cell>();

		}

		public Room(int start_X, int start_Y, int end_X, int end_Y)
		{
			this.roomStart_X = start_X;
			this.roomStart_Y = start_Y;
			this.roomEnd_X = end_X;
			this.roomEnd_Y = end_Y;
			roomType = RoomType.Square;
			roomWidth = roomEnd_X + 1 - roomStart_X;
			roomHeight = roomEnd_Y + 1 - roomStart_Y;
		}

		public Room(List<Cell> cells)
		{
			roomType = RoomType.Freeform;
			cellsInRoom = cells;
		}
	}
}
