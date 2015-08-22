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
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;
using ProD.TMX;

namespace ProD
{
	//this class provides methods for storing and saving and loading maps into/from the TMX (Tile Map XML) format
	//TODO: have methods to choose and customise the spritesheet, with which the map is displayed in Tiled
	public class FilePorter : Singleton<FilePorter>
	{

		/// <summary>
		/// Converts a given map to a CSV string.
		/// </summary>
		/// <returns>
		/// The map as a single string.
		/// </returns>
		/// <param name='map'>
		/// The specific map you want to be converted.
		/// </param>
		/// <param name='types'>
		/// The list of types. The indices of the types are used as values in the CSV string. Every new found type in the map is added to the list.
		/// </param>
		public string mapToCsvString(Map map, ref List<string> types)
		{
			if (types == null) return "error";

			string result = null;

			using (StringWriter stringWriter = new StringWriter())
			{
				stringWriter.WriteLine(""); //newline for readability
				for (int y = map.size_Y - 1; y >= 0; y--) // reverse y axis to match Tiled's coordinate system
				{
					for (int x = 0; x < map.size_X; x++)
					{
						int idx = types.IndexOf(map.GetCell(x, y).type);
						if (idx == -1)
						{
							types.Add(map.GetCell(x, y).type);
							idx = types.Count - 1;
						}
						stringWriter.Write(idx + 1); // write idx plus one because thats what tmx does appearently
						if (x < map.size_X - 1 || y > 0) // file format crashes, when there is a comma after last value
							stringWriter.Write(",");
					}
					stringWriter.WriteLine(""); //newline for readability
				}

				result = stringWriter.ToString();
			}


			return result;
		}

		/// <summary>
		/// Saves a given map int the TMX (Tile Map XML) format.
		/// By now it uses no compression and CSV encoding.
		/// Be carefull, as the "defaultProd.png" spritesheet to display the tmx in Tiled only supports 10 additional types besides the standard types Abyss, Wall, Path, Door, Entrance and Exit.
		/// Warning: may throw exceptions, occuring during filewriting!
		/// Warning: this will only work for standalone builds. (TODO: test for Mac and Linux)
		/// </summary>
		/// <param name='map'>
		/// The map you want to be saved.
		/// </param>
		/// <param name='path'>
		/// The path where the file should be saved.  Make sure that the last character is a slash! When trying to open the file with an editor (for example "Tiled") make shure that the spritesheet "defaultProd.png" resides in the same folder
		/// </param>
		/// <param name='filename'>
		/// The name of the TMX file without extension.
		/// </param>
		public void saveMapToTmx(Map map, string path, string filename)
		{
			List<string> types = new List<string>();
			//add standard types
			types.Add("Abyss");
			types.Add("Wall");
			types.Add("Path");
			types.Add("Door");
			types.Add("Entrance");
			types.Add("Exit");
			//all other types will be added while creating the string representing the map

			string mapAsString = mapToCsvString(map, ref types);

			MapXML mapXml = new MapXML();
			mapXml.version = "1.0";
			mapXml.orientation = "orthogonal";
			mapXml.tilewidth = 32;
			mapXml.tileheight = 32;
			mapXml.width = map.size_X;
			mapXml.height = map.size_Y;
			mapXml.tilesetXml = new TilesetXML();
			mapXml.tilesetXml.firstgid = 1;
			mapXml.tilesetXml.name = "defaultProD";
			mapXml.tilesetXml.tilewidth = 32;
			mapXml.tilesetXml.tileheight = 32;
			mapXml.tilesetXml.spacing = 0;
			mapXml.tilesetXml.margin = 0;
			mapXml.tilesetXml.imageXml = new ImageXML();
			mapXml.tilesetXml.imageXml.source = path + "defaultProd.png";
			mapXml.tilesetXml.tilesXml = new List<TileXML>();

			for (int i = 0; i < types.Count; i++)
			{
				TileXML tileXml = new TileXML();
				tileXml.id = i;
				tileXml.propertiesXml = new PropertiesXML();
				tileXml.propertiesXml.propertiesXml = new List<PropertyXML>();
				PropertyXML propertyXml = new PropertyXML();
				propertyXml.name = "type";
				propertyXml.value = types[i];
				tileXml.propertiesXml.propertiesXml.Add(propertyXml);
				mapXml.tilesetXml.tilesXml.Add(tileXml);

			}

			mapXml.layerXml = new LayerXML();
			mapXml.layerXml.name = "GroundLayer";
			mapXml.layerXml.width = mapXml.width;
			mapXml.layerXml.height = mapXml.height;
			mapXml.layerXml.dataXml = new DataXML();
			mapXml.layerXml.dataXml.encoding = "csv";
			mapXml.layerXml.dataXml.text = mapAsString;
			mapXml.propertiesXml = new PropertiesXML();
			mapXml.propertiesXml.propertiesXml = new List<PropertyXML>();
			PropertyXML propertyMapXml = new PropertyXML();
			propertyMapXml.name = "theme";
			propertyMapXml.value = map.theme;
			mapXml.propertiesXml.propertiesXml.Add(propertyMapXml);

			XmlSerializer serializer = new XmlSerializer(typeof(MapXML));
			using (FileStream stream = new FileStream(path + filename + ".tmx", FileMode.Create))
			{
				serializer.Serialize(stream, mapXml);
			}
		}

