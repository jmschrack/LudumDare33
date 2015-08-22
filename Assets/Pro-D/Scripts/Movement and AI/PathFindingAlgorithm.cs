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
using System;

namespace ProD
{
	public class PathfindingAlgorithm
	{
		public List<string> walkableCellTypes;

		private Heap<Cell> _openList;
		private IList<Cell> _closedList;
		private Map _map;
		private Cell[,] _world;

		public PathfindingAlgorithm(Map map)
		{
			walkableCellTypes = new List<string>();
			_map = map;
			_world = _map.cellsOnMap;
			_openList = new Heap<Cell>();
			_closedList = new List<Cell>();
		}

		private void SetManhattenDist(Address start, Address target)
		{
			for (int x = 0; x < _map.size_X; ++x)
			{
				for (int y = 0; y < _map.size_Y; ++y)
				{
					Cell c = _world[x, y];
					if (c.address.Equals(start))
						continue;

					c.h = Mathf.Abs(target.x - x) + Mathf.Abs(target.y - y);
					c.g = int.MaxValue;
					c.f = 0;
					c.parent = null;
				}
			}
		}

		public int ReturnManhattanDist(Address start, Address target)
		{
			return Mathf.Abs(target.x - start.x) + Mathf.Abs(target.y - start.y);
		}

		public Stack<Cell> GetFastestPath(Cell start, Cell target)
		{
			if (target == null)
			{
				throw new Exception("target is null.");
			}

			if (walkableCellTypes.Contains(target.type) == false)
				throw new Exception("Cannot move to an unwalkable cell.");

			_openList.Clear();
			_closedList.Clear();

			SetManhattenDist(start.address, target.address);
			start.g = 0;
			//the estimation of going from target cell to target cell is 0 of course.
			target.h = 0;
			Stack<Cell> cells = new Stack<Cell>();

			if (!start.Equals(target))
			{
				_openList.Push(start);
				while (true)
				{
					Cell c;
					try { c = _openList.Pop(); }
					catch (IndexOutOfRangeException e)
					{
						e.ToString();
						break;
					}
					if (c == null)
						break;

					//if we havent already checked out the cell c
					if (!_closedList.Contains(c))
					{
						List<Cell> neighbours = MethodLibrary.FindNeighbourCells(_map, c.x, c.y, false, false);

						foreach (Cell cc in neighbours)
						{
							if (cc == null)
								continue;

							if (walkableCellTypes.Contains(cc.type) && !_closedList.Contains(cc))
							{
								int dist = Mathf.Abs(cc.address.x - c.address.x) != Mathf.Abs(cc.address.y - c.address.y) ? 14 : 10;
								if (c.g + dist < cc.g)
								{
									cc.g = c.g + dist;
									cc.parent = c;
									cc.f = c.g + c.h;
								}
								if (!_openList.Contains(cc))
									_openList.Push(cc);
							}
						} _closedList.Add(c);
					}
					if (c.address.Equals(target.address))
					{
						cells.Push(c);
						while (c.parent != null && !c.parent.Equals(start))
						{
							c = c.parent;
							cells.Push(c);
						}
						break;
					}
				}
			}
			return cells;
		}
	}
}
