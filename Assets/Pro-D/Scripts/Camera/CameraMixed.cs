using UnityEngine;
using System.Collections;


namespace ProD
{
	public class CameraMixed : CameraObjectTracker
	{
		private bool IsFreeMode = false;
		private Vector3 _OldPosition;

		protected override void OnEnable()
		{
			base.OnEnable();
			_OldPosition = transform.position;
		}

		protected override void SetCameraToPosition(Vector3 newPosition)
		{
			if (IsFreeMode) return;
			base.SetCameraToPosition(newPosition);

			_OldPosition = transform.position;
		}

		protected override void Update()
		{
			base.Update();

			if (Input.GetKeyDown(KeyCode.Space))
			{
				FocusPlayer();
			}

			if (!IsFreeMode && transform.position != _OldPosition)
			{
				IsFreeMode = true;
				InputManager.Instance.allowDragClicks = false;
			}
		}

		public void FocusPlayer()
		{
			IsFreeMode = false;
			InputManager.Instance.allowDragClicks = true;

			UpdatePosition();
			//UpdateMinimumDisplay();
		}
	}

}