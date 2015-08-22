/* 
* This code has been designed and developed by Gray Lake Studios.
* You may only use this code if you’ve acquired the appropriate license.
* To acquire such licenses you may visit www.graylakestudios.com and/or Unity Asset Store
* For all inquiries you may contact contact@graylakestudios.com
* Copyright © 2012 Gray Lake Studios
*/

using UnityEngine;
using System.Collections;

namespace ProD
{
	public class CameraObjectTracker : MonoBehaviour
	{
		public float additionalSize = 0.0f;

		private Transform Target;

		private float sizeToDisplay;

		private Rect oldScreen;

		protected virtual void OnEnable()
		{
			oldScreen = new Rect(0, 0, Screen.width, Screen.height);

			sizeToDisplay = GetComponent<Camera>().orthographicSize;
		}

		/// <summary>
		/// Sets the target for this camera to keep track of
		/// </summary>
		/// <param name='target'>
		/// The target.
		/// </param>
		public void SetTarget(Transform target)
		{
			this.Target = target;
			if (Target != null)
				SetCameraToPosition(target.position);

		}

		/// <summary>
		/// Informs the Object tracker that the position has changed
		/// </summary>
		public void UpdatePosition()
		{
			if (Target != null)
				SetCameraToPosition(Target.position);
		}

		protected virtual void SetCameraToPosition(Vector3 newPosition)
		{
			if (ProDManager.Instance.topDown)
			{
				newPosition = new Vector3(newPosition.x, transform.position.y, newPosition.z);
			}
			else
			{
				newPosition = new Vector3(newPosition.x, newPosition.y, transform.position.z);
			}
			if (enabled)
				transform.position = newPosition;
		}

		/// <summary>
		/// Sets the size of the view area. 
		/// </summary>
		/// <param name='min'>
		/// The camera will always show atleast an area of min length.
		/// </param>
		public void SetMinimumDisplay(float min)
		{
			sizeToDisplay = min;
			UpdateMinimumDisplay();

		}

		protected void UpdateMinimumDisplay()
		{
			if (Target == null) return;

			if (enabled)
				if (Screen.width >= Screen.height)
				{
					GetComponent<Camera>().orthographicSize = sizeToDisplay + additionalSize;
				}
				else
				{
					float aspect = (float)Screen.width / (float)Screen.height;
					GetComponent<Camera>().orthographicSize = (sizeToDisplay + additionalSize) / aspect;
				}
		}


		protected virtual void Update()
		{
			Rect newScreen = new Rect(0, 0, Screen.width, Screen.height);
			if (oldScreen.Equals(newScreen) == false)
			{
				UpdateMinimumDisplay();
				oldScreen = newScreen;
			}
		}

	}
}
