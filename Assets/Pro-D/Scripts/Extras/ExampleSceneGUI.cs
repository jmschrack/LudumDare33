/* 
* This code has been designed and developed by Gray Lake Studios.
* You may only use this code if you’ve acquired the appropriate license.
* To acquire such licenses you may visit www.graylakestudios.com and/or Unity Asset Store
* For all inquiries you may contact contact@graylakestudios.com
* Copyright © 2012 Gray Lake Studios
*/

using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProD
{
	public class ExampleSceneGUI : MonoBehaviour
	{
		public GUISkin skin;
		public Texture logo;
		public Texture logo_bg;

		public bool showGUI = true;

		public GameObject player2DPrefab;
		public GameObject player3DPrefab;
        public GameObject enemy3DPrefab;

		public GameObject cameraGO;

		public WorldMap worldMap;


		private Texture mapTexture;
		private bool mapTextureNeedsUpdate;

		#region Gui colors
		public Color bgBox_Top;
		public Color bgBox_Bot;
		public Color buttons;
		public Color worldVars;
		public Color playerVars;
		public Color generatorVars;
		public Color materializeButton;

		public float greyedOutAlpha;
		#endregion

		#region Gui sizes
		public float winLeft;
		public float winTop;
		public float winWidth;
		public float winHeight;
		public float winHeightCollapsed;

		private int line;
		public float lineHeight;

		public float scrollLineLeft;

		public float scrollWidth;
		public float scrollHeight;
		public float scrollInnerHeight;
		public float scrollLeftWidth;
		public float scrollRightWidth;

		public float textureTop;
		public float textureHeight;

		private int lineHelp;

		public float helpLeft;
		public float helpTop;
		public float helpWidth;

		public float tooltipLeft;
		public float tooltipWidth;

		#endregion

		#region Gui sizes splashscreen
		public float splashLeft;
		public float splashTop;
		public float splashWidth;
		public float splashHeight;

		#endregion

		#region Gui element values
		#region General
		private bool isSplashShowing = true;
		private bool isCollapsed = false;
		private Vector2 scrollPosition = Vector2.zero;

		private int worldWidth = 1;
		private int worldHeight = 1;
		private int mapWidth = 29;
		private int mapHeight = 29;


		private PlayerType playerType = PlayerType.None2D;
		private bool playerTypeDropdown = false;
		private bool useFollowCam = true;
		private bool useFow = true;
		private int fowRange = 6;
		private FogOfWar.ShadowType fowType = FogOfWar.ShadowType.Flood;
		private bool fowTypeDropdown = false;
		private FilterMode fowFilterMode = FilterMode.Bilinear;
		private bool fowFilterModeDropdown = false;

		private bool usePathfinding = true;
		private PathFinding.LimitedTo pathfindingLimitedTo = PathFinding.LimitedTo.VisibleOnly;
		private bool pathfindingLimitedToDropdown = false;
		private int pathfindingWalkSpeed = 100;

		private GeneratorType generatorType = GeneratorType.Dungeon;
		private bool generatorTypeDropdown = false;

		private bool isGenerating = false;

		private bool useSeed = false;
		private string seedString = "";

		private string currentTheme = "Sandy Theme";
		private bool currentThemeDropdown = false;

		public List<string> themes;
		public List<string> themes3D;

		private int blockedLineStart = -1;
		private int blockedLineEnd = -1;


		public PlayerType materializedPlayerType { get; private set; }
		private bool materializedUsePathfinding = true;
		private bool materializedUseFollowCam = true;


		#endregion

		#region Cavern
		private int cavern_thickness = 2;
		private int cavern_repeat_0 = 300;
		private int cavern_repeat_1 = 8;
		private int cavern_conversion_Density = 4;
		private int cavern_frame_Size = 1;
		#endregion
		#region Dungeon
		private int dungeon_room_Min_X = 3;
		private int dungeon_room_Max_X = 11;
		private int dungeon_room_Min_Y = 3;
		private int dungeon_room_Max_Y = 11;
		private int dungeon_room_Freq = 10;
		private int dungeon_room_Retry = 6;
		private int dungeon_doorsPerRoom = 1;
		private int dungeon_repeat = 12;
		private bool dungeon_frameMap = false;
		#endregion
		#region AlternativeDungeon
		private int alternativeDungeon_room_Min_X = 3;
		private int alternativeDungeon_room_Max_X = 11;
		private int alternativeDungeon_room_Min_Y = 3;
		private int alternativeDungeon_room_Max_Y = 11;
		private int alternativeDungeon_room_Freq = 10;
		private int alternativeDungeon_room_Retry = 6;
		private int alternativeDungeon_repeat = 12;
		private bool alternativeDungeon_frameMap = false;
		private bool alternativeDungeon_createDoors = false;
		#endregion
		#region DungeonRuins
		private int dungeonRuins_room_Min_X = 3;
		private int dungeonRuins_room_Max_X = 11;
		private int dungeonRuins_room_Min_Y = 3;
		private int dungeonRuins_room_Max_Y = 11;
		private int dungeonRuins_room_Freq = 10;
		private int dungeonRuins_room_Retry = 6;
		private int dungeonRuins_doorsPerRoom = 1;
		private int dungeonRuins_repeat = 12;
		#endregion
		#region ObstacleBiome
		private int obstacleBiome_conversion_Density = 5;
		private int obstacleBiome_frame_Size = 1;
		private int obstacleBiome_repeat_0 = 400;
		private int obstacleBiome_repeat_1 = 8;
		#endregion
		#region PerlinLikeBiome
		private int perlinLikeBiome_conversion_Density = 5;
		private int perlinLikeBiome_frame_Size = 1;
		private int perlinLikeBiome_repeat_0 = 500;
		private int perlinLikeBiome_repeat_1 = 8;
		#endregion
		#region RockyHill
		private int rockyHill_conversion_Density = 5;
		private int rockyHill_frame_Size = 1;
		private int rockyHill_repeat_0 = 500;
		private int rockyHill_repeat_1 = 8;
		#endregion
		#region StickBiome
		private int stickBiome_frame_Size = 1;
		private int stickBiome_repeat_0 = 50;
		#endregion
		#region StickDungeon
		private int stickDungeon_frame_Size = 1;
		private int stickDungeon_repeat_0 = 50;
		#endregion
		#region RoundRooms
		private int roundRooms_room_Diameter = 7;
		private int roundRooms_room_Spacing = 6;
		#endregion
		#region DwarfTown
		private int dwarfTown_room1_Min_X = 3;
		private int dwarfTown_room1_Max_X = 11;
		private int dwarfTown_room1_Min_Y = 3;
		private int dwarfTown_room1_Max_Y = 11;
		private int dwarfTown_room1_Freq = 10;
		private int dwarfTown_room1_Retry = 6;
		private int dwarfTown_room2_Min_X = 3;
		private int dwarfTown_room2_Max_X = 11;
		private int dwarfTown_room2_Min_Y = 3;
		private int dwarfTown_room2_Max_Y = 11;
		private int dwarfTown_room2_Freq = 10;
		private int dwarfTown_room2_Retry = 6;
		#endregion
		#region Castle
		private int castle_room_Freq = 5;
		private int castle_tower_Freq = 20;
		private int castle_tower_Dist = 7;
		private int castle_tower_Diameter = 2;
		private int castle_gate_Count = 1;
		private int castle_doorsPerRoom = 2;
		private int castle_ward_Chance = 3;
		private int castle_ward_growth = 6;
		#endregion

		#endregion

		public enum GeneratorType
		{
			Cavern,
			Dungeon,
			AlternativeDungeon,
			DungeonRuins,
			Maze,
			ObstacleBiome,
			PerlinLikeBiome,
			RockyHill,
			StickBiome,
			StickDungeon,
			RoundRooms,
			DwarfTown,
			Castle
		};

		public enum PlayerType
		{
			None2D,
			Player2D,
			Player3D
		}

		// Use this for initialization
		void OnEnable()
		{
			mapTextureNeedsUpdate = false;

			ProDManager.Instance.UseSeed = true;

			generate(); //To start with a map.
			#region Gui sizes
			/*
			 * It may be more practical to use the prefab values.
			 * I will leave this here in case something gets messed up.
			 * 
			lineHeight = 0.03f;

			winLeft = 0.69f;
			winTop = 0.02f;
			winWidth = 0.3f;
			winHeight = 30 * lineHeight;

			scrollLineLeft = 0.02f;

			scrollWidth = 0.28f;
			scrollHeight = 18 * lineHeight;
			scrollInnerHeight = 40 * lineHeight;
			scrollLeftWidth = 0.14f;
			scrollRightWidth = scrollWidth - scrollLeftWidth - scrollLineLeft;

			textureTop = winTop + 20 * lineHeight;
			textureHeight = 8 * lineHeight;

			splashLeft = 0.2f;
			splashTop= 0.2f;
			splashWidth = 0.6f;
			splashHeight = 24 * lineHeight;
			*/
			#endregion
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.H))
			{
				isCollapsed = !isCollapsed;
			}

			if (Input.GetKeyDown(KeyCode.Return))
			{
				materialize();
			}

			if (isGenerating && useSeed == false) generate();

			if (mapTextureNeedsUpdate)
			{
				if (mapTexture != null) Destroy(mapTexture);
				mapTexture = Materializer.Instance.dataLayer.ConvertMapToTexture(worldMap.maps[0, 0], false);

				mapTextureNeedsUpdate = false;
			}
		}

		/// <summary>
		/// The GUI to adjust all settings.
		/// </summary>
		void OnGUI()
		{
			bool needsRematerialize = false;

			blockedLineStart = -1;
			blockedLineEnd = -1;

			if (showGUI == false) return;

			GUI.skin = skin;

			#region splash

			if (isSplashShowing)
			{
				Rect splashRect = new Rect(relW(splashLeft), relH(splashTop), relW(splashWidth), relH(splashHeight));
				//GUI.Box(splashRect, "");
				GUI.DrawTexture(splashRect, logo_bg, ScaleMode.StretchToFill);
				GUI.DrawTexture(splashRect, logo, ScaleMode.ScaleToFit);

				Rect scrollPosRect = new Rect(relW(splashLeft), relH(splashTop), relW(splashWidth), relH(splashHeight - lineHeight));
				Rect scrollInnerRect = new Rect(0.0f, 0.0f, relW(splashWidth - 14), relH(16 * lineHeight));
				scrollPosition = GUI.BeginScrollView(scrollPosRect, scrollPosition, scrollInnerRect);


				//Rect headlineRect = new Rect(0.0f, 0.0f, relW(splashWidth), relH(3 * lineHeight));
				//GUI.Label(headlineRect, "Welcome to Pro-D 4.1!", "Headline");

				//Rect logoRect = new Rect(relW(splashWidth / 2), 0.0f, relW(splashWidth / 2), relH(splashHeight - lineHeight));
				//GUI.DrawTexture(logoRect, logo, ScaleMode.ScaleToFit);

				Rect textRect = new Rect(0f, 0f, relW(splashWidth / 2.7f), relH(8 * lineHeight));
				GUI.Label(textRect,
						  "Welcome to Pro-D 4.3! \n\nRemember to read the manual if you're stuck. You can visit the forums, doxygen documentation or e-mail us for help at any given time! \n\nEnjoy Pro-D!"
					, "Text");

				GUI.EndScrollView(true);

				Rect docButtonRect = new Rect(relW(splashLeft) + 5, relH(splashTop + splashHeight - lineHeight), relW(splashWidth / 3), relH(lineHeight * 2));
				if (GUI.Button(docButtonRect, "Doxygen Help"))
				{
					Application.OpenURL("http://graylakestudios.com/graylakestudios/graylake_redirect/rd_prod_doxygen.html");
				}

				Rect communityButtonRect = new Rect(relW(splashLeft + splashWidth / 3) + 1, relH(splashTop + splashHeight - lineHeight), relW(splashWidth / 3), relH(lineHeight * 2));
				if (GUI.Button(communityButtonRect, "r/ProD Forums"))
				{
					Application.OpenURL("http://graylakestudios.com/graylakestudios/graylake_redirect/rd_prod_community.html");
				}

				GUI.color = Color.green;
				Rect closeButtonRect = new Rect(relW(splashLeft + 2 * splashWidth / 3) + 2, relH(splashTop + splashHeight - lineHeight), relW(splashWidth / 3), relH(lineHeight * 2));
				if (GUI.Button(closeButtonRect, "START TESTING"))
				{
					isSplashShowing = false;

					materialize();
				}
				GUI.color = Color.white;

				return;
			}

			#endregion

			#region helpbox

			if (!isCollapsed)
			{
				lineHelp = 0;

				makeLabelInHelp("'H' to hide the GUI.");

				if (materializedPlayerType == PlayerType.Player2D || materializedPlayerType == PlayerType.Player3D)
				{
					makeLabelInHelp("'ARROW KEYS' to move the player.");

					if (materializedPlayerType == PlayerType.Player2D && materializedUsePathfinding)
					{
						makeLabelInHelp("'CLICK' to auto-walk.");

					}
				}

				if (materializedPlayerType == PlayerType.Player3D)
				{
					makeLabelInHelp("'F' to toggle flying.");
					makeLabelInHelp("'SPACE' to jump/fly up.");
					makeLabelInHelp("'SHIFT' to fly down.");
				}
				else
				{

					makeLabelInHelp("'CLICK+DRAG' to move camera,");

					if (materializedPlayerType == PlayerType.Player2D && materializedUseFollowCam)
					{
						makeLabelInHelp("'SPACE' to focus camera on player.");

					} 
				}

				makeLabelInHelp("'RETURN' to materialize.");

			}

			#endregion

			//Rect windowRect = new Rect(relW(winLeft), relH(winTop), relW(winWidth), isCollapsed ? relH(lineHeight) : relH(winHeight));
			//GUI.Window(-1, windowRect, null, "");
			GUI.color = bgBox_Top;
			Rect bgBoxRect_Top = new Rect(relW(winLeft), relH(winTop), relW(winWidth), isCollapsed ? relH(lineHeight) : relH(winHeight));
			GUI.Box(bgBoxRect_Top, "");
			GUI.color = bgBox_Bot;
			Rect bgBoxRect_Bot = new Rect(relW(winLeft), relH(textureTop), relW(winWidth), isCollapsed ? relH(0) : relH(textureHeight));
			GUI.Box(bgBoxRect_Bot, "");
			GUI.color = Color.white;


			Rect collapseButtonRect = new Rect(relW(winLeft) - 3, relH(winTop), relW(winWidth) + 5, relH(lineHeight));
			if (GUI.Button(collapseButtonRect, isCollapsed ? "show generator settings" : "hide"))
			{
				isCollapsed = !isCollapsed;
			}

			if (!isCollapsed)
			{
				#region ScrollView
				Rect scrollPosRect = new Rect(relW(winLeft), relH(winTop + lineHeight), relW(winWidth), relH(scrollHeight));
				Rect scrollInnerRect = new Rect(0.0f, 0.0f, relW(scrollWidth), relH(scrollInnerHeight));
				scrollPosition = GUI.BeginScrollView(scrollPosRect, scrollPosition, scrollInnerRect);

				line = 0;

				#region general settings

				GUI.color = playerVars;

				//if (makeToggleInScrollbar("place a player", ref usePlayer, true, "'Place a player' will spawn a player for you to test the map."))

				if (playerType != makeDropdownInScrollbar<PlayerType>("Player", ref playerType, ref playerTypeDropdown, true, "The type of player you want to spawn to test the map."))
				{
					needsRematerialize = true;
				}

				switch (playerType)
				{
					case PlayerType.None2D:
						break;
					case PlayerType.Player2D:
						if (themes.Count == 0)
						{
							Debug.Log("There are no 2D Themes assigned." + Environment.NewLine + "Create some and assign them in the ExampeSceneGUI object.");
							playerType = PlayerType.Player3D;
						}

						makeToggleInScrollbar("follow player", ref useFollowCam, true, "'Follow player' will lock the camera on the player if checked.");

						makeToggleInScrollbar("use fog of war", ref useFow, true, "'Use fog of war' will cast fog of war and apply shadow type as players's field of vision.");

						makeSliderInScrollbar("visibility range", ref fowRange, 2, 15, useFow, "'Visibility range' limits the distance of the players field of vision.");
						makeDropdownInScrollbar<FogOfWar.ShadowType>("shadow type", ref fowType, ref fowTypeDropdown, useFow, "'Shadow type' is the type of algorithm used in calculating player's field of vision.");
						makeDropdownInScrollbar<FilterMode>("filter mode", ref fowFilterMode, ref fowFilterModeDropdown, useFow, "'Filter mode' will configure the edges of the shadow type.");

						makeToggleInScrollbar("use pathfinding", ref usePathfinding, true, "'Use pathfinding' will show a travelling red line that indicates the pathfinding algorithms solution from Player's position to another path on the map.");

						makeSliderInScrollbar("auto-walk speed", ref pathfindingWalkSpeed, 1, 150, usePathfinding, "'Auto-walk speed' defines the speed it will take for the player to travel from starting location to target location.");

						makeDropdownInScrollbar<PathFinding.LimitedTo>("limited to", ref pathfindingLimitedTo, ref pathfindingLimitedToDropdown, usePathfinding && useFow, "'Limited to' defines the space the player is allowed to travel");

						break;
					case PlayerType.Player3D:
						if (themes3D.Count == 0)
						{
							Debug.Log("There are no 3D Themes assigned." + Environment.NewLine + "Create some and assign them in the ExampeSceneGUI object.");
							playerType = PlayerType.None2D;
						}
						break;
					default:
						break;
				}



				line = 10; //harcoded so player settings have fixed space

				GUI.color = worldVars;

				makeSliderInScrollbar("world width", ref worldWidth, 1, 3, true, "World Width is the count of individual maps generated horizontally.");
				makeSliderInScrollbar("world height", ref worldHeight, 1, 3, true, "World Height is the count of individual maps generated vertically.");
				makeSliderInScrollbar("map width", ref mapWidth, 15, 55, true, "Map Width is the total number of cells a map has from left to right.");
				makeSliderInScrollbar("map height", ref mapHeight, 15, 55, true, "Map Height is the total number of cells a map has from top to bottom.");
				if (mapWidth % 2 == 0) mapWidth++;
				if (mapHeight % 2 == 0) mapHeight++;

				List<string> themesToDisplay = new List<string>();
				switch (playerType)
				{
					case PlayerType.None2D:
					case PlayerType.Player2D:
						themesToDisplay.AddRange(themes);
						break;
					case PlayerType.Player3D:
						themesToDisplay.AddRange(themes3D);
						break;
					default:
						break;
				}

				if (currentTheme != makeDropdownInScrollbar("theme", ref currentTheme, themesToDisplay, ref currentThemeDropdown, true, "Theme defines the set of visual assets used for a map. Graphical assets used in a theme are in the Resources folder of your project."))
				{
					needsRematerialize = true;
				}

				line++;

				GUI.color = generatorVars;

				makeDropdownInScrollbar<GeneratorType>("generator", ref generatorType, ref generatorTypeDropdown, true, "Generator is the type of generator script used in creating the map.");

				#endregion

				#region generator specific settings

				switch (generatorType)
				{
					case GeneratorType.Cavern:
						showSettingsCavern();
						break;

					case GeneratorType.Dungeon:
						showSettingsDungeon();
						break;

					case GeneratorType.AlternativeDungeon:
						showSettingsAlternativeDungeon();
						break;

					case GeneratorType.DungeonRuins:
						showSettingsDungeonRuins();
						break;

					case GeneratorType.ObstacleBiome:
						showSettingsObstacleBiome();
						break;

					case GeneratorType.PerlinLikeBiome:
						showSettingsPerlinLikeBiome();
						break;

					case GeneratorType.RockyHill:
						showSettingsRockyHill();
						break;

					case GeneratorType.StickBiome:
						showSettingsStickBiome();
						break;

					case GeneratorType.StickDungeon:
						showSettingsStickDungeon();
						break;

					case GeneratorType.RoundRooms:
						showSettingsRoundRooms();
						break;

					case GeneratorType.DwarfTown:
						showSettingsDwarfTown();
						break;

					case GeneratorType.Castle:
						showSettingsCastle();
						break;

					default:
						break;
				}

				#endregion


				GUI.EndScrollView(true);

				scrollInnerHeight = 30 * lineHeight;
				//scrollInnerHeight = (line + 1) * lineHeight;

				GUI.color = Color.white;

				#endregion


				#region lower part

				if (mapTexture != null)
				{
					Rect textureRect = new Rect(relW(winLeft), relH(textureTop), relH(textureHeight), relH(textureHeight));
					GUI.DrawTexture(textureRect, mapTexture, ScaleMode.ScaleToFit);
				}

				Rect seedToggleRect = new Rect(relW(winLeft) + relH(textureHeight), relH(textureTop), relW(winWidth) - relH(textureHeight), relH(lineHeight));
				useSeed = GUI.Toggle(seedToggleRect, useSeed, new GUIContent("use custom seed", "Use custom seed will allows to you enter a seed in the seed box. Therefore you can recreate maps."));

				Rect seedTextRect = new Rect(relW(winLeft) + relH(textureHeight), relH(textureTop + lineHeight), (relW(winWidth) - relH(textureHeight)), relH(lineHeight));
				Rect generateButtonRect = new Rect(relW(winLeft) + relH(textureHeight), relH(textureTop + 2 * lineHeight), relW(winWidth) - relH(textureHeight), relH(lineHeight));

				GUI.Label(seedTextRect, new GUIContent("", "This is the seed."));

				if (useSeed)
				{
					seedString = GUI.TextField(seedTextRect, seedString);

					GUI.color = materializeButton;
					if (GUI.Button(generateButtonRect, new GUIContent("apply", "Apply will generate a map according to the variables you have entered."), "Button1")) generate();
				}
				else
				{
					Color grayed = GUI.color;
					grayed.a = greyedOutAlpha;
					GUI.color = grayed;

					GUI.TextField(seedTextRect, seedString);

					if (!isGenerating) GUI.color = materializeButton;
					else GUI.color = Color.red;
					if (GUI.Button(generateButtonRect, new GUIContent(isGenerating ? "stop" : "generate new", "Generate new will start generating random maps according to the variables you have entered."), "Button1")) isGenerating = !isGenerating; //the actual generate is called in the update
				}
				GUI.color = Color.white;


				string buildNotice = "";
				string buttonStyle = "Button1";
#if !UNITY_EDITOR
					buildNotice = " (not supported in WebPlayer)";
					buttonStyle = "Button1Disabled";
#endif

				Rect saveButtonRect = new Rect(relW(winLeft) + relH(textureHeight), relH(textureTop + 3 * lineHeight), relW(winWidth) - relH(textureHeight), relH(lineHeight));
				if (GUI.Button(saveButtonRect, new GUIContent("save to file", "Save to file will save the last generated map as an .tmx file you can edit with the free Tiled software." + buildNotice), buttonStyle)) saveToFile();

				Rect loadButtonRect = new Rect(relW(winLeft) + relH(textureHeight), relH(textureTop + 4 * lineHeight), relW(winWidth) - relH(textureHeight), relH(lineHeight));
				if (GUI.Button(loadButtonRect, new GUIContent("load from file", "Load from file will load a .tmx type map and materialize it." + buildNotice), buttonStyle)) loadFromFile();

				GUI.color = materializeButton;

				Rect materializeButtonRect = new Rect(relW(winLeft) - 3, relH(textureTop + textureHeight), relW(winWidth) + 5, relH(lineHeight * 2));
				if (GUI.Button(materializeButtonRect, new GUIContent("MATERIALIZE", "Materialize will create the map in the scene when clicked. This will delete the old map and place the latest generated map and its prefabs into the scene.")))
				{
					materialize();
				}

				GUI.color = Color.white;

				#endregion

				if(needsRematerialize)
				{
					bool oldUseSeed = useSeed;
					useSeed = true;
					generate();
					materialize();
					useSeed = oldUseSeed;
				}
			}

			Rect tooltipRect = new Rect(relW(tooltipLeft), Screen.height - Input.mousePosition.y - relH(4 * lineHeight), relW(tooltipWidth), relH(8 * lineHeight));
			GUI.Label(tooltipRect, GUI.tooltip, "Tooltip");

		}

		/// <summary>
		/// Generates a WorldMap with the current settings.
		/// </summary>
		private void generate()
		{
			if (!useSeed)
			{
				seedString = UnityEngine.Random.seed.ToString();
			}

			int seed;
			if (int.TryParse(seedString, out seed) == false)
			{
				seed = GetIntHashCode(seedString);

				Debug.Log("The seed you entered is not a integer. We converted it to an int for you." + Environment.NewLine + "You can see how it was converted in the ExampleSceneGUI.cs script, in the GetIntHashCode(string strText) function.");
			}
			ProDManager.Instance.Seed = seed;
			ProDManager.Instance.ApplySeed();

			//then we need to set generator specific properties for the generator we choose to use.
			//all those parameters are filled by the GUI.
			switch (generatorType)
			{
				case GeneratorType.Cavern:
					Generator_Cavern.SetSpecificProperties("Abyss", "Path", "Wall", cavern_thickness, cavern_repeat_0, cavern_repeat_1, cavern_conversion_Density, cavern_frame_Size);
					break;
				case GeneratorType.Dungeon:
					Generator_Dungeon.SetSpecificProperties("Abyss", "Path", "Wall", dungeon_room_Min_X, dungeon_room_Max_X, dungeon_room_Min_Y, dungeon_room_Max_Y, dungeon_room_Freq, dungeon_room_Retry, dungeon_doorsPerRoom, dungeon_repeat, dungeon_frameMap);
					break;
				case GeneratorType.AlternativeDungeon:
					Generator_AlternativeDungeon.SetSpecificProperties("Abyss", "Path", "Wall", alternativeDungeon_room_Min_X, alternativeDungeon_room_Max_X, alternativeDungeon_room_Min_Y, alternativeDungeon_room_Max_Y, alternativeDungeon_room_Freq, alternativeDungeon_room_Retry, alternativeDungeon_repeat, alternativeDungeon_frameMap, alternativeDungeon_createDoors);
					break;
				case GeneratorType.DungeonRuins:
					Generator_DungeonRuins.SetSpecificProperties("Abyss", "Path", "Wall", dungeonRuins_room_Min_X, dungeonRuins_room_Max_X, dungeonRuins_room_Min_Y, dungeonRuins_room_Max_Y, dungeonRuins_room_Freq, dungeonRuins_room_Retry, dungeonRuins_doorsPerRoom, dungeonRuins_repeat);
					break;
				case GeneratorType.RockyHill:
					Generator_RockyHill.SetSpecificProperties("Abyss", "Path", "Wall", rockyHill_conversion_Density, rockyHill_frame_Size, rockyHill_repeat_0, rockyHill_repeat_1);
					break;
				case GeneratorType.Maze:
					Generator_Maze.SetSpecificProperties("Abyss", "Path", "Wall");
					break;
				case GeneratorType.ObstacleBiome:
					Generator_ObstacleBiome.SetSpecificProperties("Abyss", "Path", "Wall", obstacleBiome_conversion_Density, obstacleBiome_frame_Size, obstacleBiome_repeat_0, obstacleBiome_repeat_1);
					break;
				case GeneratorType.PerlinLikeBiome:
					Generator_PerlinLikeBiome.SetSpecificProperties("Abyss", "Path", "Wall", perlinLikeBiome_conversion_Density, perlinLikeBiome_frame_Size, perlinLikeBiome_repeat_0, perlinLikeBiome_repeat_1);
					break;
				case GeneratorType.StickDungeon:
					Generator_StickDungeon.SetSpecificProperties("Abyss", "Path", "Wall", stickDungeon_frame_Size, stickDungeon_repeat_0);
					break;
				case GeneratorType.StickBiome:
					Generator_StickBiome.SetSpecificProperties("Abyss", "Path", "Wall", stickBiome_frame_Size, stickBiome_repeat_0);
					break;
				case GeneratorType.RoundRooms:
					Generator_RoundRooms.SetSpecificProperties("Abyss", "Path", "Wall", roundRooms_room_Diameter, roundRooms_room_Spacing);
					break;
				case GeneratorType.DwarfTown:
					Generator_DwarfTown.SetSpecificProperties("Abyss", "Path", "Wall", dwarfTown_room1_Min_X, dwarfTown_room1_Max_X, dwarfTown_room1_Min_Y, dwarfTown_room1_Max_Y, dwarfTown_room1_Freq, dwarfTown_room1_Retry, dwarfTown_room2_Min_X, dwarfTown_room2_Max_X, dwarfTown_room2_Min_Y, dwarfTown_room2_Max_Y, dwarfTown_room2_Freq, dwarfTown_room2_Retry);
					break;
				case GeneratorType.Castle:
					Generator_Castle.SetSpecificProperties("Abyss", "Path", "Wall", castle_room_Freq, castle_doorsPerRoom, castle_tower_Freq, castle_tower_Dist, castle_tower_Diameter, castle_ward_Chance, castle_ward_growth, castle_gate_Count);
					break;
				default:
					Debug.LogError("something went horribly wrong. We are sorry, this should not have happened.");
					return;
			}

			//then we set up the Generator_Generic_World.
			//this is a convenient way to create worlds, because it lets you choose which generator to use for the maps.
			//if you want a world consisting of different map types, have a look at the Generator_Variable_World.
			Generator_Generic_World.theme = currentTheme; //Set this in GUI.

			//we just tell it to generate our world, passing all the world relevant settings, and there we go, we just created our world.
			worldMap = Generator_Generic_World.Generate(generatorType.ToString(), worldWidth, worldHeight, mapWidth, mapHeight);

			//keep the preview up to date
			mapTextureNeedsUpdate = true;
		}

		/// <summary>
		/// Saves the first Map in the current WorldMap to a .tmx file.
		/// Is only available in Editor-Mode because of the file save dialog.
		/// </summary>
		private void saveToFile()
		{
#if UNITY_EDITOR //save/load dialog is only available in editor mode
			if (worldMap == null) return;

			string defaultFilename = generatorType.ToString() + " - " + worldMap.maps[0, 0].theme;

			//this uses the editor utility to display a save dialog
			//sadly this is not available in builds. therefore you have to find another way to determine the filepath in your application
			string fullPath = EditorUtility.SaveFilePanel("save map...", "", defaultFilename, "tmx");
			if (fullPath == null || fullPath.Equals(string.Empty)) return;
			string path = Path.GetDirectoryName(fullPath) + "/";
			string filename = Path.GetFileNameWithoutExtension(fullPath);

			//this simple call saves the map to a file. you can of course use this in a build.
			FilePorter.Instance.saveMapToTmx(worldMap.maps[0, 0], path, filename);

			//ensure that the spritesheet exists
			if (!System.IO.File.Exists(path + "defaultProD.png"))
			{
				Debug.Log("You might want to copy the defaultProD.png spritesheet into that directory. Else you wont be able to open that file with Tiled. you can find it in the package in ProD\\Visual Assets\\FilePorter\\");
			}
#endif
		}

		/// <summary>
		/// Loads a Map from a .tmx file and creates a new WorldMap with it.
		/// Is only available in Editor-Mode because of the file load dialog.
		/// </summary>
		private void loadFromFile()
		{
#if UNITY_EDITOR //save/load dialog is only available in editor mode
			isGenerating = false;

			//this uses the editor utility to display a load dialog
			//sadly this is not available in builds. therefore you have to find another way to determine the filepath in your application
			string fullPath = EditorUtility.OpenFilePanel("load map...", "", "tmx");
			if (fullPath == null || fullPath.Equals(string.Empty)) return;
			string path = Path.GetDirectoryName(fullPath) + "/";
			string filename = Path.GetFileNameWithoutExtension(fullPath);


			//this simple call loads the map from a file. you can of course use this in a build.
			Map map = FilePorter.Instance.loadMapFromTmx(path, filename);

			worldMap = new WorldMap(1, 1);
			map.worldMap = worldMap;
			map.addressOnWorldMap = new Address(0, 0);
			worldMap.maps[0, 0] = map;

			//keep the preview up to date
			mapTextureNeedsUpdate = true;
#endif
		}

		/// <summary>
		/// Materializes the current WorldMap with the current settings.
		/// </summary>
		private void materialize()
		{

			if (worldMap == null) return;

			//first of all we need to get rid of old stuff in case there is already a materialized world
			ProDManager.Instance.DestroyPlayer();
			ProDManager.Instance.getFogOfWar().DestroyFoW();
			ProDManager.Instance.getPathfinding().DestroyPathfinding();
			Materializer.Instance.UnmaterializeWorldMap();
			TurnManager.Instance.removeAllActors(); //we remove the actors here, but they add themself when they are instantiated

			//3d themes usually need rotation. in future releases this wil be solved more elegantly.
			Materializer.Instance.rotateTiles = themes3D.Contains(currentTheme);

			//with a simple line of code, the materializer instantiates the world for us.
			Materializer.Instance.MaterializeWorldMap(worldMap);


			//here we choose which camera movement script should be active
			//we disable the default camera, because the Player3D comes with his own first person camera.
			cameraGO.GetComponent<Camera>().enabled = playerType != PlayerType.Player3D;
			cameraGO.GetComponent<CameraObjectTracker>().enabled = playerType == PlayerType.Player2D && useFollowCam;

			switch (playerType)
			{
				case PlayerType.None2D:
					break;
				case PlayerType.Player2D:
					if (useFow)
					{
						//if we choose to use the fog of war, we need to set the parameters, and then just call the init method
						applyFowSettings();
						ProDManager.Instance.getFogOfWar().InitFoW(worldMap.maps[0, 0]);
					}

					if (usePathfinding)
					{
						//if we choose to use the pathfinding, we need to set the parameters, and then just call the init method
						applyPathfindingSettings();
						ProDManager.Instance.getPathfinding().InitPathfinding(worldMap.maps[0, 0]);
					}
					//reapply the seed to ensure that player spawns at same position every time we materialise a world created with a seed
					ProDManager.Instance.ApplySeed();
					//to spawn the player, only one simple call has to be done
					ProDManager.Instance.SpawnPlayer(player2DPrefab, worldMap);
					break;

				case PlayerType.Player3D:
					//reapply the seed to ensure that player spawns at same position every time we materialise a world created with a seed
					ProDManager.Instance.ApplySeed();
					//to spawn the player, only one simple call has to be done
					ProDManager.Instance.SpawnPlayer(player3DPrefab, worldMap);
                    print("Spawning Enemies");
                    ProDManager.Instance.SpawnEnemy(enemy3DPrefab, worldMap);
					break;

				default:
					break;
			}


			materializedPlayerType = playerType;
			materializedUsePathfinding = usePathfinding;
			materializedUseFollowCam = useFollowCam;

		}

		/// <summary>
		/// Sets the parameters for the fog of war.
		/// </summary>
		private void applyFowSettings()
		{
			ProDManager.Instance.getFogOfWar().visibilityRange = fowRange;
			ProDManager.Instance.getFogOfWar().type = fowType;
			ProDManager.Instance.getFogOfWar().filterMode = fowFilterMode;
		}

		/// <summary>
		/// Sets the parameters for the pathfinding.
		/// </summary>
		private void applyPathfindingSettings()
		{
			ProDManager.Instance.getPathfinding().limitedTo = useFow ? pathfindingLimitedTo : PathFinding.LimitedTo.All;
			ProDManager.Instance.getPathfinding().walkSpeed = pathfindingWalkSpeed;
			InputManager.Instance.allowDragClicks = useFollowCam;
		}

		#region generator specific Guis

		private void showSettingsCavern()
		{
			makeSliderInScrollbar("conversion density", ref cavern_conversion_Density, 1, 10, true, "Conversion density is the minimum amount of wall cells a cell in a map must have for that cell to converted into a wall cell.");
			makeSliderInScrollbar("filter frame size", ref cavern_frame_Size, 1, 5, true, "Filter frame size is the filter-free perimeter within the maps boundaries.");
			makeSliderInScrollbar("repeat noise", ref cavern_repeat_0, 100, 1000, true, "Repeat noise is the amount of times noise is applied.");
			makeSliderInScrollbar("repeat noise filter", ref cavern_repeat_1, 1, 20, true, "Repeat filter is the amount of times a filter is applied.");
			makeSliderInScrollbar("map frame size", ref cavern_thickness, 1, 5, true, "Map frame size is the thickness of a wall perimeter within the map boundaries.");
		}

		private void showSettingsDungeon()
		{
			makeSliderInScrollbar("room min x", ref dungeon_room_Min_X, 3, 31, true, "Room Min X is the minimum width of a room in cells.");
			makeSliderInScrollbar("room max x", ref dungeon_room_Max_X, 3, 31, true, "Room Max X is the maximum width of a room in cells.");
			makeSliderInScrollbar("room min y", ref dungeon_room_Min_Y, 3, 31, true, "Room Min Y is the minimum height of a room in cells.");
			makeSliderInScrollbar("room max y", ref dungeon_room_Max_Y, 3, 31, true, "Room Max Y is the maximum height of a room in cells.");
			if (dungeon_room_Min_X % 2 == 0) dungeon_room_Min_X++;
			if (dungeon_room_Max_X % 2 == 0) dungeon_room_Max_X++;
			if (dungeon_room_Min_Y % 2 == 0) dungeon_room_Min_Y++;
			if (dungeon_room_Max_Y % 2 == 0) dungeon_room_Max_Y++;
			if (dungeon_room_Min_X > dungeon_room_Max_X) dungeon_room_Min_X = dungeon_room_Max_X;
			if (dungeon_room_Min_Y > dungeon_room_Max_Y) dungeon_room_Min_Y = dungeon_room_Max_Y;

			makeSliderInScrollbar("room frequency", ref dungeon_room_Freq, 1, 50, true, "Room Frequency is the amount of attempts to place a room in the map.");
			makeSliderInScrollbar("room retry", ref dungeon_room_Retry, 1, 20, true, "Room Retry is the amount of retry attempts to place a room upon failing to place a room.");
			makeSliderInScrollbar("doors per room", ref dungeon_doorsPerRoom, 1, 4, true, "Doors per room is the amount of doors every room will have.");
			makeSliderInScrollbar("u path reduction", ref dungeon_repeat, 0, 20, true, "U path reduction is the amount of times the corridors are simplified in a map.");
			makeToggleInScrollbar("frame map", ref dungeon_frameMap, true, "Frame map draws a wall rectangle around a map if checked.");

		}

		private void showSettingsAlternativeDungeon()
		{
			makeSliderInScrollbar("room min x", ref alternativeDungeon_room_Min_X, 3, 31, true, "Room Min X is the minimum width of a room in cells.");
			makeSliderInScrollbar("room max x", ref alternativeDungeon_room_Max_X, 3, 31, true, "Room Max X is the maximum width of a room in cells.");
			makeSliderInScrollbar("room min y", ref alternativeDungeon_room_Min_Y, 3, 31, true, "Room Min Y is the minimum height of a room in cells.");
			makeSliderInScrollbar("room max y", ref alternativeDungeon_room_Max_Y, 3, 31, true, "Room Max Y is the maximum height of a room in cells.");
			if (alternativeDungeon_room_Min_X % 2 == 0) alternativeDungeon_room_Min_X++;
			if (alternativeDungeon_room_Max_X % 2 == 0) alternativeDungeon_room_Max_X++;
			if (alternativeDungeon_room_Min_Y % 2 == 0) alternativeDungeon_room_Min_Y++;
			if (alternativeDungeon_room_Max_Y % 2 == 0) alternativeDungeon_room_Max_Y++;
			if (alternativeDungeon_room_Min_X > alternativeDungeon_room_Max_X) alternativeDungeon_room_Min_X = alternativeDungeon_room_Max_X;
			if (alternativeDungeon_room_Min_Y > alternativeDungeon_room_Max_Y) alternativeDungeon_room_Min_Y = alternativeDungeon_room_Max_Y;

			makeSliderInScrollbar("room frequency", ref alternativeDungeon_room_Freq, 1, 50, true, "Room Frequency is the amount of attempts to place a room in the map.");
			makeSliderInScrollbar("room retry", ref alternativeDungeon_room_Retry, 1, 20, true, "Room Retry is the amount of retry attempts to place a room upon failing to place a room.");
			makeToggleInScrollbar("create doors", ref alternativeDungeon_createDoors, true, "Create Doors will add doors to the map if toggled.");
			makeSliderInScrollbar("u path reduction", ref alternativeDungeon_repeat, 0, 20, true, "U path reduction is the amount of times the corridors are simplified in a map.");
			makeToggleInScrollbar("frame map", ref alternativeDungeon_frameMap, true, "Frame map draws a wall rectangle around a map if checked.");

		}

		private void showSettingsDungeonRuins()
		{
			makeSliderInScrollbar("room min x", ref dungeonRuins_room_Min_X, 3, 31, true, "Room Min X is the minimum width of a room in cells.");
			makeSliderInScrollbar("room max x", ref dungeonRuins_room_Max_X, 3, 31, true, "Room Max X is the maximum width of a room in cells.");
			makeSliderInScrollbar("room min y", ref dungeonRuins_room_Min_Y, 3, 31, true, "Room Min Y is the minimum height of a room in cells.");
			makeSliderInScrollbar("room max y", ref dungeonRuins_room_Max_Y, 3, 31, true, "Room Max Y is the maximum height of a room in cells.");
			if (dungeonRuins_room_Min_X % 2 == 0) dungeonRuins_room_Min_X++;
			if (dungeonRuins_room_Max_X % 2 == 0) dungeonRuins_room_Max_X++;
			if (dungeonRuins_room_Min_Y % 2 == 0) dungeonRuins_room_Min_Y++;
			if (dungeonRuins_room_Max_Y % 2 == 0) dungeonRuins_room_Max_Y++;
			if (dungeonRuins_room_Min_X > dungeonRuins_room_Max_X) dungeonRuins_room_Min_X = dungeonRuins_room_Max_X;
			if (dungeonRuins_room_Min_Y > dungeonRuins_room_Max_Y) dungeonRuins_room_Min_Y = dungeonRuins_room_Max_Y;

			makeSliderInScrollbar("room frequency", ref dungeonRuins_room_Freq, 1, 50, true, "Room Frequency is the amount of attempts to place a room in the map.");
			makeSliderInScrollbar("room retry", ref dungeonRuins_room_Retry, 1, 20, true, "Room Retry is the amount of retry attempts to place a room upon failing to place a room.");
			makeSliderInScrollbar("doors per room", ref dungeonRuins_doorsPerRoom, 1, 4, true, "Doors per room is the amount of doors every room will have.");
			makeSliderInScrollbar("u path reduction", ref dungeonRuins_repeat, 0, 20, true, "U path reduction is the amount of times the corridors are simplified in a map.");

		}

		private void showSettingsObstacleBiome()
		{
			makeSliderInScrollbar("conversion density", ref obstacleBiome_conversion_Density, 1, 10, true, "Conversion density is the minimum amount of wall cells a cell in a map must have for that cell to converted into a wall cell.");
			makeSliderInScrollbar("filter frame size", ref obstacleBiome_frame_Size, 1, 5, true, "Filter frame size is the filter-free perimeter within the maps boundaries.");
			makeSliderInScrollbar("repeat noise", ref obstacleBiome_repeat_0, 100, 1000, true, "Repeat noise is the amount of times noise is applied.");
			makeSliderInScrollbar("repeat noise filter", ref obstacleBiome_repeat_1, 1, 20, true, "Repeat filter is the amount of times a filter is applied.");
		}

		private void showSettingsPerlinLikeBiome()
		{
			makeSliderInScrollbar("conversion density", ref perlinLikeBiome_conversion_Density, 1, 10, true, "Conversion density is the minimum amount of wall cells a cell in a map must have for that cell to converted into a wall cell.");
			makeSliderInScrollbar("filter frame size", ref perlinLikeBiome_frame_Size, 1, 5, true, "Filter frame size is the filter-free perimeter within the maps boundaries.");
			makeSliderInScrollbar("repeat noise", ref perlinLikeBiome_repeat_0, 100, 1000, true, "Repeat noise is the amount of times noise is applied.");
			makeSliderInScrollbar("repeat noise filter", ref perlinLikeBiome_repeat_1, 1, 20, true, "Repeat filter is the amount of times a filter is applied.");
		}

		private void showSettingsRockyHill()
		{
			makeSliderInScrollbar("conversion density", ref rockyHill_conversion_Density, 1, 10, true, "Conversion density is the minimum amount of wall cells a cell in a map must have for that cell to converted into a wall cell.");
			makeSliderInScrollbar("filter frame size", ref rockyHill_frame_Size, 1, 5, true, "Filter frame size is the filter-free perimeter within the maps boundaries.");
			makeSliderInScrollbar("repeat noise", ref rockyHill_repeat_0, 100, 1000, true, "Repeat noise is the amount of times noise is applied.");
			makeSliderInScrollbar("repeat noise filter", ref rockyHill_repeat_1, 1, 20, true, "Repeat filter is the amount of times a filter is applied.");
		}

		private void showSettingsStickBiome()
		{
			makeSliderInScrollbar("map frame size", ref stickBiome_frame_Size, 1, 5, true, "Map frame size is the thickness of a wall perimeter within the map boundaries.");
			makeSliderInScrollbar("repeat noise", ref stickBiome_repeat_0, 1, 200, true, "Repeat noise is the amount of times noise is applied.");
		}

		private void showSettingsStickDungeon()
		{
			makeSliderInScrollbar("map frame size", ref stickDungeon_frame_Size, 1, 5, true, "Map frame size is the thickness of a wall perimeter within the map boundaries.");
			makeSliderInScrollbar("repeat noise", ref stickDungeon_repeat_0, 1, 200, true, "Repeat noise is the amount of times noise is applied.");
		}

		private void showSettingsRoundRooms()
		{
			makeSliderInScrollbar("room diameter", ref roundRooms_room_Diameter, 5, 31, true, "Room diameter is the size of a round room in cells.");
			makeSliderInScrollbar("room spacing", ref roundRooms_room_Spacing, 3, 31, true, "Room spacing is the minimum distance between any given rooms in cells.");
			if (roundRooms_room_Diameter % 2 == 0) roundRooms_room_Diameter++;

		}

		private void showSettingsDwarfTown()
		{
			makeSliderInScrollbar("room1 min x", ref dwarfTown_room1_Min_X, 3, 31, true, "Room1 Min X is the minimum width of a room in cells.");
			makeSliderInScrollbar("room1 max x", ref dwarfTown_room1_Max_X, 3, 31, true, "Room1 Max X is the maximum width of a room in cells.");
			makeSliderInScrollbar("room1 min y", ref dwarfTown_room1_Min_Y, 3, 31, true, "Room1 Min Y is the minimum height of a room in cells.");
			makeSliderInScrollbar("room1 max y", ref dwarfTown_room1_Max_Y, 3, 31, true, "Room1 Max Y is the maximum height of a room in cells.");
			if (dwarfTown_room1_Min_X > dwarfTown_room1_Max_X) dwarfTown_room1_Min_X = dwarfTown_room1_Max_X;
			if (dwarfTown_room1_Min_Y > dwarfTown_room1_Max_Y) dwarfTown_room1_Min_Y = dwarfTown_room1_Max_Y;

			makeSliderInScrollbar("room1 frequency", ref dwarfTown_room1_Freq, 1, 50, true, "Room Frequency is the amount of attempts to place a room in the map.");
			makeSliderInScrollbar("room1 retry", ref dwarfTown_room1_Retry, 1, 20, true, "Room Retry is the amount of retry attempts to place a room upon failing to place a room.");

			makeSliderInScrollbar("room2 min x", ref dwarfTown_room2_Min_X, 3, 31, true, "Room2 Min X is the minimum width of a room in cells.");
			makeSliderInScrollbar("room2 max x", ref dwarfTown_room2_Max_X, 3, 31, true, "Room2 Max X is the maximum width of a room in cells.");
			makeSliderInScrollbar("room2 min y", ref dwarfTown_room2_Min_Y, 3, 31, true, "Room2 Min Y is the minimum height of a room in cells.");
			makeSliderInScrollbar("room2 max y", ref dwarfTown_room2_Max_Y, 3, 31, true, "Room2 Max Y is the maximum height of a room in cells.");
			if (dwarfTown_room2_Min_X > dwarfTown_room2_Max_X) dwarfTown_room2_Min_X = dwarfTown_room2_Max_X;
			if (dwarfTown_room2_Min_Y > dwarfTown_room2_Max_Y) dwarfTown_room2_Min_Y = dwarfTown_room2_Max_Y;

			makeSliderInScrollbar("room2 frequency", ref dwarfTown_room2_Freq, 1, 50, true, "Room Frequency is the amount of attempts to place a room in the map.");
			makeSliderInScrollbar("room2 retry", ref dwarfTown_room2_Retry, 1, 20, true, "Room Retry is the amount of retry attempts to place a room upon failing to place a room.");
		}

		private void showSettingsCastle()
		{
			makeSliderInScrollbar("room frequency", ref castle_room_Freq, 1, 10, true, "Room Frequency is the amount of attempts to place a room in the map.");
			makeSliderInScrollbar("doors per room", ref castle_doorsPerRoom, 1, 5, true, "Doors per room is the amount of doors every room will have.");
			makeSliderInScrollbar("tower frequency", ref castle_tower_Freq, 1, 100, true, "Tower Frequency is the amount of attempts to place a tower on the castles wall.");
			makeSliderInScrollbar("tower spacing", ref castle_tower_Dist, 1, 55, true, "Tower spacing is the minimum distance between any given towers in cells.");
			makeSliderInScrollbar("tower diameter", ref castle_tower_Diameter, 1, 10, true, "Tower diameter is the size of a tower in cells.");
			makeSliderInScrollbar("inner yard density", ref castle_ward_Chance, 1, 100, true, "The probability to place a round piece of inner yard.");
			makeSliderInScrollbar("inner yard growth", ref castle_ward_growth, 1, 30, true, "The diameter of the pieces of inner yard.");
			makeSliderInScrollbar("castle gates", ref castle_gate_Count, 1, 5, true, "The number of doors in the castles outer wall.");
			
		}

		#endregion

		#region Gui helper functions
		//just because I am lazy. Warning: tightly coupled methods. Do not use somewhere else!

		private Rect getLabelRect()
		{
			return new Rect(relW(scrollLineLeft), relH(line * lineHeight + lineHeight / 3), relW(scrollLeftWidth), relH(lineHeight));
		}

		private Rect getControllRect()
		{
			if (blockedLineStart >= 0 && blockedLineEnd >= 0 && line >= blockedLineStart && line < blockedLineEnd)
			{
				return new Rect(relW(1.0f), relH(line * lineHeight), relW(scrollRightWidth), relH(lineHeight));

			}

			return new Rect(relW(scrollLeftWidth + scrollLineLeft), relH(line * lineHeight), relW(scrollRightWidth), relH(lineHeight));
		}

		//creates a new line with a dropdown menu
		private t makeDropdownInScrollbar<t>(string label, ref t value, ref bool isOpen, bool enabled = true, string tooltip = "")
		{
			string valueString = value.ToString();

			List<string> options = new List<string>(Enum.GetNames(typeof(t)));

			makeDropdownInScrollbar(label, ref valueString, options, ref isOpen, enabled, tooltip);
			value = (t)Enum.Parse(typeof(t), valueString);

			return value;
		}

		//creates a new line with a dropdown menu, where there can be disabled options
		private t makeDropdownInScrollbar<t>(string label, ref t value, List<string> fullList, ref bool isOpen, bool enabled = true, string tooltip = "")
		{
			string valueString = value.ToString();

			List<string> options = new List<string>(Enum.GetNames(typeof(t)));

			makeDropdownInScrollbar(label, ref valueString, options, fullList, ref isOpen, enabled, tooltip);
			value = (t)Enum.Parse(typeof(t), valueString);

			return value;
		}

		//creates a new line with a dropdown menu
		private string makeDropdownInScrollbar(string label, ref string value, List<string> options, ref bool isOpen, bool enabled = true, string tooltip = "")
		{
			if (!options.Contains(value) && options.Count > 0)
			{
				value = options[0];
			}

			Color oldColor = GUI.color;
			if (!enabled)
			{
				Color greyedColor = GUI.color;
				greyedColor.a = greyedOutAlpha;
				GUI.color = greyedColor;
			}

			GUI.Label(getLabelRect(), new GUIContent(label + ": ", tooltip));

			if (!isOpen || !enabled || blockedLineStart >= 0 || blockedLineEnd >= 0)
			{
				bool tmpIsOpen = !GUI.Toggle(getControllRect(), true, new GUIContent(value, tooltip));
				if (enabled) isOpen = tmpIsOpen;
			}
			else
			{
				int oldLine = line;
				blockedLineStart = line;

				foreach (string name in options)
				{
					if (name.Equals(value))
						isOpen = GUI.Toggle(getControllRect(), true, name, "DropdownElement");
					else
						isOpen = !GUI.Toggle(getControllRect(), false, name, "DropdownElement");
					if (!isOpen)
					{
						value = name;
						break;
					}
					line++;
				}


				blockedLineEnd = line;

				line = oldLine;
				//line--;
			}

			GUI.color = oldColor;
			line++;
			return value;
		}

		//creates a new line with a dropdown menu, where there can be disabled options
		private string makeDropdownInScrollbar(string label, ref string value, List<string> options, List<string> fullList, ref bool isOpen, bool enabled = true, string tooltip = "")
		{
			if (!options.Contains(value) && options.Count > 0)
			{
				value = options[0];
			}

			Color oldColor = GUI.color;
			if (!enabled)
			{
				Color greyedColor = GUI.color;
				greyedColor.a = greyedOutAlpha;
				GUI.color = greyedColor;
			}

			GUI.Label(getLabelRect(), new GUIContent(label + ": ", tooltip));

			if (!isOpen || !enabled || blockedLineStart >= 0 || blockedLineEnd >= 0)
			{
				bool tmpIsOpen = !GUI.Toggle(getControllRect(), true, new GUIContent(value, tooltip));
				if (enabled) isOpen = tmpIsOpen;
			}
			else
			{
				int oldLine = line;
				blockedLineStart = line;

				foreach (string name in fullList)
				{
					if (options.Contains(name))
					{
						if (name.Equals(value))
							isOpen = GUI.Toggle(getControllRect(), true, name, "DropdownElement");
						else
							isOpen = !GUI.Toggle(getControllRect(), false, name, "DropdownElement");
						if (!isOpen)
						{
							value = name;
							break;
						}
					}
					else
					{
						Color greyedColor = GUI.color;
						greyedColor.a = greyedOutAlpha;
						GUI.color = greyedColor;
						GUI.Toggle(getControllRect(), false, name, "DropdownElement");
						GUI.color = oldColor;
					}
					line++;
				}


				blockedLineEnd = line;

				line = oldLine;
				//line--;
			}

			GUI.color = oldColor;
			line++;
			return value;
		}
		
		//creates a new line with a toggle
		private bool makeToggleInScrollbar(string label, ref bool value, bool enabled = true, string tooltip = "")
		{
			Color oldColor = GUI.color;
			if (!enabled)
			{
				Color greyedColor = GUI.color;
				greyedColor.a = greyedOutAlpha;
				GUI.color = greyedColor;
			}

			GUI.Label(getLabelRect(), new GUIContent(label + ": ", tooltip));
			bool tmpValue = GUI.Toggle(getControllRect(), value, new GUIContent("", tooltip));
			if (enabled) value = tmpValue;

			GUI.color = oldColor;
			line++;
			return value;
		}

		//creates a new line with a slider
		private int makeSliderInScrollbar(string label, ref int value, int min, int max, bool enabled = true, string tooltip = "")
		{
			Color oldColor = GUI.color;
			if (!enabled)
			{
				Color greyedColor = GUI.color;
				greyedColor.a = greyedOutAlpha;
				GUI.color = greyedColor;
			}

			GUI.Label(getLabelRect(), new GUIContent(label + ": " + value, tooltip));
			GUI.Label(getControllRect(), new GUIContent("", tooltip));//empty label under slider for tooltip
			int tmpValue = Mathf.RoundToInt(GUI.HorizontalSlider(getControllRect(), value, min, max));
			if (enabled) value = tmpValue;

			GUI.color = oldColor;

			line++;
			return value;
		}

		private void makeLabelInHelp(string label)
		{
			Rect rect = new Rect(relW(helpLeft), relH(helpTop + lineHelp * lineHeight), relW(helpWidth), relH(lineHeight));
			GUI.Label(rect, label);
			lineHelp++;
		}

		//returns absolute screen position from relative (x)
		private int relW(float percentage)
		{
			return (int)(Screen.width * percentage);
		}

		//returns absolute screen position from relative (y)
		private int relH(float percentage)
		{
			return (int)(Screen.height * percentage);
		}
		#endregion

		/// <summary>
		/// Return unique Int value for input string
		/// This is not safe in any way. just to convert strings to a more or less unique int
		/// </summary>
		/// <param name="strText"></param>
		/// <returns></returns>
		static int GetIntHashCode(string strText)
		{
			int hashCode = 0;
			if (!string.IsNullOrEmpty(strText))
			{
				//Unicode Encode Covering all characterset
				byte[] byteContents = System.Text.Encoding.Unicode.GetBytes(strText);
				System.Security.Cryptography.MD5 hash =
				new System.Security.Cryptography.MD5CryptoServiceProvider();
				byte[] hashText = hash.ComputeHash(byteContents);
				//32Byte hashText separate
				//hashCodeStart = 0~7  8Byte
				//hashCodeMedium = 8~23  8Byte
				//hashCodeEnd = 24~31  8Byte
				//and Fold
				hashCode = BitConverter.ToInt32(hashText, 0);
			}
			return (hashCode);
		}
	}
}