using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Beholder
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class BeholdR : MonoBehaviour
	{
#if UNITY_EDITOR
		#region Data Members
		public static BeholdR ActiveInstance;

		#region GUI controls
		public TextAnchor ControlsAnchor = TextAnchor.UpperLeft;
		public bool ShowGuiControls = true;
		public bool ShowCameraComponents;
		#endregion

		#region Scene View Linking
		[SerializeField]
		private Camera _camera;

		public SceneView LinkedSceneView;
		public bool MatchCameraColor;
		public bool MatchClippingPlanes;

		private static object _sceneColorField;
		private static PropertyInfo _sceneColorProperty;
		private static PropertyInfo SceneColorProperty
		{
			get
			{
				if(_sceneColorProperty == null)
				{
					Type svType = Utilities.FindTypeByName("UnityEditor.SceneView");
					FieldInfo cField = svType.GetField("kSceneViewBackground", Utilities.DEFAULT_BINDING_FLAG);
					if(cField == null) return null;

					_sceneColorField = cField.GetValue(null);
					if(_sceneColorField == null) return null;

					Type bgColorType = _sceneColorField.GetType();
					_sceneColorProperty = bgColorType.GetProperty("Color");
				}

				return _sceneColorProperty;
			}
		}

		private static Color _cachedSceneColor;
		#endregion

		#region Automatic Disable
		private const string AUTO_DISABLE_PKEY = "BeholdR_IsAutoDisableActive";
		public static bool AutoDisableInPlayMode
		{
			get { return EditorPrefs.GetBool(AUTO_DISABLE_PKEY, true); }
			set { EditorPrefs.SetBool(AUTO_DISABLE_PKEY, value); }
		}
		#endregion

		#region Automatic Discovery
		private int _lastComponentCount;

		private const string AUTO_DISCOVER_PKEY = "BeholdR_IsAutoDiscoverActive";
		public static bool AutoDiscover
		{
			get { return EditorPrefs.GetBool(AUTO_DISCOVER_PKEY, true); }
			set { EditorPrefs.SetBool(AUTO_DISCOVER_PKEY, value); }
		}

		private const string POST_FX_ATTRIBUTE_KEY = "Image Effects";
		private static readonly List<Type> PostFxBaseTypes = new List<Type>
		{
			Utilities.FindTypeByName("ImageEffectBase"),
			Utilities.FindTypeByName("PostEffectsBase")
		};
		#endregion

		#region Suppression
		public bool IsSyncSupressed;
		public const string SUPPRESSION_MESSAGE =
			"BeholdR is currently suppressed because of conflicts with Scene View filtering or Render Mode.\n" +
			"BeholdR will re-synchronize as soon as the filter is removed and Render Mode is set to Shaded.";
		#endregion

		#region Camera PostEffects synchronization related fields
		public List<Component> PostEffects = new List<Component>();

		private List<bool> _integrityList = new List<bool>();
		private Camera[] _sceneCameras = { };
		public bool OrderNeeded = true;
		public bool SkipFrame = false;
		#endregion

		private static readonly HashSet<Camera> FilteredSceneCameras = new HashSet<Camera>();
		#endregion

		#region MonoBehaviour events
		/// <summary>
		/// Initialization
		/// </summary>
		void OnEnable()
		{
			_camera = GetComponent<Camera>();

			EditorApplication.RepaintHierarchyWindow();

			EnsureSingle();

			if(AutoDiscover) TryAutoCollectPostFx();

			// Subscribe to Editor events
			EditorApplication.update += UpdateBeholdR;

			if(!MatchCameraColor) CacheSceneColor();

			ActiveInstance = this;
		}

		/// <summary>
		/// Clear the effects of this instance. Called also just before OnDestroy
		/// </summary>
		void OnDisable()
		{
			// Unsubscribe from Editor events
			EditorApplication.update -= UpdateBeholdR;

			if(ActiveInstance == this)
			{
				ActiveInstance = null;
			}

			CleanAllSceneCameras();
			ResetSceneColor();
		}

		/// <summary>
		/// Perform cleanup
		/// </summary>
		void OnDestroy()
		{
			PostEffects = new List<Component>();
			UpdateBeholdR();
			CleanAllSceneCameras();
		}
		#endregion

		#region Unity Editor Event Hooks
		/// <summary>
		/// Custom Update method, called on every Editor.Update
		/// </summary>
		public void UpdateBeholdR()
		{
			if(!enabled) return;
			if(EditorApplication.isPlaying && AutoDisableInPlayMode) return;

			IsSyncSupressed = TestSuppressionNeeded();
			if(IsSyncSupressed)
			{
				CleanAllSceneCameras();
				return;
			}

			GetSceneCameras();
			if(_sceneCameras.Length == 0) return;

			if(AutoDiscover) AttemptAutoDiscover();
			if(IsPostEffectsListDirty()) EnsureCamerasClean();

			SyncComponents();

			if(LinkedSceneView != null) SyncCameraTransform();
			if(MatchCameraColor) SetSceneColor(_camera.backgroundColor);

			if(OrderNeeded)
			{
				if(SkipFrame)
				{
					SkipFrame = false;
					return;
				}

				OrderNeeded = false;
				ReorderActualEffects();
			}
		}
		#endregion

		#region Private Methods
		#region Setup
		/// <summary>
		/// Makes sure that only one <see cref="BeholdR"/> component is active at any point in time to avoid conflicts.
		/// </summary>
		private void EnsureSingle()
		{
			BeholdR[] allBeholders = FindObjectsOfType<BeholdR>();
			foreach(BeholdR beholdR in allBeholders)
			{
				if(beholdR != this)
				{
					beholdR.enabled = false;
				}
			}
		}

		/// <summary>
		/// Resets the scene camera array to include only currently live cameras
		/// </summary>
		private void GetSceneCameras()
		{
			_sceneCameras = InternalEditorUtility.GetSceneViewCameras();
		}
		#endregion

		#region Automatic Discovery
		/// <summary>
		/// If there is a difference between the current and last component count on the camera, attempt to auto discover the new components
		/// </summary>
		private void AttemptAutoDiscover()
		{
			int currentCount = GetComponents<Component>().Length;
			if(currentCount != _lastComponentCount)
			{
				TryAutoCollectPostFx();
				OrderNeeded = true;
			}

			_lastComponentCount = currentCount;
		}

		/// <summary>
		/// Look at the component's AddComponentMenu attribute for "Image Effects", which is standard for Unity's built-in postFX and many others
		/// </summary>
		/// <param name="postFxCandidate">the component we're currently checking in the <see cref="TryAutoCollectPostFx"/> loop</param>
		/// <returns>True if the component was found to be an Image Effect and was added to the list, false otherwise</returns>
		private bool TryWithAttribute(Component postFxCandidate)
		{
			// find the AddComponent attribute
			object[] attributes = postFxCandidate.GetType().GetCustomAttributes(typeof(AddComponentMenu), false);
			if(attributes.Length == 0) return false;

			// extract information to see if it contains the Image Effects menu
			foreach(AddComponentMenu attr in attributes)
			{
				if(attr.componentMenu.Split('/')[0].Contains(POST_FX_ATTRIBUTE_KEY) && !PostEffects.Contains(postFxCandidate))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Look at the component's type to see if it inherits from <see cref="PostEffectsBase"/> of <see cref="ImageEffectBase"/>
		/// </summary>
		/// <param name="postFxCandidate">the component we're currently checking in the <see cref="TryAutoCollectPostFx"/> loop</param>
		/// <returns></returns>
		private static bool TryWithType(Component postFxCandidate)
		{
			// compare against base types
			foreach(Type postFxBaseType in PostFxBaseTypes)
			{
				if(postFxBaseType == null) continue;

				if(postFxCandidate.GetType().IsSubclassOf(postFxBaseType))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Look at the component for the "OnRenderImage" method that is used by image effects
		/// </summary>
		/// <param name="postFxCandidate"></param>
		/// <returns></returns>
		private static bool TryWithMethod(Component postFxCandidate)
		{
			MethodInfo m = postFxCandidate.GetType().GetMethod("OnRenderImage", Utilities.DEFAULT_BINDING_FLAG);
			return m != null;
		}
		#endregion

		#region Post Effects List Management
		/// <returns>True if the list is different since the last time we checked</returns>
		private bool IsPostEffectsListDirty()
		{
			bool dirty = false;

			if(_integrityList.Count != PostEffects.Count)
			{
				RebuildIntegrityList();
				return true;
			}

			for(int index = 0; index < PostEffects.Count; index++)
			{
				if(PostEffects[index] == null)
				{
					if(_integrityList[index])
					{
						_integrityList[index] = false;
						dirty = true;
					}
				}
				else
				{
					if(!_integrityList[index])
					{
						_integrityList[index] = true;
						dirty = true;
					}
				}
			}

			return dirty;
		}

		private void RebuildIntegrityList()
		{
			_integrityList = new List<bool>(PostEffects.Count);

			foreach(Component postEffect in PostEffects)
			{
				_integrityList.Add(postEffect != null);
			}
		}

		/// <summary>
		/// Iterates on current object's components and adds them by order to the post effects list if they were there already
		/// </summary>
		private void SortPostEffects()
		{
			List<Component> oldList = new List<Component>(PostEffects);
			PostEffects.Clear();

			foreach(Component component in gameObject.GetComponents<Component>())
			{
				if(oldList.Contains(component))
				{
					PostEffects.Add(component);
					oldList.Remove(component);
				}
			}
		}

		/// <summary>
		/// Reorder the effect components on the GameObject itself to match that of the PostEffects list
		/// </summary>
		private void ReorderActualEffects()
		{
			MoveComponentToLastPosition("Tonemapping");

			foreach(Component postEffect in PostEffects)
			{
				Component nEffect = gameObject.AddComponent(postEffect.GetType());
				EditorUtility.CopySerialized(postEffect, nEffect);
				DestroyImmediate(postEffect);
			}

			TryAutoCollectPostFx();

			// force scene views to update to reflect changes
			UpdateBeholdR();
			SceneView.RepaintAll();
		}

		/// <summary>
		/// Moves a component with the same name as <see cref="componentName"/> to the last position in the PostEffects list
		/// </summary>
		/// <param name="componentName">The name of the component we want to move, to be compared against <see cref="Type.Name"/></param>
		private void MoveComponentToLastPosition(string componentName)
		{
			Component comp = null;

			foreach (Component postEffect in PostEffects)
			{
				if(postEffect.GetType().Name == componentName)
				{
					comp = postEffect;
					break;
				}
			}

			if(comp != null)
			{
				PostEffects.Remove(comp);
				PostEffects.Add(comp);
			}
		}

		#endregion

		#region Synchronization
		/// <summary>
		/// Make sure the all scene cameras has all the components in the PostEffect list and that all their values are copied correctly
		/// </summary>
		private void SyncComponents()
		{
			// don't bother if there are no components to sync
			if(!PostEffects.AnyConcrete()) return;

			// sync to all cameras
			foreach(Camera sceneCamera in _sceneCameras)
			{
				if(sceneCamera == null || HasFilter(sceneCamera)) continue;

				GameObject sceneCameraGo = sceneCamera.gameObject;

				// ensure the camera has all the post effects
				foreach(Component postEffect in PostEffects)
				{
					if(postEffect == null) continue;

					Type effectType = postEffect.GetType();

					// HACK: specifically skip Tonemapping because it conflicts with the editor in Unity 5
					if(postEffect.GetType().Name == "Tonemapping") continue;

					// get the component, adding it if missing
					Component cameraComponent = sceneCameraGo.GetComponent(effectType) ?? sceneCameraGo.AddComponent(effectType);

					// skip components we don't want to mess with
					if(Utilities.IsForbiddenComponent(sceneCamera, cameraComponent)) continue;

					// this does the actual update by copying the values from the game camera to the scene camera
					EditorUtility.CopySerialized(postEffect, cameraComponent);

					// some post processes need an extra step to ensure they update correctly
					EnsurePostProcessUpdates(cameraComponent);
				}
			}
		}

		/// <summary>
		/// Matches our camera's position and rotation with the selected scene view camera
		/// </summary>
		void SyncCameraTransform()
		{
			MatchMeToTarget(LinkedSceneView.camera.transform);
		}

		/// <summary>
		/// Makes our transform match the target
		/// </summary>
		private void MatchMeToTarget(Transform target)
		{
			transform.position = target.position;
			transform.rotation = target.rotation;
		}

		/// <summary>
		/// Matches the scene view background color to the camera's background color
		/// </summary>
		private static void SetSceneColor(Color color)
		{
			SceneColorProperty.SetValue(_sceneColorField, color, null);
			SceneView.RepaintAll();
		}
		#endregion

		#region Post Process Fixes
		/// <summary>
		/// If the existing post process does not update automatically, handling it should happen here
		/// </summary>
		/// <param name="targetComponent">the post process component requiring manual fix</param>
		private void EnsurePostProcessUpdates(Component targetComponent)
		{
			switch(targetComponent.GetType().Name)
			{
				case "ColorCorrectionCurves":
					FixColorCorrectionCurves(targetComponent);
					break;
				case "ColorCorrectionLut":
				case "ColorCorrectionLookup":
					FixColorCorrectionLut(targetComponent);
					break;
			}
		}

		/// <summary>
		/// specific method to fix a problem where changing the curves of a ColorCorrectionCurves post process in particular won't synchronize to the scene camera
		/// </summary>
		/// <param name="cccCandidate">the post process component we suspect is a ColorCorrectionCurves</param>
		private static void FixColorCorrectionCurves(Component cccCandidate)
		{
			Type cccType = cccCandidate.GetType();

			// force curve update
			MethodInfo updateParamsMethod = cccType.GetMethod("UpdateParameters");
			if(updateParamsMethod == null) return;
			updateParamsMethod.Invoke(cccCandidate, null);
		}

		/// <summary>
		/// specific method to fix a problem where the ColorCorrectionLut would lose the converted 3D LUT texture
		/// Attempt to fix the LUT problem with looking at the components on the scene and actual cameras
		/// </summary>
		/// <param name="cclCandidate">the post process component we suspect is a ColorCorrectionLut</param>
		private void FixColorCorrectionLut(Component cclCandidate)
		{
			// find the current LUT on scene camera
			Type cclType = cclCandidate.GetType();
			FieldInfo lutTexture = cclType.GetField("converted3DLut", Utilities.DEFAULT_BINDING_FLAG);
			if(lutTexture == null) return;

			// if all is well with the scene LUT, carry on
			object lutVal = lutTexture.GetValue(cclCandidate);
			if(!lutVal.Equals(null)) return;

			// find the current LUT on the game camera
			Component camComp = gameObject.GetComponent(cclType);
			if(camComp == null) return;

			// failed to find LUT, but there's a path to the LUT target so we can attempt conversion
			FieldInfo tempTex = cclType.GetField("basedOnTempTex", Utilities.DEFAULT_BINDING_FLAG);
			if(tempTex == null) return;

			// find path to LUT target
			string tempVal = tempTex.GetValue(camComp) as string;
			if(string.IsNullOrEmpty(tempVal)) return;

			// re-import texture for LUT compatible format
			TextureImporter texImport = AssetImporter.GetAtPath(tempVal) as TextureImporter;
			if(texImport == null) return;

			bool doImport = texImport.isReadable == false ||
							texImport.mipmapEnabled ||
							texImport.textureFormat != TextureImporterFormat.AutomaticTruecolor;

			if(doImport)
			{
				texImport.isReadable = true;
				texImport.mipmapEnabled = false;
				texImport.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				AssetDatabase.ImportAsset(tempVal, ImportAssetOptions.ForceUpdate);
			}

			// convert to LUT
			MethodInfo convertMethod = cclType.GetMethod("Convert", Utilities.DEFAULT_BINDING_FLAG);
			if(convertMethod == null) return;

			Texture2D tex = AssetDatabase.LoadMainAssetAtPath(tempVal) as Texture2D;
			if(tex == null) return;

			convertMethod.Invoke(cclCandidate, new object[] { tex, tempVal });
		}
		#endregion

		#region Suppression
		/// <summary>
		/// Tests to see if we need to suppress BeholdR in order to prevent crash
		/// </summary>
		private static bool TestSuppressionNeeded()
		{
			foreach(SceneView sceneView in SceneView.sceneViews)
			{
				if(sceneView.renderMode != DrawCameraMode.Textured) return true;

				MethodInfo useFilteringMethod = sceneView.GetType().GetMethod("UseSceneFiltering", Utilities.DEFAULT_BINDING_FLAG);
				bool useFilteringValue = useFilteringMethod != null && (bool)useFilteringMethod.Invoke(sceneView, null);
				if(useFilteringValue) return true;
			}

			return false;
		}
		#endregion

		#region Cleanup
		/// <summary>
		/// Remove all the post processes from the scene camera
		/// </summary>
		private void CleanSceneCamera(Camera sceneCamera)
		{
			if(sceneCamera == null) return;

			CleanMissingPostEffects(sceneCamera);

			foreach(Component postEffect in PostEffects)
			{
				if(postEffect == null) continue;

				Type effectType = postEffect.GetType();
				Component cameraComponent = sceneCamera.GetComponent(effectType);

				if(Utilities.IsForbiddenComponent(sceneCamera, cameraComponent)) continue;

				DestroyImmediate(cameraComponent);
			}
		}

		/// <summary>
		/// In case we have a difference between the scene camera and our list, re-sync to list
		/// </summary>
		private static void CleanMissingPostEffects(Camera sceneCamera)
		{
			if(sceneCamera == null) return;

			List<Component> wipeList = new List<Component>(sceneCamera.GetComponents<Component>());

			foreach(Component component in wipeList)
			{
				if(Utilities.IsForbiddenComponent(sceneCamera, component)) continue;

				DestroyImmediate(component);
			}
		}

		/// <summary>
		/// Make sure that all our cameras are clean of missing components
		/// </summary>
		private void EnsureCamerasClean()
		{
			foreach(Camera sceneCamera in _sceneCameras)
			{
				CleanMissingPostEffects(sceneCamera);
			}
		}

		#endregion
		#endregion

		#region Public Methods
		#region Scene Background Color
		/// <summary>
		/// Sets the scene view background color to the cached initial value
		/// </summary>
		public static void ResetSceneColor()
		{
			SceneColorProperty.SetValue(_sceneColorField, _cachedSceneColor, null);
			SceneView.RepaintAll();
		}

		/// <summary>
		/// Save the initial color of the scene view so that we can later reset to it
		/// </summary>
		public static void CacheSceneColor()
		{
			Type svType = Utilities.FindTypeByName("UnityEditor.SceneView");
			FieldInfo cField = svType.GetField("kSceneViewBackground", Utilities.DEFAULT_BINDING_FLAG);
			if(cField == null) return;

			object bgColor = cField.GetValue(null);
			if(bgColor == null) return;

			Type bgColorType = bgColor.GetType();
			PropertyInfo col = bgColorType.GetProperty("Color");
			if(col == null) return;

			_cachedSceneColor = (Color)col.GetValue(bgColor, null);
		}
		#endregion

		#region PostEffects List
		/// <summary>
		/// Looks for and automatically adds to the list any component that inherits from the defined base types
		/// </summary>
		public void TryAutoCollectPostFx()
		{
			foreach(Component component in GetComponents<Component>())
			{
				if(TryWithAttribute(component) ||
				   TryWithType(component) ||
				   TryWithMethod(component))
				{
					PostEffects.Add(component);
				}
			}

			ReorderPostEffectsList();
		}

		/// <summary>
		/// Remove all null post processes from the list
		/// </summary>
		public void CleanPostEffectsList()
		{
			PostEffects.RemoveAll(c => c == null);
			RebuildIntegrityList();
		}

		/// <summary>
		/// Orders the list so that it matches the order of components on the camera object
		/// Then re-sync the scene cameras
		/// </summary>
		public void ReorderPostEffectsList()
		{
			CleanPostEffectsList();
			SortPostEffects();
			RebuildIntegrityList();
			CleanAllSceneCameras();
		}
		#endregion

		#region Filters
		/// <summary>
		/// Add target scene <see cref="Camera"/> to the filtered set so it won't update with post effects
		/// </summary>
		/// <param name="sceneCamera">The camera we want to stop updating</param>
		public void AddFilter(Camera sceneCamera)
		{
			if(!FilteredSceneCameras.Contains(sceneCamera))
			{
				FilteredSceneCameras.Add(sceneCamera);
				CleanSceneCamera(sceneCamera);
			}
		}

		/// <summary>
		/// Remove target scene <see cref="Camera"/> from the filtered set so it will once again update with post effects
		/// </summary>
		/// <param name="sceneCamera">The camera we want to update again</param>
		public void RemoveFilter(Camera sceneCamera)
		{
			if(FilteredSceneCameras.Contains(sceneCamera))
			{
				FilteredSceneCameras.Remove(sceneCamera);
			}
		}

		/// <summary>
		/// Check if target scene <see cref="Camera"/> is filtered or not
		/// </summary>
		/// <param name="sceneCamera">The camera to test</param>
		/// <returns>True if part of the filtered set, False otherwise</returns>
		public bool HasFilter(Camera sceneCamera)
		{
			return FilteredSceneCameras.Contains(sceneCamera);
		}
		#endregion

		#region Cleanup
		/// <summary>
		/// clean post effects from all scene cameras
		/// </summary>
		public void CleanAllSceneCameras()
		{
			foreach(Camera sceneViewCamera in InternalEditorUtility.GetSceneViewCameras())
			{
				CleanSceneCamera(sceneViewCamera);
			}
		}
		#endregion
		#endregion
#endif
	}
}