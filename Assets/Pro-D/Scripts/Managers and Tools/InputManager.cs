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
	public class InputManager : Singleton<InputManager>
	{

		public bool allowCrossMovement = false;

		public bool allowDragClicks = false;

		public TurnBasedPlayerMovement turnBasedMovement { get; set; }
		public CameraDragAndZoom cameraDragAndZoom { get; set; }
		public PathFinding pathFinding { get; set; }

		private Vector2 downMousePos;

		private bool inputDisabled = false;
		// Update is called once per frame
		void Update()
		{
			if (turnBasedMovement != null)
			{
				Vector2 input = new Vector2();

				input.x = Input.GetAxis("Horizontal");
				input.y = Input.GetAxis("Vertical");

				if (allowCrossMovement == false && input.x != 0.0f && input.y != 0.0f)
					input = Vector2.zero;

				if (inputDisabled == false)
				{
					turnBasedMovement.SetInput(input);
				}

				inputDisabled = input != Vector2.zero;
			}

			if (cameraDragAndZoom != null)
			{
				cameraDragAndZoom.SetInput(Input.GetAxis("Mouse ScrollWheel"), new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")), Input.GetMouseButton(0) && GUIUtility.hotControl == 0);
			}

			if (pathFinding != null)
			{
				if (Input.GetMouseButtonDown(0) && GUIUtility.hotControl == 0)
				{
					downMousePos = Input.mousePosition;
				}

				//mousbutton is released and was not on gui and either dragging is allowed or mouse position stayed the same
				bool clickFinished = Input.GetMouseButtonUp(0) && GUIUtility.hotControl == 0 && (allowDragClicks || ((Vector2)Input.mousePosition == downMousePos));

				pathFinding.SetInput(Input.mousePosition, clickFinished);
			}
		}

	}

}
