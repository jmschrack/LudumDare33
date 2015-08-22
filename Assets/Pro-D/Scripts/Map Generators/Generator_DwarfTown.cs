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
	public static class Generator_DwarfTown
	{
		#region PROD DEFINED DEFAULT CONTENT
		private static Map map;
		private static void PrepareMap()
		{
			map = new Map(map_Size_X, map_Size_Y);
			map.theme = theme;
		}
		public static void SetGenericProperties(int map_Size_X_, int map_Size_Y_, string theme_)
		{
			map_Size_X = map_Size_X_;
			map_Size_Y = map_Size_Y_;
			theme = theme_;
		}
		#endregion PROD DEFINED DEFAULT CONTENT

		#region USER DEFINED DEFAULT CONTENT
		//You should set all these variables to your liking.
		private static int map_Size_X = 29;
		private static int map_Size_Y = 29;

		//private static string theme = "Stone Dungeon Theme";
		private static string theme = "Terminal Theme";
		public static void SetTheme(string t)
		{
			theme = t;
		}


		//These variables are here for runtime alteration, mainly for debug testing.
		private static string type_Abyss = "Abyss";
		private static string type_Path = "Path";
		private static string type_Wall = "Wall";
		private static int room1_Min_X = 3;
		private static int room1_Max_X = 11;
		private static int room1_Min_Y = 3;
		private static int room1_Max_Y = 11;
		private static int room1_Freq = 10;
		private static int room1_Retry = 6;
		private static int room2_Min_X = 3;
		private static int room2_Max_X = 11;
		private static int room2_Min_Y = 3;
		private static int room2_Max_Y = 11;
		private static int room2_Freq = 10;
		private static int room2_Retry = 6;


		//This method is only here for runtime alteration, mainly for debug testing.
		public static void SetSpecificProperties(string type_Abyss_, string type_Path_, string type_Wall_,
			int room1_Min_X_, int room1_Max_X_, int room1_Min_Y_, int room1_Max_Y_, int room1_Freq_, int room1_Retry_,
			int room2_Min_X_, int room2_Max_X_, int room2_Min_Y_, int room2_Max_Y_, int room2_Freq_, int room2_Retry_)
		{
			type_Abyss = type_Abyss_;
			type_Path = type_Path_;
			type_Wall = type_Wall_;
			room1_Min_X = room1_Min_X_;
			room1_Max_X = room1_Max_X_;
			room1_Min_Y = room1_Min_Y_;
			room1_Max_Y = room1_Max_Y_;
			room1_Freq = room1_Freq_;
			room1_Retry = room1_Retry_;
			room2_Min_X = room2_Min_X_;
			room2_Max_X = room2_Max_X_;
			room2_Min_Y = room2_Min_Y_;
			room2_Max_Y = room2_Max_Y_;
			room2_Freq = room2_Freq_;
			room2_Retry = room2_Retry_;
		}

		//Generate() has the actual methods that generate the map data. 
		//You may customize this method with MethodLibrary methods.
		public static Map Generate()
		{

			string type_Wall2 = "Wall2";
			string type_PathOutside = "PathOutside";

			List<string> walkableTypes = new List<string>();
			walkableTypes.Add(type_Path);
			walkableTypes.Add(type_PathOutside);
			walkableTypes.Add("Door");

			int areaCount = 0;
			int retry = 100; //to prevent unity from crashing
			do
			{
				PrepareMap();

				MethodLibrary.FrameMap(map, type_Wall, 1);

				List<Room> rooms1 = MethodLibrary.CreateRooms(map, type_Wall, type_Path, room1_Min_X, room1_Max_X, room1_Min_Y, room1_Max_Y, room1_Freq, room1_Retry);

				MethodLibrary.SetCellsOfTypeAToB(map, type_Wall, type_Wall2);

				List<Room> rooms2 = MethodLibrary.CreateRooms(map, type_Wall, type_Path, room2_Min_X, room2_Max_X, room2_Min_Y, room2_Max_Y, room2_Freq, room2_Retry);

				foreach (Room r in rooms1)
				{
					MethodLibrary.CreateDoors("Door", type_Wall2, map, r, false, Random.Range(1, 4));
				}

				foreach (Room r in rooms2)
				{
					MethodLibrary.CreateDoors("Door", type_Wall, map, r, false, Random.Range(1, 4));
				}


				MethodLibrary.SetCellsOfTypeAToB(map, type_Wall2, type_Wall);

				MethodLibrary.SetCellsOfTypeAToB(map, type_Abyss, type_PathOutside);

				//ensure that everything is connected
				List<Room> areas = MethodLibrary.GetIsolatedAreas(map, walkableTypes);
				areaCount = areas.Count;

				retry--;
			} while (areaCount > 1 && retry > 0);

			//MethodLibrary.AddNoiseOn_I(map, "PathWithGoblin", type_Path, 5);


			return map;
		}
		#endregion USER DEFINED DEFAULT CONTENT
	}
}