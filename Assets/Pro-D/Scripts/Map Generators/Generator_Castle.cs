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
	public static class Generator_Castle
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
		private static string theme = "Terminal Theme";

		//These variables are here for runtime alteration, mainly for debug testing.
		private static string type_Abyss = "Abyss";
		private static string type_Path = "Path";
		private static string type_Wall = "Wall";
		private static string type_Door = "Door";
		private static string type_PathOutside = "PathOutside";
		private static int room_Freq = 5;
		private static int doorsPerRoom = 2;
		private static int tower_Freq = 20;
		private static int tower_Dist = 7;
		private static int tower_Diameter = 2;
		private static int ward_Chance = 3;
		private static int ward_growth = 6;
		private static int gate_Count = 1;

		//This method is only here for runtime alteration, mainly for debug testing.
		public static void SetSpecificProperties(string type_Abyss_, string type_Path_, string type_Wall_,
			int room_Freq_, int doorsPerRoom_,
			int tower_Freq_, int tower_Dist_, int tower_Diameter_,
			int ward_Chance_, int ward_growth_, int gate_Count_)
		{
			type_Abyss = type_Abyss_;
			type_Path = type_Path_;
			type_Wall = type_Wall_;
			room_Freq = room_Freq_;
			tower_Freq = tower_Freq_;
			tower_Dist = tower_Dist_;
			tower_Diameter = tower_Diameter_;
			gate_Count = gate_Count_;
			doorsPerRoom = doorsPerRoom_;
			ward_Chance = ward_Chance_;
			ward_growth = ward_growth_;
		}

		//Generate() has the actual methods that generate the map data. 
		//You may customize this method with MethodLibrary methods.
		public static Map Generate()
		{
			int areaCount = 0;
			int retry = 100; //to prevent unity from crashing
			do
			{

				PrepareMap();

				List<string> types = new List<string>();

				int frameThickness = (map_Size_X < map_Size_Y) ? map_Size_X : map_Size_Y;
				frameThickness /= 4;

				//frame map
				MethodLibrary.FrameMap(map, "frame", frameThickness);
				//create the rooms
				List<Room> rooms = MethodLibrary.CreateRooms(map, "frame", type_Path, 3, frameThickness, 3, frameThickness, room_Freq, 1000);
				MethodLibrary.EnwallCells(map, type_Path, type_Wall);

				//grow around rooms
				MethodLibrary.AddNoiseOn_II(map, type_PathOutside, type_Abyss, ward_Chance);
				types.Add(type_Wall);
				MethodLibrary.GrowCellsCircular(type_PathOutside, ward_growth, map, types, false);
				types.Clear();

				//unframe map
				MethodLibrary.SetCellsOfTypeAToB(map, "frame", type_Abyss);

				//build castlewalls
				MethodLibrary.ConvertUnreachableCells(map, type_Abyss, "tmp1");
				MethodLibrary.SetCellsOfTypeAToB(map, type_Abyss, "castleWall");
				MethodLibrary.SetCellsOfTypeAToB(map, "tmp1", type_Abyss);

				//build towers
				MethodLibrary.AddNoiseOn_II(map, "tower", "castleWall", tower_Freq);
				MethodLibrary.ApplyMinDistance("tower", "castleWall", map, tower_Dist);
				MethodLibrary.GrowCellsCircular("tower", tower_Diameter, map);
				MethodLibrary.SetCellsOfTypeAToB(map, "tower", type_Wall);

				//the castles gate
				MethodLibrary.AddNoiseOn_I(map, type_Door, "castleWall", gate_Count);

				//castlewalls to walls
				MethodLibrary.SetCellsOfTypeAToB(map, "castleWall", type_Wall);

				//make doors
				foreach (Room r in rooms)
				{
					MethodLibrary.CreateDoors(type_Door, type_Wall, map, r, false, doorsPerRoom);
				}

				//make surroundings path
				MethodLibrary.SetCellsOfTypeAToB(map, type_Abyss, type_PathOutside);


				//ensure that everything is connected
				types.Add(type_Path);
				types.Add(type_PathOutside);
				types.Add(type_Door);
				List<Room> areas = MethodLibrary.GetIsolatedAreas(map, types);
				types.Clear();
				areaCount = areas.Count;

				retry--;
			} while (areaCount > 1 && retry > 0);

			return map;
		}
		#endregion USER DEFINED DEFAULT CONTENT

	}
}

