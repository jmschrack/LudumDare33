using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Beholder.Editor
{
	/// <summary>
	/// The custom Inspector for the <see cref="BeholdR"/> component
	/// </summary>
	[CustomEditor(typeof(BeholdR))]
	public class BeholdRInspector : UnityEditor.Editor
	{
		#region Data Members
		private BeholdR _beholdR;

		private Texture2D _icon;
		private Texture2D Icon
		{
			get
			{
				return _icon ?? (_icon = Utilities.GetAsset<Texture2D>(Utilities.ICONS_SUBDIR + "BeholdR.png"));
			}
		}

		private GUIStyle _foldStyle;

		#region Sections Visibility
		private static bool _isControlSectionVisible = true;
		private static bool _isLinkSectionVisible = true;
		private static bool _isPresetSectionVisible = true;
		private static bool _isSyncSectionVisible = true;
		#endregion

		// TODO: extract this into another class...
		#region Version Check
		private const string VERSION_CHECK_URL = @"https://www.virtual-mirror.com/version_check.txt";
		private const string PRODUCT_KEY = "BeholdR";
		private const string LOCAL_VERSION = "5.0.0";
		private int _localVersion = -1;
		private int GetLocalVersion
		{
			get
			{
				if(_localVersion < 0)
				{
					int.TryParse(LOCAL_VERSION.Replace(".", ""), out _localVersion);
				}

				return _localVersion;
			}
		}

		private string _normalHostUrl;
		private static WWW _versionRequest;
		private static string _versionText;
		private static string GetVersion
		{
			get
			{
				// request version from web server if needed
				if(string.IsNullOrEmpty(_versionText))
				{
					if(_versionRequest == null || !_versionRequest.isDone || !string.IsNullOrEmpty(_versionRequest.error))
					{
						_versionText = string.Empty;
					}
					else
					{
						foreach(var productLine in _versionRequest.text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
						{
							var split = productLine.Split(';');
							if(split[0] == PRODUCT_KEY)
							{
								_versionText = split[1];
							}
						}
					}
				}

				return _versionText;
			}
		}

		private int _remoteVersion = -1;
		private int GetRemoteVersion
		{
			get
			{
				if(_remoteVersion < 0)
				{
					int.TryParse(GetVersion.Replace(".", ""), out _remoteVersion);
				}

				return _remoteVersion;
			}
		}
		#endregion

		#region PostEffects List
		private ReorderableList _reordableFxList;
		private static bool _isListVisible = true;
		#endregion

		#region Preset List
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

		private string _presetName = "PostEffectsSet";
		#endregion
		#endregion

		#region Unity Events
		/// <summary>
		/// Called whenever the inspector for a BeholdR object is initialized, usually when selecting a camera with BeholdR component
		/// </summary>
		void OnEnable()
		{
			_beholdR = (BeholdR)target;

			InitializeReordableList();
		}

		public override void OnInspectorGUI()
		{
			DrawAboutSection();
			DrawVersionSection();
			DrawControlSection();
			DrawLinkSection();
			DrawPresetSection();
			DrawSyncSection();

			serializedObject.ApplyModifiedProperties();

			RefreshOnChange();
		}
		#endregion

		#region Reordered List
		private void InitializeReordableList()
		{
			_reordableFxList = new ReorderableList(serializedObject, serializedObject.FindProperty("PostEffects"), true, true, true,
				true)
			{
				drawHeaderCallback = DrawListHeader,
				drawElementCallback = DrawListElement,
				onChangedCallback = ReorderActualEffects
			};
		}

		/// <summary>
		/// Mark that we need to reorder the components on the object to reflect a change in the list
		/// </summary>
		/// <param name="list"></param>
		private void ReorderActualEffects(ReorderableList list)
		{
			_beholdR.OrderNeeded = true;
		}

		/// <summary>
		/// Draw each of the list's elements as an assignable property field
		/// </summary>
		/// <param name="rect">The drawing area</param>
		/// <param name="index">The index of the element in the list</param>
		/// <param name="isactive">Is this the active element</param>
		/// <param name="isfocused">Is this the focused element</param>
		private void DrawListElement(Rect rect, int index, bool isactive, bool isfocused)
		{
			// fixing slight height issue
			rect.y += 2;
			rect.height = EditorGUIUtility.singleLineHeight;

			// draw an assignable post effect field
			SerializedProperty element = serializedObject.FindProperty("PostEffects").GetArrayElementAtIndex(index);

			string label = string.Empty;
			if(_beholdR.PostEffects[index] != null)
			{
				label = _beholdR.PostEffects[index].GetType().FullName.Split('.').Last();
				EditorGUI.LabelField(rect, label);
			}

			EditorGUI.PropertyField(rect, element, new GUIContent(label));
		}

		/// <summary>
		/// Draw the header line of the reorder-able post effects list
		/// </summary>
		/// <param name="rect">The rectangle of the GUI content of the list's header</param>
		private void DrawListHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, "Post Effects", EditorStyles.boldLabel);
		}
		#endregion

		#region Inspector Sections
		/// <summary>
		/// A short header section with some information about BeholdR
		/// </summary>
		private void DrawAboutSection()
		{
			EditorGUILayout.BeginHorizontal("box");
			{
				GUILayout.Label(Icon, GUILayout.Width(Icon.width), GUILayout.Height(Icon.height));
				GUILayout.Label("BeholdR v" + LOCAL_VERSION + "\n(c) Virtual Mirror Game Studios ltd. 2011-2015", EditorStyles.miniLabel);
			}
			EditorGUILayout.EndHorizontal();
		}

		/// <summary>
		/// notify the user if their local version of BeholdR is outdated
		/// </summary>
		private void DrawVersionSection()
		{
			if(_versionRequest == null)
			{
				// cache and overwrite the host URL for this version check
				_normalHostUrl = EditorSettings.webSecurityEmulationHostUrl;
				EditorSettings.webSecurityEmulationHostUrl = VERSION_CHECK_URL;

				// make version request
				_versionRequest = new WWW(VERSION_CHECK_URL);

				// reset host URL
				EditorSettings.webSecurityEmulationHostUrl = _normalHostUrl;

				return;
			}

			// wait until request has completed
			if(!_versionRequest.isDone) return;

			// show warning that local version may be out of date
			if(GetRemoteVersion > GetLocalVersion)
			{
				EditorGUILayout.HelpBox("Your version of BeholdR may be outdated! The newest available version is " + GetVersion, MessageType.Warning);
			}
		}

		/// <summary>
		/// The section related to the in-scene control panel
		/// </summary>
		private void DrawControlSection()
		{
			EditorGUILayout.BeginVertical("box");
			{
				_isControlSectionVisible = DrawToggledHeader("Control Panel", _isControlSectionVisible);

				// draw the controls section if it's not collapsed
				if(_isControlSectionVisible)
				{
					bool _oldShow = _beholdR.ShowGuiControls;
					_beholdR.ShowGuiControls = EditorGUILayout.Toggle("Show Controls", _beholdR.ShowGuiControls);
					if(_oldShow != _beholdR.ShowGuiControls) SceneView.RepaintAll();

					_beholdR.ControlsAnchor = (TextAnchor)EditorGUILayout.EnumPopup("Controls Anchor", _beholdR.ControlsAnchor);
				}
			}
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// The section related to Scene View linking such as camera background color match
		/// </summary>
		private void DrawLinkSection()
		{
			EditorGUILayout.BeginVertical("box");
			{
				_isLinkSectionVisible = DrawToggledHeader("Scene Link", _isLinkSectionVisible);

				// draw the link section if it's not collapsed
				if(_isLinkSectionVisible)
				{
					// informative only
					GUI.enabled = false;
					EditorGUILayout.Toggle("Linked to a Scene?", _beholdR.LinkedSceneView != null);
					GUI.enabled = true;

					// color matching
					bool lastMatch = _beholdR.MatchCameraColor;
					_beholdR.MatchCameraColor = EditorGUILayout.Toggle("Match Background Color", _beholdR.MatchCameraColor);

					// cache if needed (before activation)
					if(_beholdR.MatchCameraColor && !lastMatch)
					{
						BeholdR.CacheSceneColor();
					}
					// restore if needed (after deactivation)
					else if(!_beholdR.MatchCameraColor && lastMatch)
					{
						BeholdR.ResetSceneColor();
					}

					// clipping planes matching (used in the experimental scene view)
					//_beholdR.MatchClippingPlanes = EditorGUILayout.Toggle("Match Clip Planes", _beholdR.MatchClippingPlanes);
				}
			}
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// The section related to saving and loading PostEffects composition presets
		/// </summary>
		private void DrawPresetSection()
		{
			EditorGUILayout.BeginVertical("box");
			{
				_isPresetSectionVisible = DrawToggledHeader("Presets", _isPresetSectionVisible);

				if(_isPresetSectionVisible)
				{
					_presetName = EditorGUILayout.TextField("Preset Name", _presetName);

					EditorGUILayout.BeginHorizontal();
					{
						// create new preset
						if(GUILayout.Button("Save Preset"))
						{
							//SaveBeholdrPreset();
							if(PresetMap != null)
							{
								PresetMap.CreatePreset(_beholdR, _presetName);
							}
						}

						// load preset
						if(GUILayout.Button("Load Preset"))
						{
							if(PresetMap != null)
							{
								PresetMap.TargetBeholdR = _beholdR;
								Selection.activeObject = PresetMap;
							}
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// The section related to component synchronization such as the post effects list and helper buttons
		/// </summary>
		private void DrawSyncSection()
		{
			EditorGUILayout.BeginVertical("box");
			{
				_isSyncSectionVisible = DrawToggledHeader("Post Effects", _isSyncSectionVisible);

				// draw the sync section if it's not collapsed
				if(_isSyncSectionVisible)
				{
					if(!_beholdR.IsSyncSupressed)
					{
						// auto discovery
						bool value = EditorGUILayout.Toggle("Auto Discover", BeholdR.AutoDiscover);
						if(value != BeholdR.AutoDiscover) _beholdR.SkipFrame = true;
						BeholdR.AutoDiscover = value;

						// auto disable
						BeholdR.AutoDisableInPlayMode = EditorGUILayout.Toggle("Auto Disable in Play", BeholdR.AutoDisableInPlayMode);

						_isListVisible = DrawToggledHeader("Effects List", _isListVisible, false);
						if(_isListVisible)
						{
							// when inside an OnInspectorGUI call, Screen.width/height returns the dimensions of the window the Inspector is docked in
							Rect rect = GUILayoutUtility.GetRect(GUILayoutUtility.GetLastRect().width, _reordableFxList.GetHeight());
							_reordableFxList.DoList(rect);
						}

						// helper buttons
						EditorGUILayout.BeginHorizontal();
						{
							if(GUILayout.Button("Discover FX"))
							{
								_beholdR.TryAutoCollectPostFx();
								serializedObject.Update();
								SceneView.RepaintAll();
							}

							if(GUILayout.Button("Clean Missing"))
							{
								_beholdR.CleanPostEffectsList();
								serializedObject.Update();
							}

							if(GUILayout.Button("Force Reorder"))
							{
								_beholdR.SkipFrame = _beholdR.OrderNeeded = true;
							}
						}
						EditorGUILayout.EndHorizontal();
					}
					else
					{
						// informational message to explain why BeholdR is not working
						EditorGUILayout.HelpBox(BeholdR.SUPPRESSION_MESSAGE, MessageType.Error);
					}
				}
			}
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// Draws a folding header for the GUI section that the user can click on to fold/unfold the section
		/// </summary>
		/// <param name="header">Section name</param>
		/// <param name="isFolded">The folded state of the section right now</param>
		/// <param name="isBold">Should the header text be bold?</param>
		/// <returns>The folded state of the section after user interaction</returns>
		private bool DrawToggledHeader(string header, bool isFolded, bool isBold = true)
		{
			// set styling for bold label
			_foldStyle = new GUIStyle(GUI.skin.FindStyle("Foldout"))
			{
				richText = true
			};

			if(isBold)
			{
				header = "<b>" + header + "</b>";
			}

			// draw the header
			isFolded = GUILayout.Toggle(isFolded, header, _foldStyle);

			return isFolded;
		}
		#endregion

		/// <summary>
		/// Used to determine if there was a change to the inspector that requires a refresh to BeholdR
		/// </summary>
		private void RefreshOnChange()
		{
			if(GUI.changed)
			{
				if(_beholdR != null)
				{
					_beholdR.CleanAllSceneCameras();
					_beholdR.UpdateBeholdR();
				}
			}
		}
	}
}