/* 
* This code has been designed and developed by Gray Lake Studios.
* You may only use this code if you’ve acquired the appropriate license.
* To acquire such licenses you may visit www.graylakestudios.com and/or Unity Asset Store
* For all inquiries you may contact contact@graylakestudios.com
* Copyright © 2012 Gray Lake Studios
*/

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace ProD
{
	public class Materializer : Singleton<Materializer>
	{
		//TODO: Write a method that nicely parents cells in a given scene to their respective maps and so forth.

		public bool rotateTiles = true;
		public bool useIsometric = false;
		public bool groupTiles = true;

		#region [ private fields ]
		private Dictionary<string, GameObject> tileDictionary;
		private GameObject parentObject;
		private GameObject previewTile;

		public TextureManager dataLayer;
		#endregion

		public static List<GameObject> allPrefabs = new List<GameObject>();
		public static List<GameObject> allTileGroups = new List<GameObject>();

		/// <summary>
		/// Materializes the world map.
		/// </summary>
		/// <param name='worldMap'>
		/// The world map to materialize.
		/// </param>
		public void MaterializeWorldMap(WorldMap worldMap)
		{
			for (int j = 0; j < worldMap.size_Y; j++)
			{
				for (int i = 0; i < worldMap.size_X; i++)
				{
					MaterializeMap(worldMap.maps[i, j]);
				}
			}

		}

		/// <summary>
		/// Unmaterializes the world map by destorying all the prefabs that were instantiated
		/// </summary>
		/// <param name='worldMap'>
		/// World map.
		/// </param>
		public void UnmaterializeWorldMap()
		{
			while (allPrefabs.Count > 0)
			{
				GameObject tempGO = allPrefabs[allPrefabs.Count - 1];
				allPrefabs.RemoveAt(allPrefabs.Count - 1);
				Destroy(tempGO);
			}
			while (allTileGroups.Count > 0)
			{
				GameObject tempGO = allTileGroups[allTileGroups.Count - 1];
				allTileGroups.RemoveAt(allTileGroups.Count - 1);
				Destroy(tempGO);
			}

		}

		/// <summary>
		/// Materializes a map.
		/// </summary>
		/// <param name='map'>
		/// A map of a level.
		/// </param>
		public void MaterializeMap(Map map)
		{
			tileDictionary = new Dictionary<string, GameObject>();

			if (groupTiles)
			{
				Address a = map.addressOnWorldMap;
				parentObject = new GameObject();
				parentObject.name = "Map_" + a.x.ToString() + "_" + a.y.ToString();
				parentObject.transform.parent = this.transform;
				allTileGroups.Add(parentObject);
			}


			for (int j = 0; j < map.size_Y; j++)
			{
				for (int i = 0; i < map.size_X; i++)
				{
					string[] orientation = new string[2];
					orientation = GetOrientation(map, new Address(i, j));
					GameObject prefab;
					prefab = GetPrefab(map.cellsOnMap[i, j].type, orientation, map.theme);
					PlacePrefab(prefab, map, new Address(i, j), orientation);

				}
			}
		}



		//TODO:  We can do all these with a binary system. Convert to a binary system later on.
		/// <summary>
		/// Gets the orientation of a tile at a given adress. The orientation of a tile
		/// means on many sides, and which sides, a wall tile will need a surface/texture.
		/// </summary>
		/// <returns>
		/// The orientation, which consists of two strings, one for orientation (Corner, One Sided etc.)
		/// and one for rotation (North, South, West, East etc.)
		/// </returns>
		/// <param name='map'>
		/// The map that contains the tile.
		/// </param>
		/// <param name='a'>
		/// The adress of the tile you want the orientation of.
		/// </param>
		private string[] GetOrientation(Map map, Address a)
		{
			string[] orientation = new string[2];
			orientation[0] = "Default";
			orientation[1] = "";

			string type = map.cellsOnMap[a.x, a.y].type;
			if (type == "Door") type = "Wall"; //Doors are alwys treated as walls
			else if (type == "Path") return orientation;

			bool[] b = new bool[4];



			//for (int i = 0, x = 0, y = 1; i < 4; i++, x += y, y = y - x, x = x - y) // this beauty loops through all 4 directions. Order : N W S E

			//Check for every cell around address. 
			for (int i = 0, x = 0, y = 1; i < 4; i++, x += y, y = y - x, x = x + y) // this beauty loops through all 4 directions. Order : N E S W
			{
				//Debug.Log(i + ": (" + x + ", " + y + ")");
				if (map.Contains(a.x + x, a.y + y))
				{
					string typeOther = map.cellsOnMap[a.x + x, a.y + y].type;
					if (typeOther == "Door") typeOther = "Wall"; //Doors are alwys treated as walls
					if (typeOther == type) b[i] = true; //If a surrounding cell is the same type then mark it true.
					if (typeOther == "Abyss") b[i] = true; //This is here so we don't get two sided walls on the outer edges.
				}
				else
					b[i] = true; //This is here so we don't get two sided walls on the border edges of the whole map.
			}
			

			/*
			 * Example
			 * .0.
			 * 3.1
			 * .2.
			 * 
			 * Core     Column 		Corner          	Tip					TwoSided 	OneSided   
			 * .#.		...	   		.#. .#. ... ... 	.#. ... ... ... 	.#. ...  	.#. .#. .#. ...
			 * ###		.#.	   		##. .## .## ##. 	.#. ##. .#. .## 	.#. ###  	.## ### ##. ###
			 * .#.		...	   		... ... .#. .#. 	... ... .#. ... 	.#. ...  	.#. ... .#. .#. 
			 * 						NW  NE  SE  SW      S   E   N   W       V   H       W   S   E   N
			 * 
			 * NA - Not available.
			 * No - North
			 * NW - NorthWest
			 * Ve - Vertical
			 * 
			 */

			//According to the marks around the cell return orientation.
			if (b[0] && b[1] && b[2] && b[3]) { orientation[0] = "Core"; orientation[1] = ""; }
			else if (!b[0] && !b[1] && !b[2] && !b[3]) { orientation[0] = "Column"; orientation[1] = ""; }

			else if (b[0] && !b[1] && !b[2] && b[3]) { orientation[0] = "Corner"; orientation[1] = "NW"; }
			else if (b[0] && b[1] && !b[2] && !b[3]) { orientation[0] = "Corner"; orientation[1] = "NE"; }
			else if (!b[0] && b[1] && b[2] && !b[3]) { orientation[0] = "Corner"; orientation[1] = "SE"; }
			else if (!b[0] && !b[1] && b[2] && b[3]) { orientation[0] = "Corner"; orientation[1] = "SW"; }

			else if (b[0] && !b[1] && !b[2] && !b[3]) { orientation[0] = "Tip"; orientation[1] = "S"; }
			else if (!b[0] && !b[1] && !b[2] && b[3]) { orientation[0] = "Tip"; orientation[1] = "E"; }
			else if (!b[0] && !b[1] && b[2] && !b[3]) { orientation[0] = "Tip"; orientation[1] = "N"; }
			else if (!b[0] && b[1] && !b[2] && !b[3]) { orientation[0] = "Tip"; orientation[1] = "W"; }

			else if (b[0] && !b[1] && b[2] && !b[3]) { orientation[0] = "TwoSided"; orientation[1] = "V"; }
			else if (!b[0] && b[1] && !b[2] && b[3]) { orientation[0] = "TwoSided"; orientation[1] = "H"; }

			else if (b[0] && b[1] && b[2] && !b[3]) { orientation[0] = "OneSided"; orientation[1] = "W"; }
			else if (b[0] && b[1] && !b[2] && b[3]) { orientation[0] = "OneSided"; orientation[1] = "S"; }
			else if (b[0] && !b[1] && b[2] && b[3]) { orientation[0] = "OneSided"; orientation[1] = "E"; }
			else if (!b[0] && b[1] && b[2] && b[3]) { orientation[0] = "OneSided"; orientation[1] = "N"; }

			return orientation;

		}

		/// <summary>
		/// Gets the prefab for a specified type of tile. 
		/// The prefabs need to be in the corresponding directories, or this will return null.
		/// </summary>
		/// <returns>
		/// The prefab.
		/// </returns>
		/// <param name='type'>
		/// The type of tile you want the prefab of
		/// </param>
		/// <param name='orientation'>
		/// The orientation of the prefab. For example, it could be a corner tile or a straight wall.
		/// </param>
		/// <param name='theme'>
		/// The theme of the tile.
		/// </param>
		public GameObject GetPrefab(string type, string[] orientation, string theme)
		{
			GameObject prefab = null;
			//Check in the dictionary if this type of tile already exists. If not, load it from the resources.

			string prefabType = orientation[0];
			string prefabRotation = orientation[1];
			//Debug.Log(prefabType);

			string directory = theme + "/Cells/" + type + "/PRE_" + type + "_" + prefabType;
			if (useIsometric)
				directory += "_" + prefabRotation;

			//Debug.Log(directory);
			if (tileDictionary.ContainsKey(directory))
				prefab = tileDictionary[directory];
			else
			{
				prefab = Resources.Load(directory) as GameObject;
				if (prefab == null)
				{
					//If there are no matches in the directory try for default prefab.
					directory = theme + "/Cells/" + type + "/PRE_" + type + "_" + "Default";
					if (tileDictionary.ContainsKey(directory))
						prefab = tileDictionary[directory];
					else
					{
						prefab = Resources.Load(directory) as GameObject;
						//Add the tile to the tileDictionary.
						tileDictionary.Add(directory, prefab);
					}
				}
				else
					//Add the tile to the tileDictionary.
					tileDictionary.Add(directory, prefab);
			}

			return prefab;
		}

		/// <summary>
		/// Places the prefab by instantiating it at a given location on the map. 
		/// </summary>
		/// <param name='prefab'>
		/// The prefab.
		/// </param>
		/// <param name='map'>
		/// The map which the tile belongs to.
		/// </param>
		/// <param name='address'>
		/// The adress of the tile in the map.
		/// </param>
		/// <param name='orientation'>
		/// The orientation of the tile.
		/// </param>
		public void PlacePrefab(GameObject prefab, Map map, Address address, string[] orientation)
		{
			if (prefab == null)
			{
				//Debug.LogError("Null is not a valid prefab for placement.");
				return;
			}

			//Location in worldMap
			float prefab_X = map.addressOnWorldMap.x * map.size_X * ProDManager.Instance.tileSpacingX;//* prefab.transform.localScale.x;
			float prefab_Y = 0;
			float prefab_Z = 0;

			if (ProDManager.Instance.topDown == true)
			{
				prefab_Z = map.addressOnWorldMap.y * map.size_Y * ProDManager.Instance.tileSpacingY; //* prefab.transform.localScale.z;
			}
			else
			{
				prefab_Y = map.addressOnWorldMap.y * map.size_Y * ProDManager.Instance.tileSpacingY; //* prefab.transform.localScale.z;
			}

			//Location in Map
			prefab_X += address.x * ProDManager.Instance.tileSpacingX; //prefab.transform.localScale.x*
			if (ProDManager.Instance.topDown == true)
			{
				prefab_Y += prefab.transform.position.y;
				prefab_Z += address.y * ProDManager.Instance.tileSpacingY; //prefab.transform.localScale.y*
			}
			else
			{
				prefab_Y += address.y * ProDManager.Instance.tileSpacingY; //prefab.transform.localScale.y*
				prefab_Z += prefab.transform.position.z;

			}


			GameObject cellGO = (GameObject)Instantiate(prefab, new Vector3(prefab_X, prefab_Y, prefab_Z), prefab.transform.rotation);

			// Reparent the instantiated object. This is expensive, so only do it if GroupTiles is enabled
			if (groupTiles) cellGO.transform.parent = this.parentObject.transform;

			//this was used to scale the prefabs. due to common demand, it doesnt scale anymore
			//if (ProDManager.Instance.topREPLACEDown == true)
			//{
			//    cellGO.transform.localScale = new Vector3(ProDManager.Instance.prefab_Size_X, 1, ProDManager.Instance.prefab_Size_Y);
			//}
			//else
			//{
			//    cellGO.transform.localScale = new Vector3(ProDManager.Instance.prefab_Size_X, ProDManager.Instance.prefab_Size_Y, 1);
			//}

			if (rotateTiles)
			{

				string rotation = orientation[1];

				if (ProDManager.Instance.topDown == true)
				{
					if (rotation == "V") cellGO.transform.Rotate(0.0f, 90f, 0.0f, Space.World);

					else if (rotation == "W" || rotation == "SE") cellGO.transform.Rotate(0.0f, 90.0f, 0.0f, Space.World);
					else if (rotation == "N" || rotation == "SW") cellGO.transform.Rotate(0.0f, 180.0f, 0.0f, Space.World);
					else if (rotation == "E" || rotation == "NW") cellGO.transform.Rotate(0.0f, 270.0f, 0.0f, Space.World);
				}
				else
				{
					if (rotation == "V") cellGO.transform.Rotate(0.0f, 0.0f, -90.0f);

					else if (rotation == "W" || rotation == "SE") cellGO.transform.Rotate(0.0f, 0.0f, -90.0f, Space.World);
					else if (rotation == "N" || rotation == "SW") cellGO.transform.Rotate(0.0f, 0.0f, -180.0f, Space.World);
					else if (rotation == "E" || rotation == "NW") cellGO.transform.Rotate(0.0f, 0.0f, -270.0f, Space.World);
				}
			}
			allPrefabs.Add(cellGO.gameObject);

			MovingObject movingObject = cellGO.GetComponentInChildren<MovingObject>();

			if (movingObject != null)
			{
				movingObject.currentMap = map;
				movingObject.currentWorld = map.worldMap;
				movingObject.currentCell = map.GetCell(address.x, address.y);
			}

			return;
		}

	}
}