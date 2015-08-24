using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beholder.Editor
{
	/// <summary>
	/// Display all the presets next to each other for easy comparison and adjustments
	/// </summary>
	public class PresetMatrixWindow : EditorWindow
	{
		/// <summary>
		/// Launch and initialize the editor window
		/// </summary>
		[MenuItem("Window/BeholdR/Preset Matrix Window")]
		static void OpenMatrixWindow()
		{
			GetWindow<PresetMatrixWindow>("Preset Matrix", true);
		}

		#region Data Members
		private Vector2 _scrollPosition;

		private PresetMap _presetMap;
		private PresetMap PresetMap
		{
			get
			{
				if(_presetMap == null)
				{
					// get the existing one
					_presetMap = Utilities.GetAsset<PresetMap>(Utilities.PRESETS_SUBDIR + "_PresetMap.asset");

					// create a new one
					if(_presetMap == null)
					{
						AssetDatabase.CreateAsset(CreateInstance<PresetMap>(), Utilities.BeholdRFolderPath + Utilities.ASSETS_SUBDIR + Utilities.PRESETS_SUBDIR + "_PresetMap.asset");
						_presetMap = Utilities.GetAsset<PresetMap>(Utilities.PRESETS_SUBDIR + "_PresetMap.asset");
					}
				}

				return _presetMap;
			}
		}

		private readonly List<Type> _uniqueTypes = new List<Type>();

		private const float COL_WIDTH = 260;
		#endregion

		#region Editor Window Events
		/// <summary>
		/// Called after the window has been created, used for initialization
		/// </summary>
		void OnEnable()
		{
			// extract unique types list
			foreach(GameObject presetObject in PresetMap.Presets)
			{
				foreach(Component postEffect in presetObject.GetComponent<BeholdR>().PostEffects)
				{
					Type postEffecType = postEffect.GetType();
					if(!_uniqueTypes.Contains(postEffecType))
					{
						_uniqueTypes.Add(postEffecType);
					}
				}
			}
		}

		/// <summary>
		/// Draw the contents of the window
		/// </summary>
		void OnGUI()
		{
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
			{
				// sort by rows first
				EditorGUILayout.BeginVertical();
				{
					DrawPresetTitlesRow();
					DrawComponentRows();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndScrollView();
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Draw a row with each of the preset's name as the title cell of the column
		/// </summary>
		private void DrawPresetTitlesRow()
		{
			EditorGUILayout.BeginHorizontal("box");
			{
				// empty first space
				GUILayout.Box(string.Empty, GUILayout.Width(COL_WIDTH));

				// titles
				foreach(GameObject presetObject in PresetMap.Presets)
				{
					GUILayout.Box(presetObject.name, GUILayout.Width(COL_WIDTH));
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		/// <summary>
		/// Draw each of the unique components in a row of it's own
		/// </summary>
		private void DrawComponentRows()
		{
			foreach(Type type in _uniqueTypes)
			{
				// begin row
				EditorGUILayout.BeginHorizontal("box");
				{
					// component type title cell
					GUILayout.Box(type.Name, GUILayout.Width(COL_WIDTH));

					// components content
					foreach(GameObject presetObject in PresetMap.Presets)
					{
						bool drawn = false;
						foreach(Component postEffect in presetObject.GetComponent<BeholdR>().PostEffects)
						{
							if(postEffect.GetType() == type)
							{
								drawn = true;
								EditorGUILayout.BeginVertical("box", GUILayout.Width(COL_WIDTH));
								{
									DrawComponent(postEffect);

									GUI.color = Color.red;
									if(GUILayout.Button("Remove from preset"))
									{
										DestroyImmediate(postEffect, true);
									}
									GUI.color = Color.white;
								}
								EditorGUILayout.EndVertical();
							}
						}

						// in case we destroyed post effects, we need to clean the list of null elements
						presetObject.GetComponent<BeholdR>().CleanPostEffectsList();

						// in case we didn't draw the component for this preset, draw an empty cell to preserve space
						if(!drawn)
						{
							EditorGUILayout.BeginVertical("box", GUILayout.Width(COL_WIDTH));
							{
								GUILayout.Box("Component Missing", GUILayout.ExpandWidth(true));

								GUI.color = Color.green;
								if(GUILayout.Button("Add " + type.Name + " to preset"))
								{
									Component added = presetObject.AddComponent(type);
									presetObject.GetComponent<BeholdR>().PostEffects.Add(added);
								}
								GUI.color = Color.white;
							}
							EditorGUILayout.EndVertical();
						}
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// Draw the component's public fields in a way that the user can edit
		/// </summary>
		/// <param name="component">The component whose fields we're drawing</param>
		private static void DrawComponent(Component component)
		{
			Type componentType = component.GetType();
			FieldInfo[] typeFields = componentType.GetFields(BindingFlags.Public | BindingFlags.Instance);

			foreach(FieldInfo fieldInfo in typeFields)
			{
				DrawField(fieldInfo, component);
			}
		}

		/// <summary>
		/// Draw the correct editor field from the given info
		/// </summary>
		/// <param name="fieldInfo">Contains all the information about the field so we can draw it properly</param>
		/// <param name="sourceComponent">The component whose field we're drawing, used for getting/setting the field's value</param>
		private static void DrawField(FieldInfo fieldInfo, Component sourceComponent)
		{
			Type fieldType = fieldInfo.FieldType;
			string fieldName = fieldInfo.Name;

			// whole numbers
			if(fieldType == typeof(int))
			{
				object value = EditorGUILayout.IntField(fieldName, (int)fieldInfo.GetValue(sourceComponent));
				fieldInfo.SetValue(sourceComponent, value);
			}
			// fractional numbers
			else if(fieldType == typeof(float) || fieldType == typeof(double))
			{
				object value = EditorGUILayout.FloatField(fieldName, (float)fieldInfo.GetValue(sourceComponent));
				fieldInfo.SetValue(sourceComponent, value);
			}
			// text
			else if(fieldType == typeof(string))
			{
				EditorGUILayout.BeginVertical("box");
				{
					EditorGUILayout.LabelField(fieldName, EditorStyles.boldLabel);
					object value = EditorGUILayout.TextArea((string)fieldInfo.GetValue(sourceComponent));
					fieldInfo.SetValue(sourceComponent, value);
				}
				EditorGUILayout.EndVertical();
			}
			// booleans
			else if(fieldType == typeof(bool))
			{
				object value = EditorGUILayout.Toggle(fieldName, (bool)fieldInfo.GetValue(sourceComponent));
				fieldInfo.SetValue(sourceComponent, value);
			}
			// enumerations and flags
			else if(fieldType.IsEnum)
			{
				if(fieldType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
				{
					object value = EditorGUILayout.EnumMaskField(fieldName, (Enum)fieldInfo.GetValue(sourceComponent));
					fieldInfo.SetValue(sourceComponent, value);
				}
				else
				{
					object value = EditorGUILayout.EnumPopup(fieldName, (Enum)fieldInfo.GetValue(sourceComponent));
					fieldInfo.SetValue(sourceComponent, value);
				}
			}
			// bounds
			else if(fieldType == typeof(Bounds))
			{
				object value = EditorGUILayout.BoundsField(fieldName, (Bounds)fieldInfo.GetValue(sourceComponent));
				fieldInfo.SetValue(sourceComponent, value);
			}
			// colors
			else if(fieldType == typeof(Color))
			{
				object value = EditorGUILayout.ColorField(fieldName, (Color)fieldInfo.GetValue(sourceComponent));
				fieldInfo.SetValue(sourceComponent, value);
			}
			// curves
			else if(fieldType == typeof(AnimationCurve))
			{
				object value = EditorGUILayout.CurveField(fieldName, (AnimationCurve)fieldInfo.GetValue(sourceComponent));
				fieldInfo.SetValue(sourceComponent, value);
			}
			// rectangles
			else if(fieldType == typeof(Rect))
			{
				object value = EditorGUILayout.RectField(fieldName, (Rect)fieldInfo.GetValue(sourceComponent));
				fieldInfo.SetValue(sourceComponent, value);
			}
			// vector 2
			else if(fieldType == typeof(Vector2))
			{
				object value = EditorGUILayout.Vector2Field(fieldName, (Vector2)fieldInfo.GetValue(sourceComponent));
				fieldInfo.SetValue(sourceComponent, value);
			}
			// vector 3
			else if(fieldType == typeof(Vector3))
			{
				object value = EditorGUILayout.Vector3Field(fieldName, (Vector3)fieldInfo.GetValue(sourceComponent));
				fieldInfo.SetValue(sourceComponent, value);
			}
			// vector 4
			else if(fieldType == typeof(Vector4))
			{
				object value = EditorGUILayout.Vector4Field(fieldName, (Vector4)fieldInfo.GetValue(sourceComponent));
				fieldInfo.SetValue(sourceComponent, value);
			}
			// shader
			else if(fieldType == typeof(Shader))
			{
				if(!((Shader)fieldInfo.GetValue(sourceComponent)).name.Contains("Hidden"))
				{
					object value = EditorGUILayout.ObjectField(fieldName, (Shader)fieldInfo.GetValue(sourceComponent), fieldType, true);
					fieldInfo.SetValue(sourceComponent, value);
				}
			}
			// inheriting from UnityEngine.Object
			else if(fieldType.IsSubclassOf(typeof(Object)) || fieldType == typeof(Object))
			{
				object value = EditorGUILayout.ObjectField(fieldName, (Object)fieldInfo.GetValue(sourceComponent), fieldType, true);
				fieldInfo.SetValue(sourceComponent, value);
			}
			// everything else is not supported
			else
			{
				EditorGUILayout.LabelField(fieldName + " Unsupported parameter type: " + fieldType.Name);
			}
		}
		#endregion
	}
}
