/* 
* This code has been designed and developed by Gray Lake Studios.
* You may only use this code if you’ve acquired the appropriate license.
* To acquire such licenses you may visit www.graylakestudios.com and/or Unity Asset Store
* For all inquiries you may contact contact@graylakestudios.com
* Copyright © 2012 Gray Lake Studios
*/

using UnityEngine;
using System;

namespace ProD
{
	//You should note that this script doesn't inherit from Monobehaviour.
	//We do this so we can generate cells on the go without having to create GameObjects for them first.
	public class Cell : IComparable<Cell>
	{
		private Address _Address;
		public Address address
		{
			get { return _Address; }

		}
		public void SetCellAddress(Address a)
		{
			_Address = a;
		}
		public void SetCellAddress(int x, int y)
		{
			_Address = new Address(x, y);
		}
		public int x
		{
			get { return address.x; }
		}
		public int y
		{
			get { return address.y; }
		}

		//Setting cell's type.
		private string _type = null;
		public string type { get { return _type; } }
		public void SetCellType(string typeName)
		{
			_type = typeName;
		}


		#region A* Pathfinding Stuff
		//g is the value that will be added to the nodes as the algorithms tries to find the quickest path from start to exit.
		public int g { get; set; }
		public int h { get; set; }
		public int f { get; set; }

		public Cell parent { get; set; }

		#region IComparable[Cell] implementation
		public int CompareTo(Cell other)
		{
			if (this.f == other.f)
				return 0;
			//higher priority when f is smaller than the others f.
			else if (this.f < other.f)
				return 1;
			else return -1;
		}
		#endregion
		#endregion

		public DirectionManager dirMan;

		public Cell()
		{
			SetCellAddress(new Address(-1, -1));
			dirMan = new DirectionManager();
		}

		public Cell(Address a)
		{
			SetCellAddress(a);
			dirMan = new DirectionManager();
		}
	}
}