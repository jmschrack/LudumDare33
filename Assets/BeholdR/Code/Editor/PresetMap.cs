using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Beholder.Editor
{
	/// <summary>
	/// Responsible for saving references to the Preset prefabs
	/// </summary>
	[Serializable]
	public class PresetMap : ScriptableObject
	{
		public BeholdR TargetBeholdR;

		[SerializeField]
		public List<GameObject> Presets = new List<GameObject>();

		/// <summary>
		/// Save the post effect component stack of the <see cref="source"/> <see cref="BeholdR"/> object and add it to the <see cref="Presets"/> list
		/// </summary>
		/// <param name="source">The <see cref="BeholdR"/> object whose state we want to save as a preset</param>
		/// <param name="presetName">The name that will be given to this preset</param>
		public void CreatePreset(BeholdR source, string presetName)
		{
			// create new game object and copy the components from the source
			GameObject preset = new GameObject(presetName);
			BeholdR presetBeholdR = preset.AddComponent<BeholdR>();
			presetBeholdR.enabled = false;

			foreach (Component postEffect in source.PostEffects)
			{
				if(postEffect == null) continue;

				Component target = preset.AddComponent(postEffect.GetType());
				EditorUtility.CopySerialized(postEffect, target);

				presetBeholdR.PostEffects.Add(target);
			}

			// save the object to file
			string presetPath = Utilities.BeholdRFolderPath + Utilities.ASSETS_SUBDIR + Utilities.PRESETS_SUBDIR + presetName + ".prefab";
			GameObject presetPrefab = PrefabUtility.CreatePrefab(presetPath, preset);
			presetPrefab.hideFlags = HideFlags.HideInHierarchy;
			
			// add as preset and remove
			Presets.Add(presetPrefab);
			DestroyImmediate(preset);

			// refresh asset database to show new preset
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			source.enabled = true;
		}

		/// <summary>
		/// Load the components configuration from the <see cref="preset"/> object to the <see cref="target"/> <see cref="BeholdR"/> object
		/// </summary>
		/// <param name="target">The <see cref="BeholdR"/> object we want to load the preset into</param>
		/// <param name="preset">The saved post effects stack preset we want to load</param>
		public void LoadPreset(BeholdR target, GameObject preset)
		{
			Undo.RegisterFullObjectHierarchyUndo(target.gameObject, "Load Preset");

			// disable BeholdR
			target.enabled = false;

			// remove all current PostEffects
			foreach(Component postEffect in target.PostEffects)
			{
				DestroyImmediate(postEffect);
			}

			target.PostEffects.Clear();

			// from the preset, add all PostEffect components and copy serialized their values
			BeholdR presetBeholdR = preset.GetComponent<BeholdR>();
			foreach(Component postEffect in presetBeholdR.PostEffects)
			{
				Component nEffect = target.gameObject.AddComponent(postEffect.GetType());
				EditorUtility.CopySerialized(postEffect, nEffect);
			}

			// restore BeholdR
			target.enabled = true;
		}
	}
}