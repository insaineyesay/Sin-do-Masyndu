using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[CustomEditor(typeof(LevelSettings))]
public class LevelSettingsInspector : Editor {
	private LevelSettings settings;

	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		EditorGUI.indentLevel = 0;
		
		settings = (LevelSettings)target;
		
		if (settings.logoTexture != null) {
			DTInspectorUtility.DrawTexture(settings.logoTexture);
		}
		
		var allStatNames = new List<string>() {
			DTInspectorUtility.DROP_DOWN_NONE_OPTION
		};
		
		Transform statsHolder = null;
		
		statsHolder = settings.transform.Find(LevelSettings.PLAYER_STATS_CONTAINER_TRANS_NAME);
		for (var i = 0; i < statsHolder.childCount; i++) {
			var child = statsHolder.GetChild(i);
			allStatNames.Add(child.name);
		}
		
		var playerStatsHolder = settings.transform.FindChild(LevelSettings.PLAYER_STATS_CONTAINER_TRANS_NAME);
		if (playerStatsHolder == null) {
			Debug.LogError("You have no child prefab of LevelSettings called '" + LevelSettings.PLAYER_STATS_CONTAINER_TRANS_NAME + "'. Please revert your LevelSettings prefab.");
			DTInspectorUtility.ShowColorWarning("Please check the console. You have a breaking error.");
			return;
		}
		var statCount = playerStatsHolder.childCount;

