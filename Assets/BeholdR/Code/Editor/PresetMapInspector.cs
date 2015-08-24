using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Beholder.Editor
{
	/// <summary>
	/// Provides custom Inspector for the <see cref="PresetMap"/> asset
	/// </summary>
	[CustomEditor(typeof(PresetMap))]
	public class PresetMapInspector : UnityEditor.Editor
	{
		private PresetMap _presetMap;
		private ReorderableList _presetList;

		#region Unity Events
		void OnEnable()
		{
			_presetMap = (PresetMap)target;

			// gather all presets from the working folder?

			_presetList = new ReorderableList(serializedObject, serializedObject.FindProperty("Presets"), true, true, false, true)
			{
				drawHeaderCallback = DrawListHeader,
				drawElementCallback = DrawListElement
			};
		}
		#endregion

		#region Preset List Drawing
		/// <summary>
		/// Draw the list header/name
		/// </summary>
		/// <param name="rect">The drawing area allocated by the list</param>
		private void DrawListHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, "Presets", EditorStyles.boldLabel);
		}

		/// <summary>
		/// Draw the body of each element in the list; in this case the name of the preset
		/// </summary>
		/// <param name="rect">The drawing area allocated by the list</param>
		/// <param name="index">The index of the element in the list</param>
		/// <param name="isactive">True if this is the actively chosen element</param>
		/// <param name="isfocused">True if this element is in focus</param>
		private void DrawListElement(Rect rect, int index, bool isactive, bool isfocused)
		{
			rect.y += 2;
			rect.height = EditorGUIUtility.singleLineHeight;

			rect.width /= 2;

			SerializedProperty element = _presetList.serializedProperty.GetArrayElementAtIndex(index);
			try
			{
				EditorGUI.LabelField(rect, element.objectReferenceValue.name);
				rect.x += rect.width;
				if(GUI.Button(rect, "load"))
				{
					((PresetMap)target).LoadPreset(_presetMap.TargetBeholdR, (GameObject)element.objectReferenceValue);
				}
			}
			catch(Exception)
			{
				EditorGUI.LabelField(rect, "Missing");
			}
		}
		#endregion

		#region Overrides of Editor
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			// set the active BeholdR to which we'll load the preset
			_presetMap.TargetBeholdR = EditorGUILayout.ObjectField("Target BeholdR", _presetMap.TargetBeholdR, typeof(BeholdR), true) as BeholdR;

			// draw the preset list
			_presetList.DoLayoutList();

			// allow us to clear the list
			if(GUILayout.Button("Clear All"))
			{
				for(int i = 0; i < _presetList.count; i++)
				{
					SerializedProperty element = _presetList.serializedProperty.GetArrayElementAtIndex(i);
					string path = AssetDatabase.GetAssetPath(element.objectReferenceValue);
					AssetDatabase.DeleteAsset(path);
				}

				_presetList.serializedProperty.ClearArray();
			}

			serializedObject.ApplyModifiedProperties();
		}
		#endregion
	}
}