/* 
* This code has been designed and developed by Gray Lake Studios.
* You may only use this code if you’ve acquired the appropriate license.
* To acquire such licenses you may visit www.graylakestudios.com and/or Unity Asset Store
* For all inquiries you may contact contact@graylakestudios.com
* Copyright © 2012 Gray Lake Studios
*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProD
{
	public class FogOfWar : MonoBehaviour
	{
		public float layer = 1.0f;

		public int visibilityRange = 6;

		public Color visible = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		public Color visited = new Color(0.0f, 0.0f, 0.0f, 0.9f);
		public Color unvisited = new Color(0.0f, 0.0f, 0.0f, 1.0f);

		public ShadowType type = ShadowType.Rekursive;

		public FilterMode filterMode = FilterMode.Bilinear;

		public List<string> opaqueCells;

		public enum ShadowType
		{
			Rekursive, RaySquare, RayRound, Flood, RekursiveFlood, RaySquareFlood, RayRoundFlood
		}

		private Map map;

		private GameObject fogOfWarPlane;
		private Texture2D fogTexture;
		public Texture2D FogTexture { get { return fogTexture; } }


		private struct DirectionVector
		{
			public int X { get; private set; }
			public int Y { get; private set; }
			public DirectionVector(int x, int y)
				: this()
			{
				this.X = x;
				this.Y = y;
			}
		}

		private struct ColumnPortion
		{
			public int X { get; private set; }
			public DirectionVector BottomVector { get; private set; }
			public DirectionVector TopVector { get; private set; }
			public ColumnPortion(int x, DirectionVector bottom, DirectionVector top)
				: this()
			{
				this.X = x;
				this.BottomVector = bottom;
				this.TopVector = top;
			}
		}

		public void InitFoW(Map map_)
		{
			map = map_;

			if (fogOfWarPlane != null) Destroy(fogOfWarPlane);

			string directory = "FogOfWar/PRE_FogOfWarPlane";
			GameObject tempPlane = Resources.Load(directory) as GameObject;
			fogOfWarPlane = (GameObject)Instantiate(tempPlane);

			fogTexture = new Texture2D(map.size_X, map.size_Y);

			for (int i = 0; i < map.size_X; i++)
				for (int j = 0; j < map.size_Y; j++)
					fogTexture.SetPixel(i, j, unvisited);
			fogTexture.Apply();
			fogTexture.filterMode = filterMode;
			//previewTexture = dataLayer.ConvertMapToTexture(map, false);


			if (ProDManager.Instance.topDown)
			{
				fogOfWarPlane.transform.localScale = new Vector3(map.size_X, layer, map.size_Y);
				fogOfWarPlane.transform.position = new Vector3(map.size_X / 2, layer, map.size_Y / 2);
			}
			else
			{
				fogOfWarPlane.transform.Rotate(-90f, 0.0f, 0.0f, Space.World);
				fogOfWarPlane.transform.localScale = new Vector3(map.size_X, layer, map.size_Y);
				fogOfWarPlane.transform.position = new Vector3(map.size_X / 2, map.size_Y / 2, -layer);
			}

			fogOfWarPlane.GetComponent<Renderer>().material.SetTexture("_MainTex", fogTexture);

			fogOfWarPlane.GetComponent<Renderer>().material.color = Color.white;
		}

		public void DestroyFoW()
		{
			if (fogOfWarPlane != null)
			{
				Destroy(fogOfWarPlane);
				fogOfWarPlane = null;
			}

			if (fogTexture != null)
			{
				Destroy(fogTexture);
				fogTexture = null;
			}
		}

		public void UpdateFoW(Address playerPosition)
		{
			if (fogOfWarPlane == null) return;

			//GenerateFog(map);
			fadeExploredCells();

			List<Address> visibleCells;

			switch (type)
			{
				case ShadowType.Rekursive:
					visibleCells = findVisibleCellsColumn(map, playerPosition, visibilityRange);
					break;

				case ShadowType.RaySquare:
					visibleCells = findVisibleCellsRaySquare(map, playerPosition, visibilityRange);
					break;

				case ShadowType.RayRound:
					visibleCells = findVisibleCellsRayRound(map, playerPosition, visibilityRange);
					break;

				case ShadowType.Flood:
					visibleCells = findVisibleCellsFlood(map, playerPosition, visibilityRange);
					break;

				case ShadowType.RekursiveFlood:
					visibleCells = findVisibleCellsColumn(map, playerPosition, visibilityRange);
					List<Address> a = findVisibleCellsFlood(map, playerPosition, 2 * visibilityRange);
					visibleCells.RemoveAll(add => a.Find(bdd => bdd.Equals(add)) == null);
					break;

				case ShadowType.RaySquareFlood:
					visibleCells = findVisibleCellsRaySquare(map, playerPosition, visibilityRange);
					List<Address> b = findVisibleCellsFlood(map, playerPosition, 2 * visibilityRange);
					visibleCells.RemoveAll(add => b.Find(bdd => bdd.Equals(add)) == null);
					break;

				case ShadowType.RayRoundFlood:
					visibleCells = findVisibleCellsRayRound(map, playerPosition, visibilityRange);
					List<Address> c = findVisibleCellsFlood(map, playerPosition, 2 * visibilityRange);
					visibleCells.RemoveAll(add => c.Find(bdd => bdd.Equals(add)) == null);
					break;

				default:
					visibleCells = new List<Address>();
					break;
			}

			foreach (Address a in visibleCells)
				fogTexture.SetPixel(a.x, a.y, visible);
			fogTexture.Apply();
			fogOfWarPlane.GetComponent<Renderer>().material.SetTexture("_MainTex", fogTexture);

		}

		private void fadeExploredCells()
		{
			for (int i = 0; i < fogTexture.width; i++)
			{
				for (int j = 0; j < fogTexture.height; j++)
				{
					if (fogTexture.GetPixel(i, j).Equals(visible))
					{
						fogTexture.SetPixel(i, j, visited);
					}
				}
			}
		}

		private List<Address> findVisibleCellsFlood(Map map, Address position, int range)
		{
			int arrSize = range * 2 + 1;

			int[,] arr = new int[arrSize, arrSize];

			for (int i = 0; i < arrSize; i++)
				for (int j = 0; j < arrSize; j++)
					arr[i, j] = range + 1; //init array with high distance

			arr[range, range] = 0; // center cell is target

			Stack<Address> stack = new Stack<Address>();

			stack.Push(new Address(range, range));

			while (stack.Count != 0)
			{
				Address current = stack.Pop();

				Address mapAddress = new Address(position.x + current.x - range, position.y + current.y - range);

				if (!map.Contains(mapAddress) || opaqueCells.Contains(map.cellsOnMap[mapAddress.x, mapAddress.y].type))
					continue;

				int newDist = arr[current.x, current.y] + 1;

				if (current.x > 0 && newDist < arr[current.x - 1, current.y])
				{
					stack.Push(new Address(current.x - 1, current.y));
					arr[current.x - 1, current.y] = newDist;
				}
				if (current.x < arrSize - 1 && newDist < arr[current.x + 1, current.y])
				{
					stack.Push(new Address(current.x + 1, current.y));
					arr[current.x + 1, current.y] = newDist;
				}
				if (current.y > 0 && newDist < arr[current.x, current.y - 1])
				{
					stack.Push(new Address(current.x, current.y - 1));
					arr[current.x, current.y - 1] = newDist;
				}
				if (current.y < arrSize - 1 && newDist < arr[current.x, current.y + 1])
				{
					stack.Push(new Address(current.x, current.y + 1));
					arr[current.x, current.y + 1] = newDist;
				}
			}


			List<Address> result = new List<Address>();

			for (int i = 0; i < arrSize; i++)
			{
				for (int j = 0; j < arrSize; j++)
				{
					Address mapAddress = new Address(position.x + i - range, position.y + j - range);

					if (map.Contains(mapAddress) && arr[i, j] <= range)
						result.Add(mapAddress);
				}
			}


			return result;
		}

		private List<Address> findVisibleCellsRaySquare(Map map, Address position, int range)
		{
			List<Address> result = new List<Address>();

			for (int i = position.x - range; i <= position.x + range; i++)
				result.AddRange(CastRay(position.x, position.y, i, position.y + range, map, opaqueCells));
			for (int i = position.y + range; i >= position.y - range; i--)
				result.AddRange(CastRay(position.x, position.y, position.x + range, i, map, opaqueCells));
			for (int i = position.x + range; i >= position.x - range; i--)
				result.AddRange(CastRay(position.x, position.y, i, position.y - range, map, opaqueCells));
			for (int i = position.y - range; i <= position.y + range; i++)
				result.AddRange(CastRay(position.x, position.y, position.x - range, i, map, opaqueCells));

			return result;
		}

		private List<Address> findVisibleCellsRayRound(Map map, Address position, int range)
		{
			List<Address> result = new List<Address>();

			//bresenham circle:

			//set up the values needed for the algorithm
			int f = 1 - range; //used to track the progress of the drawn circle (since its semi-recursive)
			int ddFx = 1; //step x
			int ddFy = -2 * range; //step y
			int x = 0;
			int y = range;

			//this algorithm doesn't account for the farthest points, 
			//so we have to set them manually
			result.AddRange(CastRay(position.x, position.y, position.x + range, position.y, map, opaqueCells));
			result.AddRange(CastRay(position.x, position.y, position.x - range, position.y, map, opaqueCells));
			result.AddRange(CastRay(position.x, position.y, position.x, position.y + range, map, opaqueCells));
			result.AddRange(CastRay(position.x, position.y, position.x, position.y - range, map, opaqueCells));

			while (x < y)
			{
				if (f >= 0)
				{
					y--;
					ddFy += 2;
					f += ddFy;
				}
				x++;
				ddFx += 2;
				f += ddFx;

				//build our current arc
				result.AddRange(CastRay(position.x, position.y, position.x + x, position.y + y, map, opaqueCells));
				result.AddRange(CastRay(position.x, position.y, position.x - x, position.y + y, map, opaqueCells));
				result.AddRange(CastRay(position.x, position.y, position.x + x, position.y - y, map, opaqueCells));
				result.AddRange(CastRay(position.x, position.y, position.x - x, position.y - y, map, opaqueCells));
				result.AddRange(CastRay(position.x, position.y, position.x + y, position.y + x, map, opaqueCells));
				result.AddRange(CastRay(position.x, position.y, position.x - y, position.y + x, map, opaqueCells));
				result.AddRange(CastRay(position.x, position.y, position.x + y, position.y - x, map, opaqueCells));
				result.AddRange(CastRay(position.x, position.y, position.x - y, position.y - x, map, opaqueCells));
			}
			return result;
		}

		private static List<Address> CastRay(int x0, int y0, int x1, int y1, Map map, List<string> types)
		{
			// Optimization: it would be preferable to calculate in
			// advance the size of "result" and to use a fixed-size array
			// instead of a list.
			List<Address> result = new List<Address>();

			bool steep = false;
			if (Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0))
				steep = true;

			//if (steep) {
			//	Swap(ref x0, ref y0);
			//	Swap(ref x1, ref y1);
			//}
			//if (x0 > x1) {
			//	Swap(ref x0, ref x1);
			//	Swap(ref y0, ref y1);
			//}

			int deltax = Mathf.Abs(x1 - x0);
			int deltay = Mathf.Abs(y1 - y0);
			int error = 0;
			int ystep;
			int xstep;
			int x = x0;
			int y = y0;
			if (x0 < x1) xstep = 1; else xstep = -1;
			if (y0 < y1) ystep = 1; else ystep = -1;

			if (!steep)
			{
				for (int i = 0; i <= deltax; i++)
				{

					if (map.Contains(x, y) == false)
						return result;
					result.Add(new Address(x, y));
					if (types.Contains(map.cellsOnMap[x, y].type))
						return result;
					x += xstep;
					error += deltay;
					if (2 * error >= deltax)
					{
						y += ystep;
						error -= deltax;
					}
				}
			}
			else
			{
				for (int i = 0; i <= deltay; i++)
				{
					if (map.Contains(x, y) == false)
						return result;
					result.Add(new Address(x, y));
					if (types.Contains(map.cellsOnMap[x, y].type))
						return result;
					y += ystep;
					error += deltax;
					if (2 * error >= deltay)
					{
						x += xstep;
						error -= deltay;
					}
				}
			}

			return result;
		}

		private List<Address> findVisibleCellsColumn(Map map, Address position, int range)
		{
			List<Address> visibleCells = new List<Address>();

			for (int i = 0; i < 8; i++)
			{
				Queue<ColumnPortion> workQueue = new Queue<ColumnPortion>();

				workQueue.Enqueue(new ColumnPortion(0, new DirectionVector(1, 0), new DirectionVector(1, 1)));
				while (workQueue.Count > 0)
				{
					ColumnPortion currentPortion = workQueue.Dequeue();
					visibleCells.AddRange(checkColumn(currentPortion, position, map, workQueue, i, range));
				}
			}
			return visibleCells;
		}

		private List<Address> checkColumn(ColumnPortion cp, Address origin, Map map, Queue<ColumnPortion> queue, int octant, int range)
		{
			List<Address> result = new List<Address>();
			DirectionVector topVector = cp.TopVector;
			DirectionVector bottomVector = cp.BottomVector;
			int x = cp.X;
			int topY;
			if (cp.X == 0)
				topY = 0;
			else
			{
				int quotient = (2 * cp.X + 1) * cp.TopVector.Y / (2 * cp.TopVector.X);
				int remainder = (2 * cp.X + 1) * cp.TopVector.Y % (2 * cp.TopVector.X);

				topY = quotient;
				if (remainder > cp.TopVector.X)
					topY = quotient++;
			}

			int bottomY;
			if (cp.X == 0)
				bottomY = 0;
			else
			{
				int quotient = (2 * cp.X - 1) * cp.BottomVector.Y / (2 * cp.BottomVector.X);
				int remainder = (2 * cp.X - 1) * cp.BottomVector.Y % (2 * cp.BottomVector.X);

				bottomY = quotient;
				if (remainder >= cp.BottomVector.X)
					bottomY = quotient++;

			}

			bool? wasLastCellOpaque = null;

			for (int y = topY; y >= bottomY; --y)
			{
				bool inRadius = IsInRadius(x, y, range);
				if (inRadius)
				{
					Address temp = TranslateOctant(new Address(x, y), octant);
					// The current cell is in the field of view.
					result.Add(new Address(origin.x + temp.x, origin.y + temp.y));
				}

				// A cell that was too far away to be seen is effectively
				// an opaque cell; nothing "above" it is going to be visible
				// in the next column, so we might as well treat it as 
				// an opaque cell and not scan the cells that are also too
				// far away in the next column.

				bool currentIsOpaque = !inRadius || isOpaque(map, origin, x, y, octant);
				if (wasLastCellOpaque != null)
				{
					if (currentIsOpaque)
					{
						// We've found a boundary from transparent to opaque. Make a note
						// of it and revisit it later.
						if (!wasLastCellOpaque.Value)
						{
							// The new bottom vector touches the upper left corner of 
							// opaque cell that is below the transparent cell. 
							queue.Enqueue(new ColumnPortion(
								x + 1,
								new DirectionVector(x * 2 - 1, y * 2 + 1),
								topVector));
						}
					}
					else if (wasLastCellOpaque.Value)
					{
						// We've found a boundary from opaque to transparent. Adjust the
						// top vector so that when we find the next boundary or do
						// the bottom cell, we have the right top vector.
						//
						// The new top vector touches the lower right corner of the 
						// opaque cell that is above the transparent cell, which is
						// the upper right corner of the current transparent cell.
						topVector = new DirectionVector(x * 2 + 1, y * 2 + 1);
					}
				}
				wasLastCellOpaque = currentIsOpaque;
			}

			// Make a note of the lowest opaque-->transparent transition, if there is one. 
			if (wasLastCellOpaque != null && !wasLastCellOpaque.Value)
				queue.Enqueue(new ColumnPortion(x + 1, bottomVector, topVector));

			return result;
		}

		private static bool IsInRadius(int x, int y, int length)
		{
			return (2 * x - 1) * (2 * x - 1) + (2 * y - 1) * (2 * y - 1) <= 4 * length * length;
		}

		private bool isOpaque(Map map, Address origin, int x, int y, int octant)
		{
			Address temp = TranslateOctant(new Address(x, y), octant);
			x = temp.x; y = temp.y;
			return opaqueCells.Contains(map.cellsOnMap[origin.x + x, origin.y + y].type);
		}

		private static Address TranslateOctant(Address a, int octant)
		{
			switch (octant)
			{
				default: return a;
				case 1: return new Address(a.y, a.x);
				case 2: return new Address(-a.y, a.x);
				case 3: return new Address(-a.x, a.y);
				case 4: return new Address(-a.x, -a.y);
				case 5: return new Address(-a.y, -a.x);
				case 6: return new Address(a.y, -a.x);
				case 7: return new Address(a.x, -a.y);
			}
		}
	}
}

