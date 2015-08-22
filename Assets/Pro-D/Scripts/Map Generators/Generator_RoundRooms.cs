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
	public static class Generator_RoundRooms
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
		private static int room_Diameter = 7;
		private static int room_Spacing = 6;


		//This method is only here for runtime alteration, mainly for debug testing.
		public static void SetSpecificProperties(string type_Abyss_, string type_Path_, string type_Wall_,
			int room_Diameter_, int room_Spacing_)
		{
			type_Abyss = type_Abyss_;
			type_Path = type_Path_;
			type_Wall = type_Wall_;
			room_Diameter = room_Diameter_;
			room_Spacing = room_Spacing_;
		}

		//Generate() has the actual methods that generate the map data. 
		//You may customize this method with MethodLibrary methods.
		public static Map Generate()
		{
			PrepareMap();

			MethodLibrary.AddNoise_III(map, type_Path, 20, room_Diameter / 2 + 1);

			MethodLibrary.ApplyMinDistance(type_Path, type_Abyss, map, room_Spacing);

			List<Cell> centers = MethodLibrary.GetListOfCellType(type_Path, map);
			centers.Shuffle();

			List<string> types = new List<string>();
			types.Add(type_Path);

			for (int i = 0; i < room_Diameter / 2; i++)
			{
				if (i % 2 == 0)
					MethodLibrary.GrowCellsInPlusShape(type_Path, 1, map);
				else
					MethodLibrary.ExpandCell(types, map, 0);
			}


			Cell first = centers[0];
			Cell last = null;

			foreach (Cell current in centers)
			{
				if (last != null)
					MethodLibrary.ConnectTwoCells(current, last, map, type_Path, true, 1);

				last = current;
			}
			MethodLibrary.ConnectTwoCells(first, last, map, type_Path, true, 1);

			MethodLibrary.EnwallCells(map, type_Path, type_Wall);

			return map;
		}
		#endregion USER DEFINED DEFAULT CONTENT
	}
}