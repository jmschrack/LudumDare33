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
	/// <summary>
	/// Pro D manager. The main class for managing all ProD related stuff.
	/// </summary>
	public class ProDManager : Singleton<ProDManager>
	{
		public float tileSpacingX = 1;
		public float tileSpacingY = 1;

		public bool topDown = true;

		public bool UseSeed = false;

        public int numEnemy = 5;
        private List<GameObject> enemies;

		private string _PlayerPrefabName = "PRE_Player";
		public string playerPrefabName
		{
			get { return _PlayerPrefabName; }
			set { _PlayerPrefabName = value; }
		}

		private GameObject playerGO;
        private GameObject enemyGO;

		private int _seed = 0;
		public int Seed
		{
			get { return _seed; }
			set
			{
				if (UseSeed)
				{
					_seed = value;
					Random.seed = _seed;
				}
			}
		}

        public void Awake()
        {
            enemies = new List<GameObject>();
        }

		public void ApplySeed()
		{
			if (UseSeed)
				Random.seed = _seed;
		}
		/// <deprecated>
		/// this uses the resource folder and fixed naming and should not be used!
		/// </deprecated>
		/// <summary>
		/// Spawns the player in a specified world. 
		/// </summary>
		/// <param name='world'>
		/// The world to respawn the player in.
		/// </param>
		public void SpawnPlayer(WorldMap world)
		{
			SpawnPlayer(Resources.Load("Player/" + _PlayerPrefabName) as GameObject, world);
		}

		/// <deprecated>
		/// this uses the resource folder and fixed naming and should not be used!
		/// </deprecated>
		/// <summary>
		/// Spawns the player in a specified world. 
		/// </summary>
		/// <param name='world'>
		/// The world to respawn the player in.
		/// </param>
		/// <param name='mapAdress'>
		/// The map on which the player should be spawned.
		/// </param>
		/// <param name='spawnPoint'>
		/// The position on which the player should be spawned.
		/// </param>
		public void SpawnPlayer(WorldMap world, Address mapAdress, Address spawnPoint)
		{
			SpawnPlayer(Resources.Load("Player/" + _PlayerPrefabName) as GameObject, world, mapAdress, spawnPoint);
		}

        public void SpawnEnemy(GameObject enemyPrefab, WorldMap world)
        {
            //if (enemies != null)
            //{
                while (enemies.Count <= numEnemy)
                {                    
                    enemyGO = (GameObject)Instantiate(enemyPrefab);
                    EnemyMovement enemyMovement = enemyGO.GetComponent<EnemyMovement>();
                    if (enemyMovement) enemyMovement.SetupPlayer(world);
                    enemies.Add(enemyGO);
                    
                }
            //}

        }

		/// <summary>
		/// Spawns the player in a specified world.
		/// </summary>
		/// <param name='playerPrefab'>
		/// The prefab of the player.
		/// </param>
		/// <param name='world'>
		/// The world to respawn the player in.
		/// </param>
		public void SpawnPlayer(GameObject playerPrefab, WorldMap world)
		{
			if (playerGO == null)
			{
				playerGO = (GameObject)Instantiate(playerPrefab);
				PlayerMovement playerMovement = playerGO.GetComponent<PlayerMovement>();

				if (playerMovement) playerMovement.SetupPlayer(world);
			}
		}

		/// <summary>
		/// Spawns the player in a specified world. 
		/// </summary>
		/// <param name='playerPrefab'>
		/// The prefab of the player.
		/// </param>
		/// <param name='world'>
		/// The world to respawn the player in.
		/// </param>
		/// <param name='mapAdress'>
		/// The map on which the player should be spawned.
		/// </param>
		/// <param name='spawnPoint'>
		/// The position on which the player should be spawned.
		/// </param>
		public void SpawnPlayer(GameObject playerPrefab, WorldMap world, Address mapAdress, Address spawnPoint)
		{
			if (playerGO == null)
			{
				playerGO = (GameObject)Instantiate(playerPrefab);
				PlayerMovement playerMovement = playerGO.GetComponent<PlayerMovement>();
				if (playerMovement) playerMovement.SetupPlayer(world, mapAdress, spawnPoint);
			}

		}

		/// <summary>
		/// Removes the player properly, if there is one.
		/// </summary>
		public void DestroyPlayer()
		{
			if (playerGO != null)
			{
				Destroy(playerGO);
				playerGO = null;
			}
		}

		public FogOfWar getFogOfWar()
		{
			FogOfWar fow = (FogOfWar)this.GetComponent(typeof(FogOfWar));
			return fow;
		}

		public PathFinding getPathfinding()
		{
			PathFinding pathfinding = (PathFinding)this.GetComponent(typeof(PathFinding));
			return pathfinding;
		}
	}
}
