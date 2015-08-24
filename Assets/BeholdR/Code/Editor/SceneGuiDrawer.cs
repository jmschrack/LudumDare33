using UnityEngine;
using UnityEditor;

namespace Beholder.Editor
{
	/// <summary>
	/// Responsible for drawing the Scene GUI for BeholdR
	/// </summary>
	[InitializeOnLoad]
	public static class SceneGuiDrawer
	{
		#region Constructors
		static SceneGuiDrawer()
		{
			// subscribe to scene GUI events
			SceneView.onSceneGUIDelegate += Draw;
		}
		#endregion

		#region Data Members
		private static Rect _controlsGuiRect;

		#region GUI definitions
		private const float GUI_AREA_MARGIN = 10f;
		private const float HEIGHT = 45f;
		private const float WIDTH = 64f;
		private const float UNITY_GIZMO_WIDTH = 80f;
		private const float CAMERA_PREVIEW_NORMALIZED = 0.2f;
		private const float CAMERA_PREVIEW_OFFSET = 10f;
		#endregion
		#endregion

		#region Public Methods
		/// <summary>
		/// Draws the in-scene GUI controls for the currently active BeholdR component
		/// </summary>
		/// <param name="sceneView">The currently drawing scene view</param>
		public static void Draw(SceneView sceneView)
		{
			if(	BeholdR.ActiveInstance == null ||
				BeholdR.ActiveInstance.IsSyncSupressed || 
				!BeholdR.ActiveInstance.ShowGuiControls)
			{
				return;
			}

			// calculate drawing area
			_controlsGuiRect = CalculateGuiRect(BeholdR.ActiveInstance.ControlsAnchor, sceneView.camera, Utilities.IsEditorVisible("UnityEditor.CameraEditor"));

			// do the actual drawing
			Handles.BeginGUI();
			{
				GUILayout.BeginArea(_controlsGuiRect, GUI.skin.box);
				{
					DrawViewLinkControl(sceneView);
					DrawFilterControl(sceneView);
				}
				GUILayout.EndArea();

				//EditorGUILayout.Toggle("HDR?", sceneView.camera.hdr);
				//DrawCameraPostFx(sceneView.camera);
			}
			Handles.EndGUI();
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// calculate the area in which we draw the GUI, considering the current anchor option
		/// </summary>
		/// <param name="anchor">In what area of the scene view we should draw</param>
		/// <param name="sceneCamera">The camera of the scene view in which we render the GUI</param>
		/// <param name="isPreviewOpen">Whether or not there's an open Camera Preview window on the scene view</param>
		/// <returns>the GUI area in the appropriate anchor position</returns>
		private static Rect CalculateGuiRect(TextAnchor anchor, Camera sceneCamera, bool isPreviewOpen)
		{
			switch(anchor)
			{
				case TextAnchor.UpperLeft:
					return new Rect(GUI_AREA_MARGIN, GUI_AREA_MARGIN, WIDTH, HEIGHT);
				case TextAnchor.UpperCenter:
					return new Rect((sceneCamera.pixelWidth / 2f) - (WIDTH / 2f), GUI_AREA_MARGIN, WIDTH, HEIGHT);
				case TextAnchor.UpperRight:
					return new Rect((sceneCamera.pixelWidth - WIDTH - GUI_AREA_MARGIN) - UNITY_GIZMO_WIDTH, GUI_AREA_MARGIN, WIDTH, HEIGHT);
				case TextAnchor.MiddleLeft:
					return new Rect(GUI_AREA_MARGIN, (sceneCamera.pixelHeight / 2f) - (HEIGHT / 2f), WIDTH, HEIGHT);
				case TextAnchor.MiddleCenter:
					return new Rect((sceneCamera.pixelWidth / 2f) - (WIDTH / 2f), (sceneCamera.pixelHeight / 2f) - (HEIGHT / 2f), WIDTH, HEIGHT);
				case TextAnchor.MiddleRight:
					return new Rect((sceneCamera.pixelWidth - WIDTH - GUI_AREA_MARGIN), (sceneCamera.pixelHeight / 2f) - (HEIGHT / 2f), WIDTH, HEIGHT);
				case TextAnchor.LowerLeft:
					return new Rect(GUI_AREA_MARGIN, (sceneCamera.pixelHeight - HEIGHT - GUI_AREA_MARGIN), WIDTH, HEIGHT);
				case TextAnchor.LowerCenter:
					return new Rect((sceneCamera.pixelWidth / 2f) - (WIDTH / 2f), (sceneCamera.pixelHeight - HEIGHT - GUI_AREA_MARGIN), WIDTH, HEIGHT);
				case TextAnchor.LowerRight:
					float offset = 0;
					if(isPreviewOpen)
					{
						offset = (sceneCamera.pixelWidth * CAMERA_PREVIEW_NORMALIZED) + CAMERA_PREVIEW_OFFSET + (GUI_AREA_MARGIN * 2);
					}
					return new Rect((sceneCamera.pixelWidth - WIDTH - GUI_AREA_MARGIN - offset), (sceneCamera.pixelHeight - HEIGHT - GUI_AREA_MARGIN), WIDTH, HEIGHT);
				default:
					goto case TextAnchor.UpperLeft;
			}
		}

		/// <summary>
		/// Draw a button to link/unlink the game camera with the given scene view
		/// </summary>
		/// <param name="sceneView">The currently drawing scene view</param>
		private static void DrawViewLinkControl(SceneView sceneView)
		{
			if(BeholdR.ActiveInstance.LinkedSceneView == sceneView)
			{
				Color bkp = GUI.color;
				GUI.color = Color.red;

				if(GUILayout.Button("Unlink"))
				{
					BeholdR.ActiveInstance.LinkedSceneView = null;
					SceneView.RepaintAll();
				}

				GUI.color = bkp;
			}
			else
			{
				if(GUILayout.Button("Link"))
				{
					BeholdR.ActiveInstance.LinkedSceneView = sceneView;
					SceneView.RepaintAll();
				}
			}
		}

		/// <summary>
		/// Draw a button to add/remove the drawing scene view from the filtered view set of BeholdR
		/// </summary>
		/// <param name="sceneView">The currently drawing scene view</param>
		private static void DrawFilterControl(SceneView sceneView)
		{
			if(BeholdR.ActiveInstance.HasFilter(sceneView.camera))
			{
				Color bkp = GUI.color;
				GUI.color = Color.red;

				if(GUILayout.Button("Include"))
				{
					BeholdR.ActiveInstance.RemoveFilter(sceneView.camera);
				}

				GUI.color = bkp;
			}
			else
			{
				if(GUILayout.Button("Exclude"))
				{
					BeholdR.ActiveInstance.AddFilter(sceneView.camera);
				}
			}
		}

		/// <summary>
		/// Draws a list of all the components that are on the SceneView's camera for debugging purposes
		/// </summary>
		/// <param name="sceneCamera">The camera we're drawing the list for</param>
		private static void DrawCameraPostFx(Camera sceneCamera)
		{
			EditorGUILayout.BeginVertical("box");
			{
				BeholdR.ActiveInstance.ShowCameraComponents = EditorGUILayout.Foldout(BeholdR.ActiveInstance.ShowCameraComponents, "Camera Components");
				if(BeholdR.ActiveInstance.ShowCameraComponents)
				{
					EditorGUI.indentLevel += 2;

					foreach(Component component in sceneCamera.GetComponents<Component>())
					{
						EditorGUILayout.LabelField(component.GetType().Name);
					}

					EditorGUI.indentLevel -= 2;
				}
			}
			EditorGUILayout.EndVertical();
		}
		#endregion
	}
}
