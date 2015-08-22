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
	public class PlayerMovement : MovingObject
	{

		public virtual void SetupPlayer(WorldMap newWorld)
		{
			//Override this in respective movement scripts.
		}

		public virtual void SetupPlayer(WorldMap newWorld, Address mapAdress, Address spawnPoint)
		{
			//Override this in respective movement scripts.
		}
	}
}
