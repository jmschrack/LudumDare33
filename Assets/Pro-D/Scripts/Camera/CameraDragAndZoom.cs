//This is the script that's currently being used for the Webplayer version online.
//We included this so users who fancy an in-game minimap may benefit from this example.

/* 
* This code has been designed and developed by Gray Lake Studios.
* You may only use this code if you’ve acquired the appropriate license.
* To acquire such licenses you may visit www.graylakestudios.com and/or Unity Asset Store
* For all inquiries you may contact contact@graylakestudios.com
* Copyright © 2012 Gray Lake Studios
*/

using UnityEngine;

namespace ProD
{
	public class CameraDragAndZoom : MonoBehaviour //a.k.a MiniMap.cs
	{
		public float dragSpeed = -1f;
		public float zoomSpeed = 1f;

		private Camera _Camera;
		private Transform _CameraTransform;

		protected float _ScrollAxis;
		protected Vector2 _mouseAxis;
		protected bool _mouseButtonHold;

		void OnEnable()
		{
			_Camera = gameObject.GetComponent<Camera>();
			_CameraTransform = _Camera.transform;
			InputManager.Instance.cameraDragAndZoom = this;
		}

		public void SetInput(float scroll, Vector2 mouseAxis, bool mouseButtonHold)
		{
			_ScrollAxis = scroll;
			_mouseAxis = mouseAxis;
			_mouseButtonHold = mouseButtonHold;
		}


		private void Update()
		{
			//Zoom in and out with scrollwheel
			if (_ScrollAxis < 0) //Backwardsscroll.
			{
				_Camera.orthographicSize = _Camera.orthographicSize + (1 * zoomSpeed);
			}
			else if (_ScrollAxis > 0) //Forwardscroll.
			{
				_Camera.orthographicSize = _Camera.orthographicSize - (1 * zoomSpeed);
			}

			//Click and drag the map
			if (_mouseButtonHold)
			{
				Vector3 cameraMovement;

				if (ProDManager.Instance.topDown)
				{
					cameraMovement = new Vector3(_mouseAxis.x * dragSpeed * _Camera.orthographicSize, 0, _mouseAxis.y * dragSpeed * _Camera.orthographicSize);
				}
				else
				{
					cameraMovement = new Vector3(_mouseAxis.x * dragSpeed * _Camera.orthographicSize, _mouseAxis.y * dragSpeed * _Camera.orthographicSize, 0);
				}
				_CameraTransform.position += cameraMovement;
			}
		}
	}
}