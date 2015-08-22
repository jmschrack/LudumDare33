/* 
* This code has been designed and developed by Gray Lake Studios.
* You may only use this code if you’ve acquired the appropriate license.
* To acquire such licenses you may visit www.graylakestudios.com and/or Unity Asset Store
* For all inquiries you may contact contact@graylakestudios.com
* Copyright © 2012 Gray Lake Studios
*/

using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace ProD
{
	public class TextureManager : MonoBehaviour
	{
		public Color wallColor = Color.red;
		public Color pathColor = Color.cyan;
		public Color unilluminatedColor;
		public Color abyssColor = Color.grey;
		public Color doorColor = Color.blue;
		public Color entranceColor = Color.green;
		public Color exitColor = Color.yellow;

		public string mapDir = "Maps";

		public delegate void EventHandler(Texture2D t);
		public event EventHandler TextureLoaded;

		//private string basePath;

		private WWW www;

		void Awake()
		{
			//basePath = Application.dataPath;
		}

		//public void SaveMap(Map map, string name)
		//{
		//    Texture2D tex = ConvertMapToTexture(map, false);
		//    if (name == null || name == "")
		//        name = "DefaultMap";
		//    string directory = basePath + "/../" + mapDir;
		//    if (!Directory.Exists(directory))
		//        Directory.CreateDirectory(directory);

		//    directory += "/" + name ;

		//    SaveTexture(tex, directory + ".png");
		//}

		public Map LoadMap(Texture2D mapTexture)
		{
			return ConvertTextureToMap(mapTexture);
		}

		public Map ConvertTextureToMap(Texture2D mapTexture)
		{
			Map result = new Map(mapTexture.width, mapTexture.height);
			for (int i = 0; i < result.size_X; i++)
			{
				for (int j = 0; j < result.size_Y; j++)
				{
					Color c = mapTexture.GetPixel(i, j);
					result.cellsOnMap[i, j].SetCellType(getCellType(c));
				}
			}
			return result;
		}

		public Texture2D ConvertMapToTexture(Map map, bool forcePowerOfTwo)
		{
			Texture2D generatedTexture = null;
			if (forcePowerOfTwo)
			{
				int powerOfTwo_X = 1;
				int powerOfTwo_Y = 1;

				while (powerOfTwo_X < map.size_X) powerOfTwo_X *= 2;
				while (powerOfTwo_Y < map.size_Y) powerOfTwo_Y *= 2;
				generatedTexture = new Texture2D(powerOfTwo_X, powerOfTwo_Y, TextureFormat.ARGB32, false);
			}
			else
			{
				generatedTexture = new Texture2D(map.size_X, map.size_Y, TextureFormat.ARGB32, false);
			}

			generatedTexture.filterMode = FilterMode.Point;

			//Set pixels and apply it to the mapPlane.

			//Debug.Log("tempCellsInMap is at " + i + " " + j + " in world map array.");
			for (int i = 0; i < map.size_X; i++)
			{
				for (int j = 0; j < map.size_Y; j++)
				{
					generatedTexture.SetPixel
						(
							i, j,
							getCellColor(map.cellsOnMap[i, j].type)
							);

					if (map.cellsOnMap[i, j].type.Equals("Abyss"))
					{
						generatedTexture.SetPixel
						(
							i, j,
							Color.clear
							);
					}

				}
			}
			generatedTexture.Apply();
			return generatedTexture;
		}

		//public void SaveTexture(Texture2D tex, string directory)
		//{
		//    byte[] bytes = tex.EncodeToPNG();
		//    File.WriteAllBytes(directory, bytes);
		//}

		public Color getCellColor(string cellType)
		{
			Color result = this.abyssColor;
			switch (cellType)
			{
				case "Wall":
					result = wallColor;
					break;
				case "Path":
					result = pathColor;
					break;
				case "Door":
					result = doorColor;
					break;
				case "Entrance":
					result = entranceColor;
					break;
				case "Exit":
					result = exitColor;
					break;
				default:
					break;
			}
			return result;
		}

		public string getCellType(Color cellColor)
		{
			string result = "Abyss";
			if (cellColor == wallColor)
				result = "Wall";
			else if (cellColor == pathColor)
				result = "Path";
			else if (cellColor == doorColor)
				result = "Door";
			else if (cellColor == entranceColor)
				result = "Entrance";
			else if (cellColor == exitColor)
				result = "Exit";
			return result;
		}


		public void LoadMapTexture(string path)
		{
			//string directory = "file://" + Application.dataPath + "/../" + mapDir;
			//directory += "/" + name + ".png";

			string url = "file://" + path;

			www = new WWW(url);

			StartCoroutine(getTextureFromURL(url));
		}

		public IEnumerator getTextureFromURL(string url)
		{
			yield return www;
			// assign texture
			if (TextureLoaded != null)
				TextureLoaded(www.texture);
		}


	}
}
