using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Beholder
{
	/// <summary>
	/// Contains various utility methods and constants for BeholdR
	/// </summary>
	public static class Utilities
	{
#if UNITY_EDITOR
		#region Constants
		internal const BindingFlags DEFAULT_BINDING_FLAG = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
		public const string ASSETS_SUBDIR = "Assets/";
		public const string ICONS_SUBDIR = "Icons/";
		public const string PRESETS_SUBDIR = "Presets/";
		#endregion

		#region Data Members
		private static string _folderPath;
		public static string BeholdRFolderPath
		{
			get
			{
				if(string.IsNullOrEmpty(_folderPath))
				{
					GetBeholdrFolderPath(Application.dataPath, ref _folderPath);
				}

				return _folderPath;
			}
		}

		private readonly static Dictionary<string, Type> TypesCache = new Dictionary<string, Type>();
		#endregion

		#region Extension Methods
		/// <summary>
		/// Return the first instance of the component of type <see cref="type"/> found on the game object
		/// ---- NOTE: THIS METHOD IS OBSOLETE IN UNITY 5 ----
		/// </summary>
		/// <param name="self">the game object we're looking at</param>
		/// <param name="type">the type of the component we're looking for</param>
		/// <returns>the first instance of the component whose type equals the desired type, or null if none were found</returns>
		public static Component GetComponent(this GameObject self, Type type)
		{
			foreach(Component comp in self.GetComponents<Component>())
			{
				if(comp.GetType() == type) return comp;
			}

			return null;
		}

		/// <summary>
		/// Check to see if the <see cref="source"/> contains any element that is not null
		/// </summary>
		/// <param name="source">The collection we want to check</param>
		/// <returns>True if there is any element in the collection that is not null</returns>
		public static bool AnyConcrete(this IList source)
		{
			if(source.Count == 0) return false;

			foreach(object element in source)
			{
				if(element != null) return true;
			}

			return false;
		}
		#endregion

		#region Public API
		/// <returns>True if component on scene camera is of a forbidden type</returns>
		public static bool IsForbiddenComponent(Camera sceneCamera, Component component)
		{
			if(	component == null ||
				component is Transform ||
				component is Camera ||
				component is BeholdR ||
				component is FlareLayer) return true;
			if(component == sceneCamera.GetComponent("HaloLayer")) return true;

			return false;
		}

		/// <summary>
		/// Looks for a type with the specified type name in the currently loaded assemblies
		/// </summary>
		/// <param name="typeName">The full name of the type we are looking for</param>
		/// <returns>The Type, if found</returns>
		public static Type FindTypeByName(string typeName)
		{
			Type t;

			// look at the cache first
			if(!TypesCache.TryGetValue(typeName, out t))
			{
				// find type
				t = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(assembly => assembly.GetTypes())
					.FirstOrDefault(type => type.FullName == typeName);

				// cache
				TypesCache[typeName] = t;
			}

			// return
			return t;
		}

		/// <summary>
		/// Checks all open inspectors to see if there is an opened inspector of the given type
		/// </summary>
		/// <param name="typeName">The type of inspector we are looking for by name</param>
		/// <returns>true if there is any inspector window with an open inspector of the given type</returns>
		public static bool IsEditorVisible(string typeName)
		{
			bool visible = false;

			Type inspectorT = FindTypeByName("UnityEditor.InspectorWindow");

			if(inspectorT == null) return false;

			// iterate on all inspector windows
			Object[] inspectors = Resources.FindObjectsOfTypeAll(inspectorT);
			foreach(Object inspector in inspectors)
			{
				// get the inspector tracker
				FieldInfo fieldInfo = inspectorT.GetField("m_Tracker", DEFAULT_BINDING_FLAG);
				if(fieldInfo == null) return false;

				object tracker = fieldInfo.GetValue(inspector);
				if(tracker == null) continue;

				// look at the open (active) inspectors for the provided type
				object shared = tracker.GetType().GetProperty("activeEditors").GetValue(tracker, null);
				foreach(object thing in (IEnumerable)shared)
				{
					if(thing.GetType().FullName != typeName) continue;

					object target = thing.GetType().GetProperty("target").GetValue(thing, null);
					visible = InternalEditorUtility.GetIsInspectorExpanded(target as Object);
				}
			}

			return visible;
		}

		/// <summary>
		/// Loads and returns the first asset with the given name found somewhere in the InvokR assets folder
		/// </summary>
		/// <typeparam name="T">The type of the object that will be loaded from the asset</typeparam>
		/// <param name="relativePath">The relative path to the asset under the BeholdR/Assets folder</param>
		/// <returns>An object of type T loaded from the asset file</returns>
		public static T GetAsset<T>(string relativePath) where T : Object
		{
			try
			{
				return (T)AssetDatabase.LoadAssetAtPath(string.Concat(BeholdRFolderPath, ASSETS_SUBDIR, relativePath), typeof(T));
			}
			catch(Exception)
			{

			}

			return null;
		}

		/// <summary>
		/// Perform a recursive searching the project folder in a depth-first manner 
		/// until we find the relative root BeholdR folder path
		/// </summary>
		/// <param name="rootPath">The current root folder in which we're searching</param>
		/// <param name="beholdrPath">This is a REF parameter that will be set to the path of the local BeholdR root folder</param>
		private static void GetBeholdrFolderPath(string rootPath, ref string beholdrPath)
		{
			if(!string.IsNullOrEmpty(beholdrPath)) return;

			foreach(string subDirPath in Directory.GetDirectories(rootPath))
			{
				if(!string.IsNullOrEmpty(beholdrPath)) break;

				string localPath = subDirPath.Replace('\\', '/').Split(new[] { "Assets/" }, StringSplitOptions.RemoveEmptyEntries).Last();
				if(localPath.Contains("BeholdR"))
				{
					beholdrPath = "Assets/" + localPath + "/";
					return;
				}

				GetBeholdrFolderPath(subDirPath, ref beholdrPath);
			}
		}
		#endregion

#endif
	}
}