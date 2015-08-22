/* 
* This code has been designed and developed by Gray Lake Studios.
* You may only use this code if you’ve acquired the appropriate license.
* To acquire such licenses you may visit www.graylakestudios.com and/or Unity Asset Store
* For all inquiries you may contact contact@graylakestudios.com
* Copyright © 2012 Gray Lake Studios
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProD
{
	public class PathFinding : MonoBehaviour
	{
		public int targetX;
		public int targetY;

		#region visual

		public float layer = 0.75f;

		public FilterMode filterMode = FilterMode.Bilinear;

		public Color emptyColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		public Color pathColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);

		private Map map;

		private GameObject pathPlane;
		private Texture2D pathTexture;
		#endregion

		#region pathfinding
		public List<string> walkableTypes;

		private PathfindingAlgorithm algorithm;

		private Stack<Cell> path;

		Address lastPlayerPosition;

		#endregion

		#region mouseControl
		public FogOfWar fogOfWar;

		public LimitedTo limitedTo = LimitedTo.VisibleOnly;

		public enum LimitedTo
		{
			All, VisitedOnly, VisibleOnly
		}

		public float walkSpeed;
		private float walkTime;

		private Vector2 mousePos;
		private bool hasClicked = false;

		private bool isWalking = false;

		public TurnBasedPlayerMovement turnBasedMovement { get; set; }
		#endregion

		public void InitPathfinding(Map map_)
		{
			if (pathPlane != null) Destroy(pathPlane);

			map = map_;

			string directory = "Pathfinding/PRE_PathfindingPlane";
			GameObject tempPlane = Resources.Load(directory) as GameObject;
			pathPlane = (GameObject)Instantiate(tempPlane);

			pathTexture = new Texture2D(map.size_X, map.size_Y);

			for (int i = 0; i < map.size_X; i++)
				for (int j = 0; j < map.size_Y; j++)
					pathTexture.SetPixel(i, j, emptyColor);
			pathTexture.Apply();
			pathTexture.filterMode = filterMode;


			if (ProDManager.Instance.topDown)
			{
				pathPlane.transform.localScale = new Vector3(map.size_X, layer, map.size_Y);
				pathPlane.transform.position = new Vector3(map.size_X / 2, layer, map.size_Y / 2);
			}
			else
			{
				pathPlane.transform.Rotate(-90f, 0.0f, 0.0f, Space.World);
				pathPlane.transform.localScale = new Vector3(map.size_X, layer, map.size_Y);
				pathPlane.transform.position = new Vector3(map.size_X / 2, map.size_Y / 2, -layer);
			}
			pathPlane.GetComponent<Renderer>().material.SetTexture("_MainTex", pathTexture);

			pathPlane.GetComponent<Renderer>().material.color = Color.white;

			algorithm = new PathfindingAlgorithm(map);

			algorithm.walkableCellTypes = walkableTypes;

			path = new Stack<Cell>();

			InputManager.Instance.pathFinding = this;

			isWalking = false;
		}

		public void DestroyPathfinding()
		{
			if (pathPlane != null)
			{
				Destroy(pathPlane);
				pathPlane = null;
			}

			if (pathTexture != null)
			{
				Destroy(pathTexture);
				pathTexture = null;
			}

			isWalking = false;
		}

		public void UpdatePathfinding(Address playerPosition)
		{
			if (pathPlane == null) return;

			foreach (Cell c in path)
				pathTexture.SetPixel(c.x, c.y, emptyColor);

			if (path.Count > 0 && path.Peek().address.Equals(playerPosition))
				path.Pop();
			else
			{
				try
				{
					//Debug.Log("done a pathfinding");
					path = algorithm.GetFastestPath(map.cellsOnMap[playerPosition.x, playerPosition.y], map.cellsOnMap[targetX, targetY]);
				}
				catch (System.Exception)
				{
					path.Clear();
				}

			}

			foreach (Cell c in path)
				pathTexture.SetPixel(c.x, c.y, pathColor);

			pathTexture.Apply();
			pathPlane.GetComponent<Renderer>().material.SetTexture("_MainTex", pathTexture);

			lastPlayerPosition = playerPosition;

		}

		public void SetInput(Vector2 mousePosition, bool click)
		{
			mousePos = mousePosition;
			hasClicked = click;
		}

		public void Update()
		{
			if (pathPlane == null) return;

			if (isWalking == false)
			{
				Ray ray = Camera.main.ScreenPointToRay((Vector3)mousePos);

				List<RaycastHit> hits = new List<RaycastHit>(Physics.RaycastAll(ray));

				RaycastHit hit = hits.Find(h => h.collider.gameObject.tag == "PathfindingPlane");

				if (hit.collider != null)
				{

					int idxX = Mathf.RoundToInt(hit.point.x / ProDManager.Instance.tileSpacingX);
					int idxY;
					if (ProDManager.Instance.topDown)
					{
						idxY = Mathf.RoundToInt(hit.point.z / ProDManager.Instance.tileSpacingY);
					}
					else
					{
						idxY = Mathf.RoundToInt(hit.point.y / ProDManager.Instance.tileSpacingY);
					}

					if (fogOfWar.FogTexture != null &&
							((limitedTo == LimitedTo.VisitedOnly && fogOfWar.FogTexture.GetPixel(idxX, idxY).Equals(fogOfWar.unvisited))
							|| limitedTo == LimitedTo.VisibleOnly && !fogOfWar.FogTexture.GetPixel(idxX, idxY).Equals(fogOfWar.visible)))
					{
						idxX = lastPlayerPosition.x;
						idxY = lastPlayerPosition.y;
					}

					if (idxX != targetX || idxY != targetY) //Only update pathfinding when there is a change
					{

						targetX = idxX;
						targetY = idxY;
						UpdatePathfinding(lastPlayerPosition);
					}
				}
				else
				{
					targetX = lastPlayerPosition.x;
					targetY = lastPlayerPosition.y;
					UpdatePathfinding(lastPlayerPosition);
				}

				if (hasClicked)
				{
					isWalking = true;
				}
			}
			else
			{
				if (path.Count == 0)
				{
					isWalking = false;
				}
				else if (Time.time - walkTime > 1 / walkSpeed)
				{
					Address nextStep = path.Peek().address;


					int moveX = nextStep.x - lastPlayerPosition.x;
					int moveY = nextStep.y - lastPlayerPosition.y;

					//Debug.Log("path: " + nextStep.x + "   " + nextStep.y);
					//Debug.Log("pos : " + lastPlayerPosition.x + "   " + lastPlayerPosition.y);

					turnBasedMovement.SetInput(new Vector2(moveX, moveY));

					walkTime = Time.time;
				}

			}
		}
	}
}

