using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Beholder.Editor
{
	/// <summary>
	/// An experimental SceneView built from the ground up for BeholdR in order to portray Post Effects more accurately
	/// </summary>
	public class ExperimentalSceneView : SceneView
	{
		private int _mainControlId;

		/// <summary>
		/// Call from Unit menu to display the view window
		/// TODO: automatic transparent conversion of existing SceneView windows to this and back
		/// </summary>
		[MenuItem("Window/BeholdR/Experimental View")]
		static void Init()
		{
			ExperimentalSceneView view = GetWindow<ExperimentalSceneView>("BView");
			view.Focus();

			if(lastActiveSceneView != null)
			{
				view.position = lastActiveSceneView.position;
				Camera oldCamera = lastActiveSceneView.camera;
				view.AlignViewToObject(oldCamera.transform);
			}
		}

		internal void OnGUI()
		{
			// variable setup
			Type sceneViewType = typeof(SceneView);
			Type handlesType = typeof(Handles);

			const BindingFlags nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;
			const BindingFlags nonPublicInstanceMethod = nonPublicInstance | BindingFlags.InvokeMethod;
			const BindingFlags publicInstanceMethod = BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod;
			const BindingFlags nonPublicStaticMethod = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod;
			const BindingFlags publicStaticMethod = BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod;
			const BindingFlags getNonPublicStaticField = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField;
			const BindingFlags getNonPublicInstanceField = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;
			const BindingFlags setNonPublicInstanceField = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField;

			// re-implement the SceneView 
			Event current = Event.current;
			if(current.type == EventType.Repaint)
			{
				var mouseRects = sceneViewType.InvokeMember("s_MouseRects", getNonPublicStaticField, null, null, null);
				mouseRects.GetType().InvokeMember("Clear", publicInstanceMethod, null, mouseRects, null);

				Profiler.BeginSample("SceneView.Repaint");
			}

			Color color = GUI.color;

			sceneViewType.InvokeMember("HandleClickAndDragToFocus", nonPublicInstanceMethod, null, this, null);

			if(current.type == EventType.Layout)
			{
				sceneViewType.InvokeMember("m_ShowSceneViewWindows", setNonPublicInstanceField, null, this, new object[] { lastActiveSceneView == this });
			}

			var overlay = sceneViewType.InvokeMember("m_SceneViewOverlay", getNonPublicInstanceField, null, this, null);
			overlay.GetType().InvokeMember("Begin", publicInstanceMethod, null, overlay, null);

			object[] args = { false, 0.0f };
			sceneViewType.InvokeMember("SetupFogAndShadowDistance", nonPublicInstanceMethod, null, this, args);
			bool oldFog = (bool)args[0];
			float oldShadowDistance = (float)args[1];

			sceneViewType.InvokeMember("DoToolbarGUI", nonPublicInstanceMethod, null, this, null);

			GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
			EditorGUIUtility.labelWidth = 100f;

			sceneViewType.InvokeMember("SetupCamera", nonPublicInstanceMethod, null, this, null);

			// ------------------ camera setup comes here overriding the changes Unity made  ------------------ 
			if(BeholdR.ActiveInstance != null)
			{
				Camera beholdRCamera = BeholdR.ActiveInstance.GetComponent<Camera>();
				if(!beholdRCamera.orthographic)
				{
					camera.fieldOfView = beholdRCamera.fieldOfView;
				}

				if(BeholdR.ActiveInstance.MatchClippingPlanes)
				{
					camera.nearClipPlane = beholdRCamera.nearClipPlane;
					camera.farClipPlane = beholdRCamera.farClipPlane;
				}

				camera.renderingPath = beholdRCamera.renderingPath;
				camera.depthTextureMode = beholdRCamera.depthTextureMode;
			}
			// ------------------ return execution  ------------------ 

			RenderingPath renderingPath = camera.renderingPath;

			sceneViewType.InvokeMember("SetupCustomSceneLighting", nonPublicInstanceMethod, null, this, null);

			GUI.BeginGroup(new Rect(0.0f, kToolbarHeight, position.width, position.height - kToolbarHeight));
			{
				Rect rect = new Rect(0.0f, 0.0f, position.width, position.height - kToolbarHeight);

				sceneViewType.InvokeMember("HandleViewToolCursor", nonPublicInstanceMethod, null, this, null);
				sceneViewType.InvokeMember("PrepareCameraTargetTexture", nonPublicInstanceMethod, null, this, new object[] { rect });
				sceneViewType.InvokeMember("DoClearCamera", nonPublicInstanceMethod, null, this, new object[] { rect });

				camera.cullingMask = Tools.visibleLayers;

				sceneViewType.InvokeMember("DoOnPreSceneGUICallbacks", nonPublicInstanceMethod, null, this, new object[] { rect });
				sceneViewType.InvokeMember("PrepareCameraReplacementShader", nonPublicInstanceMethod, null, this, null);
				sceneViewType.InvokeMember("m_MainViewControlID", setNonPublicInstanceField, null, this, new object[] { 1 });

				_mainControlId = GUIUtility.GetControlID(FocusType.Keyboard);
				if(current.GetTypeForControl(_mainControlId) == EventType.MouseDown)
				{
					GUIUtility.keyboardControl = _mainControlId;
				}

				args = new object[]{ rect, false };
				sceneViewType.InvokeMember("DoDrawCamera", nonPublicInstanceMethod, null, this, args);
				bool pushedGUIClip = (bool)args[1];

				sceneViewType.InvokeMember("CleanupCustomSceneLighting", nonPublicInstanceMethod, null, this, null);

				bool useSceneFiltering = (bool)sceneViewType.InvokeMember("UseSceneFiltering", nonPublicInstanceMethod, null, this, null);
				if(!useSceneFiltering)
				{
					handlesType.InvokeMember("DrawCameraStep2", nonPublicStaticMethod, null, null, new object[] { camera, renderMode });
					sceneViewType.InvokeMember("DoTonemapping", nonPublicInstanceMethod, null, this, null);
					sceneViewType.InvokeMember("HandleSelectionAndOnSceneGUI", nonPublicInstanceMethod, null, this, null);
				}

				typeof(EditorUtility).InvokeMember("SetTemporarilyAllowIndieRenderTexture", nonPublicStaticMethod, null, null, new object[] { false });

				if(current.type == EventType.ExecuteCommand || current.type == EventType.ValidateCommand)
				{
					sceneViewType.InvokeMember("CommandsGUI", nonPublicInstanceMethod, null, this, null);
				}

				camera.renderingPath = renderingPath;

				if(useSceneFiltering)
				{
					sceneViewType.InvokeMember("RestoreFogAndShadowDistance", nonPublicStaticMethod, null, this, new object[] { oldFog, oldShadowDistance });
					handlesType.InvokeMember("SetCameraFilterMode", nonPublicStaticMethod, null, null, new object[] { Camera.current, 1 });
				}
				else
				{
					handlesType.InvokeMember("SetCameraFilterMode", nonPublicStaticMethod, null, null, new object[] { Camera.current, 0 });
				}
				
				sceneViewType.InvokeMember("DefaultHandles", nonPublicInstanceMethod, null, this, null);

				if(!useSceneFiltering)
				{
					if(current.type == EventType.Repaint)
					{
						Profiler.BeginSample("SceneView.BlitRT");
						Graphics.SetRenderTarget(null);
					}

					if(pushedGUIClip)
					{
						Utilities.FindTypeByName("UnityEngine.GUIClip").InvokeMember("Pop", nonPublicStaticMethod, null, null, null);
					}

					if(current.type == EventType.Repaint)
					{
						GL.sRGBWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear);

						Texture sceneTargetTexture = (Texture)sceneViewType.InvokeMember("m_SceneTargetTexture", getNonPublicInstanceField, null, this, null);
						Texture sceneTargetTextureLdr = (Texture)sceneViewType.InvokeMember("m_SceneTargetTextureLDR", getNonPublicInstanceField, null, this, null);

						GUI.DrawTexture(rect, !camera.hdr ? sceneTargetTexture : sceneTargetTextureLdr, ScaleMode.StretchToFill, false);

						GL.sRGBWrite = false;
						Profiler.EndSample();
					}
				}

				handlesType.InvokeMember("SetCameraFilterMode", nonPublicStaticMethod, null, null, new object[] { Camera.current, 0 });
				handlesType.InvokeMember("SetCameraFilterMode", nonPublicStaticMethod, null, null, new object[] { camera, 0 });
				sceneViewType.InvokeMember("HandleDragging", nonPublicInstanceMethod, null, this, null);

				var svRot = sceneViewType.InvokeMember("svRot", getNonPublicInstanceField, null, this, null);
				svRot.GetType().InvokeMember("HandleContextClick", nonPublicInstanceMethod, null, svRot, new object[] { this });
				svRot.GetType().InvokeMember("OnGUI", nonPublicInstanceMethod, null, svRot, new object[] { this });

				if(lastActiveSceneView == this)
				{
					Type sceneViewMotionType = Utilities.FindTypeByName("UnityEditor.SceneViewMotion");
					sceneViewMotionType.InvokeMember("ArrowKeys", publicStaticMethod, null, null, new object[] { this });
					sceneViewMotionType.InvokeMember("DoViewTool", publicStaticMethod, null, null, new object[] { camera.transform, this });
				}

				sceneViewType.InvokeMember("Handle2DModeSwitch", nonPublicInstanceMethod, null, this, null);
			}
			GUI.EndGroup();
			GUI.color = color;

			overlay.GetType().InvokeMember("End", publicInstanceMethod, null, overlay, null);

			sceneViewType.InvokeMember("HandleMouseCursor", nonPublicInstanceMethod, null, this, null);

			if(current.type != EventType.Repaint)
			{
				return;
			}

			Profiler.EndSample();

			//Debug.Log(Selection.activeObject); - UnityEditor.DockArea
		}
	}
}