		/// <summary>
		/// loads a TMX (Tile Map XML) file and creates a map object out of it.
		/// By now it can only handle no compression and CSV encoding.
		/// Warning: may throw exceptions, occuring during filereading parsing of the data! the latter could happen when the file is corrupted/not edited properly
		/// Warning: this will only work for standalone builds. (TODO: test for Mac and Linux)
		/// </summary>
		/// <returns>
		/// The map.
		/// </returns>
		/// <param name='path'>
		/// The path from where the file should be loaded. Make sure that the last character is a slash!
		/// </param>
		/// <param name='filename'>
		/// The name of the TMX file without extension.
		/// </param>
		public Map loadMapFromTmx(string path, string filename)
		{
			MapXML mapXml;

			XmlSerializer serializer = new XmlSerializer(typeof(MapXML));
			using (FileStream stream = new FileStream(path + filename + ".tmx", FileMode.Open))
			{
				mapXml = serializer.Deserialize(stream) as MapXML;
			}

			//extract map info
			Map map;
			map = new Map(mapXml.width, mapXml.height);
			if (mapXml.propertiesXml != null && mapXml.propertiesXml.propertiesXml != null)
			{
				PropertyXML propertyXml = mapXml.propertiesXml.propertiesXml.Find(p => p.name.Equals("theme"));
				if (propertyXml.value != null) map.theme = propertyXml.value;
			}

			//extract the types
			List<string> types = new List<string>();

			foreach (TileXML tileXml in mapXml.tilesetXml.tilesXml)
			{
				if (tileXml.propertiesXml != null && tileXml.propertiesXml.propertiesXml != null)
				{
					PropertyXML propertyXml = tileXml.propertiesXml.propertiesXml.Find(p => p.name.Equals("type"));
					if (propertyXml.value != null) types.Add(propertyXml.value);
				}
			}

			string mapAsString;
			string[] mapAsStringArray = null;

			if (mapXml.layerXml != null && mapXml.layerXml.dataXml != null)
			{
				mapAsString = mapXml.layerXml.dataXml.text;
				if (mapAsString != null)
					mapAsStringArray = mapAsString.Split(',');
			}

			if (mapAsStringArray != null && mapAsStringArray.Length == map.size_X * map.size_Y)
			{
				for (int y = 0; y < map.size_Y; y++)
				{
					for (int x = 0; x < map.size_X; x++)
					{
						int typeIdx = Convert.ToInt32(mapAsStringArray[x + map.size_X * (map.size_Y - 1 - y)]); // reverse y axis to match Tiled's coordinate system
						if (typeIdx > 0 && typeIdx <= types.Count)
							map.GetCell(x, y).SetCellType(types[typeIdx - 1]);  // read idx minus one because thats what tmx does appearently
						else
							Debug.Log("corrupted data in the tmx file!");

					}
				}
			}
			else
			{
				Debug.Log("corrupted data in the tmx file!");
				return null;
			}
			return map;
		}

	}
	/// <summary>
	///this namespace and the contained classes are used to serialize a map in the TMX (Tile Map XML) format
	///note that these classes do not contain all attributes and elements defined in the TMX definition, only those that are of use for the ProD maps
	/// </summary>
	namespace TMX
	{

		[XmlRoot("map")]
		public class MapXML
		{
			[XmlAttribute("version")]
			public string version;
			[XmlAttribute("orientation")]
			public string orientation;
			[XmlAttribute("width")]
			public int width;
			[XmlAttribute("height")]
			public int height;
			[XmlAttribute("tilewidth")]
			public int tilewidth;
			[XmlAttribute("tileheight")]
			public int tileheight;

			[XmlElement("tileset")]
			public TilesetXML tilesetXml;

			[XmlElement("layer")]
			public LayerXML layerXml;

			[XmlElement("properties")]
			public PropertiesXML propertiesXml;
		}

		public class TilesetXML
		{
			[XmlAttribute("firstgid")]
			public int firstgid;
			[XmlAttribute("name")]
			public string name;
			[XmlAttribute("tilewidth")]
			public int tilewidth;
			[XmlAttribute("tileheight")]
			public int tileheight;
			[XmlAttribute("spacing")]
			public int spacing;
			[XmlAttribute("margin")]
			public int margin;

			[XmlElement("image")]
			public ImageXML imageXml;

			[XmlElement("tile")]
			public List<TileXML> tilesXml;
		}

		public class ImageXML
		{
			[XmlAttribute("source")]
			public string source;
		}

		public class TileXML
		{
			[XmlAttribute("id")]
			public int id;

			[XmlElement("properties")]
			public PropertiesXML propertiesXml;
		}

		public class PropertiesXML
		{
			[XmlElement("property")]
			public List<PropertyXML> propertiesXml;
		}

		public class PropertyXML
		{
			[XmlAttribute("name")]
			public string name;
			[XmlAttribute("value")]
			public string value;
		}

		public class LayerXML
		{
			[XmlAttribute("name")]
			public string name;
			[XmlAttribute("width")]
			public int width;
			[XmlAttribute("height")]
			public int height;


			[XmlElement("data")]
			public DataXML dataXml;
		}

		public class DataXML
		{
			[XmlAttribute("encoding")]
			public string encoding;

			[XmlText()]
			public string text;
		}
	}

}


//public class XML_Loader : MonoBehaviour
//{
//    public static void Save(string path, LevelXML level)
//    {

//    }

//    public static LevelXML Load(string path)
//    {
//        XmlSerializer serializer = new XmlSerializer(typeof(LevelXML));
//        using(FileStream stream = new FileStream(path, FileMode.Open))
//        {
//            return serializer.Deserialize(stream) as LevelXML;
//        }
//    }

//    public static LevelXML LoadFromResources(string path)
//    {
//        XmlSerializer serializer = new XmlSerializer(typeof(LevelXML));
//        TextAsset ta = Resources.Load<TextAsset>(path);
//        Stream s = new MemoryStream(ta.bytes);
//        return serializer.Deserialize(s) as LevelXML;
//    }
//} 