		var newDisableSyncro = EditorGUILayout.Toggle("Syncro Spawners Off", settings.disableSyncroSpawners);
		if (newDisableSyncro != settings.disableSyncroSpawners) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Syncro Spawners Off");
			settings.disableSyncroSpawners = newDisableSyncro;
		}

		EditorGUI.indentLevel = 0;

		var newUseMusic = EditorGUILayout.Toggle("Use Music Settings", settings.useMusicSettings);
		if (newUseMusic != settings.useMusicSettings) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Use Music Settings");
			settings.useMusicSettings = newUseMusic;
		}
		
		var newUseWaves = EditorGUILayout.BeginToggleGroup("Use Global Waves", settings.useWaves);
		if (newUseWaves != settings.useWaves) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Use Global Waves");
			settings.useWaves = newUseWaves;
		}
		
		EditorGUI.indentLevel = 1;
		
		var newEnableWarp = EditorGUILayout.Toggle("Custom Start Wave?", settings.enableWaveWarp);
		if (newEnableWarp != settings.enableWaveWarp) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Custom Start Wave?");
			settings.enableWaveWarp = newEnableWarp;
		}

		if (settings.enableWaveWarp) {
			EditorGUI.indentLevel = 1;

			if (settings.warpLevelNumber < 0 || settings.warpWaveNumber < 0)  {
				DTInspectorUtility.ShowColorWarning("*Custom Start Wave settings are blank. They will not be used.");
			}

			var newWarpLevel = EditorGUILayout.IntPopup("Start Level#", settings.warpLevelNumber + 1, LevelNames, LevelIndexes) - 1;
			if (newWarpLevel != settings.warpLevelNumber) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "change Start Level#");
				settings.warpLevelNumber = newWarpLevel;
				settings.warpWaveNumber = 0;
			}

			var newWarpWave = EditorGUILayout.IntPopup("Start Wave#", settings.warpWaveNumber + 1, 
				WaveNamesForLevel(settings.warpLevelNumber), WaveIndexesForLevel(settings.warpLevelNumber)) - 1;
			if (newWarpWave != settings.warpWaveNumber) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "change Start Wave#");
				settings.warpWaveNumber = newWarpWave;
			}
		}
		EditorGUILayout.EndToggleGroup();
		
		EditorGUI.indentLevel = 0;
		var newLogging = EditorGUILayout.Toggle("Log Messages", settings.isLoggingOn);
		if (newLogging != settings.isLoggingOn) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Log Messages");
			settings.isLoggingOn = newLogging;
		}
		
		var hadNoListener = settings.listener == null;
		var newListener = (LevelSettingsListener) EditorGUILayout.ObjectField("Listener", settings.listener, typeof(LevelSettingsListener), true);
		if (newListener != settings.listener) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "assign Listener");
			settings.listener = newListener;
			if (hadNoListener && settings.listener != null) {
				settings.listener.sourceTransName = settings.transform.name;
			}
		}

		EditorGUILayout.Separator();
		
		if (settings.useMusicSettings) {
			var newGoMusic = (LevelSettings.WaveMusicMode) EditorGUILayout.EnumPopup("G.O. Music Mode", settings.gameOverMusicSettings.WaveMusicMode);
			if (newGoMusic != settings.gameOverMusicSettings.WaveMusicMode) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "change G.O. Music Mode");
				settings.gameOverMusicSettings.WaveMusicMode = newGoMusic;
			}
			if (settings.gameOverMusicSettings.WaveMusicMode == LevelSettings.WaveMusicMode.PlayNew) {
				var newWaveMusic = (AudioClip) EditorGUILayout.ObjectField("G.O. Music", settings.gameOverMusicSettings.WaveMusic, typeof(AudioClip), true);
				if (newWaveMusic != settings.gameOverMusicSettings.WaveMusic) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "assign G.O. Music");
					settings.gameOverMusicSettings.WaveMusic = newWaveMusic;
				}
			}
			if (settings.gameOverMusicSettings.WaveMusicMode != LevelSettings.WaveMusicMode.Silence) {
				var newMusicVol = EditorGUILayout.Slider("G.O. Music Volume", settings.gameOverMusicSettings.WaveMusicVolume, 0f, 1f);
				if (newMusicVol != settings.gameOverMusicSettings.WaveMusicVolume) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "change G.O. Music Volume");
					settings.gameOverMusicSettings.WaveMusicVolume = newMusicVol;
				}
			} else {
				var newFadeTime = EditorGUILayout.Slider("Silence Fade Time", settings.gameOverMusicSettings.FadeTime, 0f, 15f);
				if (newFadeTime != settings.gameOverMusicSettings.FadeTime) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "change Silence Fade Time");
					settings.gameOverMusicSettings.FadeTime = newFadeTime;
				}
			}
		}
		
		EditorGUILayout.Separator();
		
		bool isDirty = false;
		
		// create spawners section
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		EditorGUI.indentLevel = 0;

		var newExpanded = DTInspectorUtility.Foldout(settings.createSpawnersExpanded, "Spawner Creation");
		if (newExpanded != settings.createSpawnersExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "toggle expand Spawner Creation");
			settings.createSpawnersExpanded = newExpanded;
		}
		
		if (settings.createSpawnersExpanded) {
	        // BUTTONS...
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
			EditorGUI.indentLevel = 0;
	
	        // Add expand/collapse buttons if there are items in the list
	
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
	        // A little space between button groups
	        GUILayout.Space(6);
			
			EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
			// end create spawners section
			
			EditorGUILayout.Separator();
	
			var newName = EditorGUILayout.TextField("New Spawner Name", settings.newSpawnerName);
			if (newName != settings.newSpawnerName) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "change New Spawner Name");
				settings.newSpawnerName = newName;
			}

			var newType = (LevelSettings.SpawnerType) EditorGUILayout.EnumPopup("Spawner Color", settings.newSpawnerType);
			if (newType != settings.newSpawnerType) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "change Spawner Color");
				settings.newSpawnerType = newType;
			}
	        
			//Debug.Log(settings.newSpawnerType);
			
			EditorGUILayout.BeginHorizontal(EditorStyles.boldLabel);
			EditorGUILayout.LabelField("", EditorStyles.miniLabel);
			if (GUILayout.Button("Create Spawner", EditorStyles.miniButton, GUILayout.MaxWidth(110))) {
				CreateSpawner();		
			}
			GUILayout.FlexibleSpace();
		}

		EditorGUILayout.EndHorizontal();
		
		
		EditorGUILayout.Separator();
		// create Prefab Pools section
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		EditorGUI.indentLevel = 0;
		newExpanded = DTInspectorUtility.Foldout(settings.createPrefabPoolsExpanded, "Prefab Pool Creation");
		if (newExpanded != settings.createPrefabPoolsExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "toggle expand Prefab Pool Creation");
			settings.createPrefabPoolsExpanded = newExpanded;
		}
		
		if (settings.createPrefabPoolsExpanded) {
	        // BUTTONS...
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
			EditorGUI.indentLevel = 0;
	
	        // Add expand/collapse buttons if there are items in the list
	
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
	        // A little space between button groups
	        GUILayout.Space(6);
			
			EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Separator();

			var newPoolName = EditorGUILayout.TextField("New Pool Name", settings.newPrefabPoolName);
			if (newPoolName != settings.newPrefabPoolName) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "change New Pool Name");
				settings.newPrefabPoolName = newPoolName;
			}
	        
			EditorGUILayout.BeginHorizontal(EditorStyles.boldLabel);
			EditorGUILayout.LabelField("", EditorStyles.miniLabel);
			if (GUILayout.Button("Create Prefab Pool", EditorStyles.miniButton, GUILayout.MaxWidth(110))) {
				CreatePrefabPool();		
			}
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		// end create prefab pools section
		
		
		EditorGUILayout.Separator();
		// Player stats
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		EditorGUI.indentLevel = 0;
		newExpanded = DTInspectorUtility.Foldout(settings.gameStatsExpanded, "World Variables");
		if (newExpanded != settings.gameStatsExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "toggle expand World Variables");
			settings.gameStatsExpanded = newExpanded;
		}
		
		if (settings.gameStatsExpanded) {
	        // BUTTONS...
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
			EditorGUI.indentLevel = 0;
	
	        // Add expand/collapse buttons if there are items in the list
	
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
	        // A little space between button groups
	        GUILayout.Space(6);
			
			EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
			
			DTInspectorUtility.ShowColorWarning("You have " + statCount + " World Variable(s) set up. Time to create more?");
			EditorGUILayout.BeginHorizontal(EditorStyles.boldLabel);
			EditorGUILayout.LabelField("", EditorStyles.miniLabel);
			if (GUILayout.Button("Edit World Variables", EditorStyles.miniButton, GUILayout.MaxWidth(130))) {
				Selection.objects = new Object[] {
					playerStatsHolder.gameObject
				};
				return;
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		} else {
	        EditorGUILayout.EndHorizontal();
		}
		// end Player  stats

		EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        EditorGUI.indentLevel = 0;  // Space will handle this for the header
		
		if (settings.useWaves) {
	        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			newExpanded = DTInspectorUtility.Foldout(settings.levelsAreExpanded, "Level Wave Settings");
			if (newExpanded != settings.levelsAreExpanded) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "toggle expand Level Wave Settings");
				settings.levelsAreExpanded = newExpanded;
			}
	
	        // BUTTONS...
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
	
	        // Add expand/collapse buttons if there are items in the list
			if (settings.LevelTimes.Count > 0) {
	            GUIContent content;
	            var collapseIcon = '\u2261'.ToString();
	            content = new GUIContent(collapseIcon, "Click to collapse all");
	            var masterCollapse = GUILayout.Button(content, EditorStyles.toolbarButton);
	
	            var expandIcon = '\u25A1'.ToString();
	            content = new GUIContent(expandIcon, "Click to expand all");
	            var masterExpand = GUILayout.Button(content, EditorStyles.toolbarButton);
				if (masterExpand) {
					ExpandCollapseAll(true);
				} 
				if (masterCollapse) {
					ExpandCollapseAll(false);
				}
	        } else {
	         	GUILayout.FlexibleSpace();
	        }
	
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(50));
			
			var addText = string.Format("Click to add level{0}.", settings.LevelTimes.Count > 0 ? " at the end" : "");
			
	        // Main Add button
	        if (GUILayout.Button(new GUIContent("+", addText), EditorStyles.toolbarButton)) {
				isDirty = true;
				CreateNewLevelAfter();
			}
	
			EditorGUILayout.EndHorizontal();
	
	        EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
	
			DTInspectorUtility.FunctionButtons levelButtonPressed = DTInspectorUtility.FunctionButtons.None;
			DTInspectorUtility.FunctionButtons waveButtonPressed = DTInspectorUtility.FunctionButtons.None;
			
			if (settings.levelsAreExpanded) { 
				EditorGUI.indentLevel = 0;
				
				int levelToDelete = -1;
				int levelToInsertAt = -1;
				int waveToInsertAt = -1;
				int waveToDelete = -1;
				
				LevelSpecifics levelSetting = null;
				for (var l = 0; l < settings.LevelTimes.Count; l++) {
					EditorGUI.indentLevel = 1;
					levelSetting = settings.LevelTimes[l];
	
		            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		            // Display foldout with current state
					newExpanded = DTInspectorUtility.Foldout(levelSetting.isExpanded, 
					  string.Format("Level {0} waves", (l + 1)));
					if (newExpanded != levelSetting.isExpanded) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "toggle expand Level Waves");
						levelSetting.isExpanded = newExpanded;
					}
		            levelButtonPressed = DTInspectorUtility.AddFoldOutListItemButtons(l, settings.LevelTimes.Count, "level", true);
		            EditorGUILayout.EndHorizontal();
					
					if (levelSetting.isExpanded) {
						for (var w = 0; w < levelSetting.WaveSettings.Count; w++) {
							var waveSetting = levelSetting.WaveSettings[w];
	
							EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
							EditorGUI.indentLevel = 2;
				            // Display foldout with current state
							var innerExpanded = DTInspectorUtility.Foldout(waveSetting.isExpanded, "Wave " + (w + 1));
							if (innerExpanded != waveSetting.isExpanded) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "toggle expand Wave");
								waveSetting.isExpanded = innerExpanded;
							}
				            waveButtonPressed = DTInspectorUtility.AddFoldOutListItemButtons(w, levelSetting.WaveSettings.Count, "wave", true);
				            EditorGUILayout.EndHorizontal();
							
							if (waveSetting.isExpanded) {
								EditorGUI.indentLevel = 0;
								if (waveSetting.skipWaveType == LevelSettings.SkipWaveMode.Always) {
									DTInspectorUtility.ShowColorWarning("*This wave is set to be skipped.");
								}
								
								if (string.IsNullOrEmpty(waveSetting.waveName)) {
									waveSetting.waveName = "UNNAMED";
								}
	
								var newWaveName = EditorGUILayout.TextField("Wave Name", waveSetting.waveName);
								if (newWaveName != waveSetting.waveName) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Wave Name");
									waveSetting.waveName = newWaveName;
								}
	
								var newWaveType = (LevelSettings.WaveType) EditorGUILayout.EnumPopup("Wave Type", waveSetting.waveType);
								if (newWaveType != waveSetting.waveType) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Wave Type");
									waveSetting.waveType = newWaveType;
								}
								
								if (waveSetting.waveType == LevelSettings.WaveType.Timed) {
									var newDuration = EditorGUILayout.IntSlider("Duration (sec)", waveSetting.WaveDuration, 1, 200);
									if (newDuration != waveSetting.WaveDuration) {
										UndoHelper.RecordObjectPropertyForUndo(settings, "change Duration");
										waveSetting.WaveDuration = newDuration;
									}
								}
								
								switch (waveSetting.skipWaveType) {
									case LevelSettings.SkipWaveMode.IfWorldVariableValueAbove:
									case LevelSettings.SkipWaveMode.IfWorldVariableValueBelow:
										EditorGUILayout.Separator();
										break;
								}
	
								var newSkipType = (LevelSettings.SkipWaveMode) EditorGUILayout.EnumPopup("Skip Wave Type", waveSetting.skipWaveType);
								if (newSkipType != waveSetting.skipWaveType) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Skip Wave Type");
									waveSetting.skipWaveType = newSkipType;
								}
								
								switch (waveSetting.skipWaveType) {
									case LevelSettings.SkipWaveMode.IfWorldVariableValueAbove:
									case LevelSettings.SkipWaveMode.IfWorldVariableValueBelow:
										var missingStatNames = new List<string>();
										missingStatNames.AddRange(allStatNames);
										missingStatNames.RemoveAll(delegate(string obj) {
											return waveSetting.skipWavePassCriteria.HasKey(obj);
										});
										
										var newStat = EditorGUILayout.Popup("Add Skip Wave Limit", 0, missingStatNames.ToArray());
										if (newStat != 0) {
											AddWaveSkipLimit(missingStatNames[newStat], waveSetting);
										}
		
										if (waveSetting.skipWavePassCriteria.statMods.Count == 0) {
											DTInspectorUtility.ShowColorWarning("*You have no Skip Wave Limits. Wave will never be skipped.");
										} else {
											EditorGUILayout.Separator();
											
											int? indexToDelete = null;
											
											for (var i = 0; i < waveSetting.skipWavePassCriteria.statMods.Count; i++) {
												var modifier = waveSetting.skipWavePassCriteria.statMods[i];
												EditorGUILayout.BeginHorizontal();
												GUILayout.Space(35);
												GUILayout.Label(modifier.statName, GUILayout.MaxWidth(120));
							 
												GUILayout.Space(19);
												GUILayout.Label(waveSetting.skipWaveType == LevelSettings.SkipWaveMode.IfWorldVariableValueAbove ? "Min" : "Max");
												var newModVal = EditorGUILayout.IntField(modifier.modValue, GUILayout.MaxWidth(70));
												if (newModVal != modifier.modValue) {
													UndoHelper.RecordObjectPropertyForUndo(settings, "change Skip Wave Limit Min");
													modifier.modValue = newModVal;
												}
												GUI.backgroundColor = Color.green;
												if (GUILayout.Button(new GUIContent("Delete", "Remove this mod"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(64))) {
													indexToDelete = i;
												}
												GUI.backgroundColor = Color.white;
												GUILayout.FlexibleSpace();
												EditorGUILayout.EndHorizontal();
											}
											
											DTInspectorUtility.ShowColorWarning("  *Limits are inclusive: i.e. 'Above' means >=");
											if (indexToDelete.HasValue) {
												UndoHelper.RecordObjectPropertyForUndo(settings, "remove Skip Wave Limit");
												waveSetting.skipWavePassCriteria.DeleteByIndex(indexToDelete.Value);
												isDirty = true;
											}
											
											EditorGUILayout.Separator();
										}
									
										break;
								}
								
								if (settings.useMusicSettings) {
									if (l > 0 || w > 0) {
										var newMusicMode = (LevelSettings.WaveMusicMode) EditorGUILayout.EnumPopup("Music Mode", waveSetting.musicSettings.WaveMusicMode);
										if (newMusicMode != waveSetting.musicSettings.WaveMusicMode) {
											UndoHelper.RecordObjectPropertyForUndo(settings, "change Music Mode");
											waveSetting.musicSettings.WaveMusicMode = newMusicMode;
										}
									}
									
									if (waveSetting.musicSettings.WaveMusicMode == LevelSettings.WaveMusicMode.PlayNew) {
										var newWavMusic = (AudioClip) EditorGUILayout.ObjectField( "Music", waveSetting.musicSettings.WaveMusic, typeof(AudioClip), true);
										if (newWavMusic != waveSetting.musicSettings.WaveMusic) {
											UndoHelper.RecordObjectPropertyForUndo(settings, "change Wave Music");
											waveSetting.musicSettings.WaveMusic = newWavMusic;
										}
									}
									if (waveSetting.musicSettings.WaveMusicMode != LevelSettings.WaveMusicMode.Silence) {
										var newVol = EditorGUILayout.Slider("Music Volume", waveSetting.musicSettings.WaveMusicVolume, 0f, 1f);
										if (newVol != waveSetting.musicSettings.WaveMusicVolume) {
											UndoHelper.RecordObjectPropertyForUndo(settings, "change Music Volume");
											waveSetting.musicSettings.WaveMusicVolume = newVol;
										}
									} else {
										var newFadeTime = EditorGUILayout.Slider("Silence Fade Time", waveSetting.musicSettings.FadeTime, 0f, 15f);
										if (newFadeTime != waveSetting.musicSettings.FadeTime) {
											UndoHelper.RecordObjectPropertyForUndo(settings, "change Silence Fade Time");
											waveSetting.musicSettings.FadeTime = newFadeTime;
										}
									}
								}
								
								// beat level variable modifiers
								var newBonusesEnabled = EditorGUILayout.BeginToggleGroup("Wave Completion Bonus", waveSetting.waveBeatBonusesEnabled);
								if (newBonusesEnabled != waveSetting.waveBeatBonusesEnabled) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Wave Completion Bonus");
									waveSetting.waveBeatBonusesEnabled = newBonusesEnabled;
								}
								
								var missingBonusStatNames = new List<string>();
								missingBonusStatNames.AddRange(allStatNames);
								missingBonusStatNames.RemoveAll(delegate(string obj) {
									return waveSetting.waveDefeatVariableModifiers.HasKey(obj);
								});
								
								var newBonusStat = EditorGUILayout.Popup("Add Variable Modifer", 0, missingBonusStatNames.ToArray());
								if (newBonusStat != 0) {
									AddBonusStatModifier(missingBonusStatNames[newBonusStat], waveSetting);
								}
								
								if (waveSetting.waveDefeatVariableModifiers.statMods.Count == 0) {
									DTInspectorUtility.ShowColorWarning("*You currently are using no modifiers for this wave.");
								} else {
									EditorGUILayout.Separator();
									
									int? indexToDelete = null;
									
									for (var i = 0; i < waveSetting.waveDefeatVariableModifiers.statMods.Count; i++) {
										var modifier = waveSetting.waveDefeatVariableModifiers.statMods[i];
										EditorGUILayout.BeginHorizontal();
										GUILayout.Space(13);
										GUILayout.Label(modifier.statName, GUILayout.MinWidth(100));
					 
										GUILayout.Space(19);
										var newModVal = EditorGUILayout.IntField(modifier.modValue, GUILayout.MaxWidth(70));
										if (newModVal != modifier.modValue) {
											UndoHelper.RecordObjectPropertyForUndo(settings, "change Variable Modifier");
											modifier.modValue = newModVal;
										}
										GUI.backgroundColor = Color.green;
										if (GUILayout.Button(new GUIContent("Delete", "Remove this mod"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(64))) {
											indexToDelete = i;
										}
										GUI.backgroundColor = Color.white;
										GUILayout.FlexibleSpace();
										EditorGUILayout.EndHorizontal();
									}
									
									if (indexToDelete.HasValue) {
										UndoHelper.RecordObjectPropertyForUndo(settings, "delete Variable Modifier");
										waveSetting.waveDefeatVariableModifiers.DeleteByIndex(indexToDelete.Value);
									}
									
									EditorGUILayout.Separator();
								}
								EditorGUILayout.EndToggleGroup();
								
								EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(50));
								
								var spawnersUsed = FindMatchingSpawners(settings, l, w, null);
								
								if (spawnersUsed == 0) {
									GUI.contentColor = Color.yellow;
								}
								EditorGUILayout.LabelField("Spawners Used: " + spawnersUsed);
								GUI.contentColor = Color.white;
								if (GUILayout.Button("List", GUILayout.MinWidth(55))) {
									FindMatchingSpawners(settings, l, w, false);
								}
								if (GUILayout.Button("Select", GUILayout.MinWidth(55))) {
									FindMatchingSpawners(settings, l, w, true);
								}
								EditorGUILayout.EndHorizontal();
							}
							
							switch (waveButtonPressed) {
								case DTInspectorUtility.FunctionButtons.Remove:
									if (levelSetting.WaveSettings.Count <= 1) {
										DTInspectorUtility.ShowAlert("You cannot delete the only Wave in a Level. Delete the Level if you like.");
									} else {
										waveToDelete = w;
									}	
	
									isDirty = true;
									break;
								case DTInspectorUtility.FunctionButtons.Add:
									waveToInsertAt = w;
									isDirty = true;
									break;
							}
						}
						
						if (waveToDelete >= 0) {
							if (DTInspectorUtility.ConfirmDialog("Delete wave? This cannot be undone.")) {
								DeleteWave(levelSetting, waveToDelete, l);
								isDirty = true;
							}
						}
						if (waveToInsertAt > -1) {
							InsertWaveAfter(levelSetting, waveToInsertAt, l);
							isDirty = true;
						}
					} 
					
					switch (levelButtonPressed) {
						case DTInspectorUtility.FunctionButtons.Remove:
							if (DTInspectorUtility.ConfirmDialog("Delete level? This cannot be undone.")) {
								levelToDelete = l;
								isDirty = true;
							}
							break;
						case DTInspectorUtility.FunctionButtons.Add:
							isDirty = true;
							levelToInsertAt = l;
							break;
					}
				}
				
				if (levelToDelete > -1) {
					DeleteLevel(levelToDelete);
				}
				
				if (levelToInsertAt > -1) {
					CreateNewLevelAfter(levelToInsertAt); 
				}
			}
		} else {
	        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			EditorGUILayout.LabelField(" Level Wave Settings (DISABLED)");
			EditorGUILayout.EndHorizontal();			
		}

		if (GUI.changed || isDirty) {
			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}

		this.Repaint();

		//DrawDefaultInspector();
    }
	
	private void ExpandCollapseAll(bool isExpand) {
		UndoHelper.RecordObjectPropertyForUndo(settings, "toggle expand / collapse all Level Wave Settings");

		foreach (var level in settings.LevelTimes) {
			level.isExpanded = isExpand;
			foreach (var wave in level.WaveSettings) {
				wave.isExpanded = isExpand;
			}
		}
	}
	
	private void CreateSpawner() {
		string name = settings.newSpawnerName;
		
		if (string.IsNullOrEmpty(name)) {
			DTInspectorUtility.ShowAlert("You must enter a name for your new Spawner.");
			return;
		}
		
		Transform spawnerTrans = null;
		
		switch (settings.newSpawnerType) {
			case LevelSettings.SpawnerType.Green:
				spawnerTrans = settings.GreenSpawnerTrans;
				break;
			case LevelSettings.SpawnerType.Red:
				spawnerTrans = settings.RedSpawnerTrans;
				break;
		}
		
		var spawnPos = settings.transform.position;
		spawnPos.x += Random.Range(-10, 10);
		spawnPos.z += Random.Range(-10, 10);

		var newSpawner = GameObject.Instantiate(spawnerTrans.gameObject, spawnPos, Quaternion.identity) as GameObject;
		UndoHelper.CreateObjectForUndo(newSpawner.gameObject, "create Spawner");
		newSpawner.name = name;
		
		var spawnersHolder = settings.transform.FindChild(LevelSettings.SPAWNER_CONTAINER_TRANS_NAME);
		if (spawnersHolder == null) {
			DTInspectorUtility.ShowAlert(LevelSettings.NO_SPAWN_CONTAINER_ALERT);
			
			GameObject.DestroyImmediate(newSpawner);
			
			return;
		}
		
		newSpawner.transform.parent = spawnersHolder.transform;
	}
	
	private void CreatePrefabPool() {
		string name = settings.newPrefabPoolName;
		
		if (string.IsNullOrEmpty(name)) {
			DTInspectorUtility.ShowAlert("You must enter a name for your new Prefab Pool.");
			return;
		}
		
		var spawnPos = settings.transform.position;
		
		var newPool = GameObject.Instantiate(settings.PrefabPoolTrans.gameObject, spawnPos, Quaternion.identity) as GameObject;
		newPool.name = name;
		
		var poolsHolder = settings.transform.FindChild(LevelSettings.PREFAB_POOLS_CONTAINER_TRANS_NAME);
		if (poolsHolder == null) {
			DTInspectorUtility.ShowAlert(LevelSettings.NO_PREFAB_POOLS_CONTAINER_ALERT);
			
			GameObject.DestroyImmediate(newPool);
			return;
		}
		
		var dupe = poolsHolder.FindChild(name);
		if (dupe != null) {
			DTInspectorUtility.ShowAlert("You already have a Prefab Pool named '" + name + "', please choose another name.");
			
			GameObject.DestroyImmediate(newPool);
			return;
		}
		
		UndoHelper.CreateObjectForUndo(newPool.gameObject, "create Prefab Pool");
		newPool.transform.parent = poolsHolder.transform;
	}
	
	private void InsertWaveAfter(LevelSpecifics spec, int waveToInsertAt, int level) {
		var spawners = settings.GetAllSpawners();
			
		var newWave = new LevelWave();

		waveToInsertAt++;
		spec.WaveSettings.Insert(waveToInsertAt, newWave);

		WaveSyncroPrefabSpawner spawnerScript = null;
	
		if (settings.enableWaveWarp) {
			if (settings.warpLevelNumber == level && settings.warpWaveNumber >= waveToInsertAt) {
				settings.warpWaveNumber++;
			}
		}
		
		foreach (var spawner in spawners) {
			spawnerScript = spawner.GetComponent<WaveSyncroPrefabSpawner>();
			spawnerScript.InsertWave(waveToInsertAt, level);
		}		
	}
	
	private void DeleteLevel(int levelToDelete) {
		List<Transform> spawners = settings.GetAllSpawners();

		if (settings.enableWaveWarp) {
			if (levelToDelete < settings.warpLevelNumber) {
				settings.warpLevelNumber--;
			} else if (levelToDelete == settings.warpLevelNumber) {
				DTInspectorUtility.ShowAlert("Your Custom Start Wave settings have been deleted. Please reset them before continuing.");
				settings.warpLevelNumber = -1;
				settings.warpWaveNumber = -1;
			}
		}
		
		settings.LevelTimes.RemoveAt(levelToDelete);
		
		WaveSyncroPrefabSpawner spawnerScript = null;
	
		foreach (var spawner in spawners) {
			spawnerScript = spawner.GetComponent<WaveSyncroPrefabSpawner>();
			spawnerScript.DeleteLevel(levelToDelete);
		}		
	}
	
	private void CreateNewLevelAfter(int? index = null) {
		List<Transform> spawners = settings.GetAllSpawners();
		
		var newLevel = new LevelSpecifics();
		var newWave = new LevelWave();
		newLevel.WaveSettings.Add(newWave);
		
		int newLevelIndex = 0;
		
		if (index == null) {
			newLevelIndex = settings.LevelTimes.Count;
		} else {
			newLevelIndex = index.Value + 1;
		}

		if (settings.enableWaveWarp) {
			if (newLevelIndex <= settings.warpLevelNumber) {
				settings.warpLevelNumber++;
			}
		}

		UndoHelper.RecordObjectPropertyForUndo(settings, "Add Level");

		settings.LevelTimes.Insert(newLevelIndex, newLevel);
		
		WaveSyncroPrefabSpawner spawnerScript = null;
	
		foreach (var spawner in spawners) {
			spawnerScript = spawner.GetComponent<WaveSyncroPrefabSpawner>();
			spawnerScript.InsertLevel(newLevelIndex);
		}		
	}

	private void DeleteWave(LevelSpecifics spec, int waveToDelete, int levelNumber) {
		var spawners = settings.GetAllSpawners();
		var affectedObjects = new List<Object>();
		affectedObjects.Add(settings);

		var spawnerScripts = new List<WaveSyncroPrefabSpawner>();
		foreach (var s in spawners) {
			spawnerScripts.Add(s.GetComponent<WaveSyncroPrefabSpawner>());
			affectedObjects.Add(s);
		}

		spec.WaveSettings.RemoveAt(waveToDelete);

		if (settings.enableWaveWarp) {
			if (settings.warpLevelNumber == levelNumber && settings.warpWaveNumber >= waveToDelete) {
				settings.warpWaveNumber--;
			}
		}
		
		foreach (var script in spawnerScripts) {
			script.DeleteWave(levelNumber, waveToDelete);
		}
	}

	private void AddWaveSkipLimit(string modifierName, LevelWave spec) {
		if (spec.skipWavePassCriteria.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This wave already has a Skip Wave Limit for World Variable: " + modifierName + ". Please modify the existing one instead.");
			return;
		}
	
		UndoHelper.RecordObjectPropertyForUndo(settings, "add Skip Wave Limit");

		spec.skipWavePassCriteria.statMods.Add(new WorldVariableModifier() {
			statName = modifierName
		});
	}
	
	private int FindMatchingSpawners(LevelSettings levSettings, int level, int wave, bool? selectThem) {
		var spawners = levSettings.GetAllSpawners();
		WaveSyncroPrefabSpawner spawnerScript = null;
		
		var matchSpawners = new List<GameObject>();
		
		StringBuilder sb = new StringBuilder();

		var spawnersUsed = 0;
		
		foreach (var spawner in spawners) {
			spawnerScript = spawner.GetComponent<WaveSyncroPrefabSpawner>();
			var matchingWave = spawnerScript.FindWave(level, wave);
			if (matchingWave == null) {
				continue;
			}
			
			spawnersUsed++;
			matchSpawners.Add(spawner.gameObject);
			if (sb.Length > 0) {
				sb.Append(", ");
			}
			sb.Append("'" + spawnerScript.name + "'");
		}
		
		if (sb.Length == 0) {
			sb.Append("~None~");
		}
		
		if (selectThem.HasValue) {
			Debug.Log(string.Format("--- Found {0} matching spawner(s) for level# {1}, wave# {2}: ({3}) ---",
				matchSpawners.Count,
				level + 1, 
				wave + 1,
				sb.ToString()));
			
			if (selectThem.Value) {
				if (matchSpawners.Count > 0) {
					Selection.objects = matchSpawners.ToArray();
				} else {
					Debug.Log("No spawners use this wave.");
				}
			}
		}
		
		return spawnersUsed;
	}

	private LevelSettings GetLevelSettings {
		get {
			return settings;
		}
	}
	
	private string[] LevelNames {
		get {
			var names = new string[GetLevelSettings.LevelTimes.Count];
			for (var i = 0; i < GetLevelSettings.LevelTimes.Count; i++) {
				names[i] = (i + 1).ToString();
			}
			
			return names;
		}
	}

	private int[] LevelIndexes {
		get {
			var indexes = new int[GetLevelSettings.LevelTimes.Count];
			
			for (var i = 0; i < GetLevelSettings.LevelTimes.Count; i++) {
				indexes[i] = i + 1;
			}
			
			return indexes;
		}
	}
	
	private string[] WaveNamesForLevel(int levelNumber) {
		if (GetLevelSettings.LevelTimes.Count <= levelNumber || GetLevelSettings.LevelTimes.Count < 1 || levelNumber < 0) {
			return new string[0];
		}
		
		var level = GetLevelSettings.LevelTimes[levelNumber];
		var names = new string[level.WaveSettings.Count];
		
		for (var i = 0; i < level.WaveSettings.Count; i++) {
			names[i] = (i + 1).ToString();
		}
		
		return names;
	}

	private int[] WaveIndexesForLevel(int levelNumber) {
		if (GetLevelSettings.LevelTimes.Count <= levelNumber || GetLevelSettings.LevelTimes.Count < 1 || levelNumber < 0) {
			return new int[0];
		}

		var level = GetLevelSettings.LevelTimes[levelNumber];
		var indexes = new int[level.WaveSettings.Count];
		
		for (var i = 0; i < level.WaveSettings.Count; i++) {
			indexes[i] = i + 1;
		}
		
		return indexes;
	}
	
	private void AddBonusStatModifier(string modifierName, LevelWave waveSpec) {
		if (waveSpec.waveDefeatVariableModifiers.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This Wave already has a modifier for World Variable: " + modifierName + ". Please modify that instead.");
			return;
		}

		UndoHelper.RecordObjectPropertyForUndo(settings, "add Wave Completion Bonus modifier");

		waveSpec.waveDefeatVariableModifiers.statMods.Add(new WorldVariableModifier() {
			statName = modifierName
		});
	}
}
