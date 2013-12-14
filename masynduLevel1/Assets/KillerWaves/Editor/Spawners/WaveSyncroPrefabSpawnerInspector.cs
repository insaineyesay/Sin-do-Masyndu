using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
	[CustomEditor(typeof(WaveSyncroPrefabSpawner))]
#else
	[CustomEditor(typeof(WaveSyncroPrefabSpawner), true)]
#endif
public class WaveSyncroPrefabSpawnerInspector : Editor {
	private LevelSettings levSettings;
	private WaveSyncroPrefabSpawner settings;

	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		
        settings = (WaveSyncroPrefabSpawner)target;

        var isDirty = false;
		
        var myParent = settings.transform.parent;
        Transform levelSettingObj = null;
        LevelSettings levelSettings = null;

        if (myParent != null)
        {
            levelSettingObj = myParent.parent;
            if (levelSettingObj != null)
            {
                levelSettings = levelSettingObj.GetComponent<LevelSettings>();
            }
        }

        if (myParent == null || levelSettingObj == null || levelSettings == null)
        {
            //Debug.LogError(string.Format("Spawner: '{0}' is not a child of LevelSettings. Aborting.", this.name));
            DrawDefaultInspector();
            return;
        }

        var allStatNames = new List<string>() {
			DTInspectorUtility.DROP_DOWN_NONE_OPTION
		};

        Transform statsHolder = null;

        if (levelSettings != null)
        {
            statsHolder = levelSettings.transform.Find(LevelSettings.PLAYER_STATS_CONTAINER_TRANS_NAME);
            for (var i = 0; i < statsHolder.childCount; i++)
            {
                var child = statsHolder.GetChild(i);
                allStatNames.Add(child.name);
            }
        }

        if (settings.logoTexture != null)
        {
            DTInspectorUtility.DrawTexture(settings.logoTexture);
        }

        EditorGUILayout.Separator();
        EditorGUI.indentLevel = 0;

		var newActive = (LevelSettings.ActiveItemMode) EditorGUILayout.EnumPopup("Active Mode", settings.activeMode);
		if (newActive != settings.activeMode) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "change Active Mode");
			settings.activeMode = newActive;
		}
		
		switch (settings.activeMode) {
			case LevelSettings.ActiveItemMode.IfWorldVariableInRange:
			case LevelSettings.ActiveItemMode.IfWorldVariableOutsideRange:
				var missingStatNames = new List<string>();
				missingStatNames.AddRange(allStatNames);
				missingStatNames.RemoveAll(delegate(string obj) {
					return settings.activeItemCriteria.HasKey(obj);
				});
				
				var newStat = EditorGUILayout.Popup("Add Active Limit", 0, missingStatNames.ToArray());
				if (newStat != 0) {
					AddActiveLimit(missingStatNames[newStat]);
				}

				if (settings.activeItemCriteria.statMods.Count == 0) {
					DTInspectorUtility.ShowColorWarning("*You have no Active Limits. Spawner will never be Active.");
				} else {
					EditorGUILayout.Separator();
					
					int? indexToDelete = null;
					
					for (var j = 0; j < settings.activeItemCriteria.statMods.Count; j++) {
						var modifier = settings.activeItemCriteria.statMods[j];
						EditorGUILayout.BeginHorizontal();
						GUILayout.Space(15);
						var statName = modifier.statName;	
						GUILayout.Label(statName);
					
						GUILayout.FlexibleSpace();
						GUILayout.Label("Min");
						var newMin = EditorGUILayout.IntField(modifier.modValueMin, GUILayout.MaxWidth(60));
						if (newMin != modifier.modValueMin) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Limit Min");
							modifier.modValueMin = newMin;
						}
						GUILayout.Label("Max");
						
						var newMax = EditorGUILayout.IntField(modifier.modValueMax, GUILayout.MaxWidth(60));
						if (newMax != modifier.modValueMax) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Limit Max");
							modifier.modValueMax = newMax;
						}
						GUI.backgroundColor = Color.green;
						if (GUILayout.Button(new GUIContent("Delete", "Remove this Limit"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(64))) {
							indexToDelete = j;
						}
						GUI.backgroundColor = Color.white;
						GUILayout.Space(5);
						EditorGUILayout.EndHorizontal();
						if (modifier.modValueMin > modifier.modValueMax) {
							DTInspectorUtility.ShowColorWarning("  *" + modifier.statName + " Min cannot exceed Max, please fix!");
						}
					}
					
					DTInspectorUtility.ShowColorWarning("  *Limits are inclusive: i.e. 'Above' means >=");
					if (indexToDelete.HasValue) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "delete Limit");
						settings.activeItemCriteria.DeleteByIndex(indexToDelete.Value);
						isDirty = true;
					}
					
					EditorGUILayout.Separator();
				}
			
				break;
		}

		var newGO = (TriggeredSpawner.GameOverBehavior)EditorGUILayout.EnumPopup("Game Over Behavior", settings.gameOverBehavior);
		if (newGO != settings.gameOverBehavior) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "change Game Over Behavior");
			settings.gameOverBehavior = newGO;
		}

		var newPause = (TriggeredSpawner.WavePauseBehavior)EditorGUILayout.EnumPopup("Wave Pause Behavior", settings.wavePauseBehavior);
		if (newPause != settings.wavePauseBehavior) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "change Wave Pause Behavior");
			settings.wavePauseBehavior = newPause;
		}

        var hadNoListener = settings.listener == null;
		var newListener = (WaveSyncroSpawnerListener)EditorGUILayout.ObjectField("Listener Prefab", settings.listener, typeof(WaveSyncroSpawnerListener), true);
		if (newListener != settings.listener) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "assign Listener");
			settings.listener = newListener;
	        if (hadNoListener && settings.listener != null)
	        {
	            settings.listener.sourceSpawnerName = settings.transform.name;
	        }
		}

        EditorGUILayout.Separator();
        EditorGUI.indentLevel = 0;
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        var disabledText = "";
        if (settings.activeMode == LevelSettings.ActiveItemMode.Never)
        {
            disabledText = " --DISABLED--";
        }

		var newExpanded = DTInspectorUtility.Foldout(settings.isExpanded,
			string.Format("Wave Settings ({0}){1}", settings.waveSpecs.Count, disabledText));
		if (newExpanded != settings.isExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "toggle expand Wave Settings");
			settings.isExpanded = newExpanded;
		}
        // BUTTONS...
        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));

        DTInspectorUtility.FunctionButtons waveButtonPressed = DTInspectorUtility.FunctionButtons.None;

        if (settings.activeMode != LevelSettings.ActiveItemMode.Never) 
        {
            // Add expand/collapse buttons if there are items in the list
            if (settings.waveSpecs.Count > 0)
            {
                GUIContent content;
                var collapseIcon = '\u2261'.ToString();
                content = new GUIContent(collapseIcon, "Click to collapse all");
                var masterCollapse = GUILayout.Button(content, EditorStyles.toolbarButton);

                var expandIcon = '\u25A1'.ToString();
                content = new GUIContent(expandIcon, "Click to expand all");
                var masterExpand = GUILayout.Button(content, EditorStyles.toolbarButton);
                if (masterExpand)
                {
                    ExpandCollapseAll(true);
                }
                if (masterCollapse)
                {
                    ExpandCollapseAll(false);
                }
            }
            else
            {
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(50));
            // A little space between button groups
            GUILayout.Space(6);

            var addText = string.Format("Click to add Wave{0}.", settings.waveSpecs.Count > 0 ? " before the first" : "");

            // Main Add button
            if (GUILayout.Button(new GUIContent("+", addText), EditorStyles.toolbarButton))
            {
                isDirty = true;
                if (levelSettings.LevelTimes.Count == 0)
                {
                    DTInspectorUtility.ShowAlert("You will not have any Level or Wave #'s to select in your Spawner Wave Settings until you add a Level in LevelSettings. Please do that first.");
                }
                else
                {
                    var newWave = new WaveSpecifics();
					UndoHelper.RecordObjectPropertyForUndo(settings, "add Wave");
					settings.waveSpecs.Add(newWave);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            if (settings.isExpanded)
            {
                EditorGUI.indentLevel = 0;

                int waveToInsertAt = -1;
                WaveSpecifics waveToDelete = null;
                WaveSpecifics waveSetting = null;
                int? waveToMoveUp = null;
                int? waveToMoveDown = null;
                LevelWave levelWave = null;

                // get list of prefab pools.
                var poolNames = LevelSettings.GetSortedPrefabPoolNames();

                for (var w = 0; w < settings.waveSpecs.Count; w++)
                {
                    EditorGUI.indentLevel = 1;
                    waveSetting = settings.waveSpecs[w];
                    levelWave = GetLevelWaveFromWaveSpec(waveSetting);

                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                    string sDisabled = "";
                    if (!waveSetting.isExpanded && !waveSetting.enableWave)
                    {
                        sDisabled = " DISABLED ";
                    }

					newExpanded = DTInspectorUtility.Foldout(waveSetting.isExpanded,
	                  string.Format("Wave Setting #{0} ({1}/{2}){3}", (w + 1),
				              waveSetting.SpawnLevelNumber + 1,
				              waveSetting.SpawnWaveNumber + 1,
				              sDisabled));
					if (newExpanded != waveSetting.isExpanded) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "toggle expand Wave Setting");
						waveSetting.isExpanded = newExpanded;
					}
					
					GUILayout.FlexibleSpace();
                    waveButtonPressed = DTInspectorUtility.AddFoldOutListItemButtons(w, settings.waveSpecs.Count, "Wave", true, true);
                    EditorGUILayout.EndHorizontal();

                    switch (waveButtonPressed)
                    {
                        case DTInspectorUtility.FunctionButtons.Remove:
                            waveToDelete = waveSetting;
							isDirty = true;
                            break;
                        case DTInspectorUtility.FunctionButtons.Add:
                            waveToInsertAt = w;
							isDirty = true;
                            break;
                        case DTInspectorUtility.FunctionButtons.ShiftDown:
                            waveToMoveDown = w;
							isDirty = true;
                            break;
                        case DTInspectorUtility.FunctionButtons.ShiftUp:
                            waveToMoveUp = w;
							isDirty = true;
                            break;
                    }

                    if (waveSetting.isExpanded)
                    {
                        EditorGUI.indentLevel = 0;
						var newEnabled = EditorGUILayout.BeginToggleGroup("Enable Wave", waveSetting.enableWave);
						if (newEnabled != waveSetting.enableWave) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Enable Wave");
							waveSetting.enableWave = newEnabled;
						}

                        var oldLevelNumber = waveSetting.SpawnLevelNumber;

						var newLevel = EditorGUILayout.IntPopup("Level#", waveSetting.SpawnLevelNumber + 1, LevelNames, LevelIndexes) - 1;
						if (newLevel != waveSetting.SpawnLevelNumber) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Level#");
							waveSetting.SpawnLevelNumber = newLevel;

	                        if (oldLevelNumber != waveSetting.SpawnLevelNumber)
	                        {
	                            waveSetting.SpawnWaveNumber = 0;
	                        }
						}

						var newWave = EditorGUILayout.IntPopup("Wave#", waveSetting.SpawnWaveNumber + 1,
							WaveNamesForLevel(waveSetting.SpawnLevelNumber), WaveIndexesForLevel(waveSetting.SpawnLevelNumber)) - 1;
						if (newWave != waveSetting.SpawnWaveNumber) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Wave#");
							waveSetting.SpawnWaveNumber = newWave;
						}

						var newMin = EditorGUILayout.IntSlider("Min To Spawn", waveSetting.MinToSpawn, 1, 100);
						if (newMin != waveSetting.MinToSpawn) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Min To Spawn");
							waveSetting.MinToSpawn = newMin;
						}

						var newMax = EditorGUILayout.IntSlider("Max To Spawn", waveSetting.MaxToSpawn, 1, 100);
						if (newMax != waveSetting.MaxToSpawn) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Max To Spawn");
							waveSetting.MaxToSpawn = newMax;
						}

                        if (waveSetting.MinToSpawn > waveSetting.MaxToSpawn)
                        {
                            waveSetting.MaxToSpawn = waveSetting.MinToSpawn;
                        }
                        else if (waveSetting.MaxToSpawn < waveSetting.MinToSpawn)
                        {
                            waveSetting.MinToSpawn = waveSetting.MaxToSpawn;
                        }

						var newTimeSpawnAll = EditorGUILayout.Slider("Time To Spawn All", waveSetting.TimeToSpawnWholeWave, 0f, SecondsForWave(waveSetting) - .1f);
						if (newTimeSpawnAll != waveSetting.TimeToSpawnWholeWave) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Time To Spawn All");
							waveSetting.TimeToSpawnWholeWave = newTimeSpawnAll;
						}

						var newDelay = EditorGUILayout.Slider("Delay Wave (sec)", waveSetting.WaveDelaySeconds, 0f, 50f);
						if (newDelay != waveSetting.WaveDelaySeconds) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Delay Wave");
							waveSetting.WaveDelaySeconds = newDelay;
						}

						var newSource = (WaveSpecifics.SpawnOrigin)EditorGUILayout.EnumPopup("Prefab Type", waveSetting.spawnSource);
						if (newSource != waveSetting.spawnSource) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Prefab Type");
							waveSetting.spawnSource = newSource;
						}
                        switch (waveSetting.spawnSource)
                        {
                            case WaveSpecifics.SpawnOrigin.Specific:
								var newPrefab = (Transform)EditorGUILayout.ObjectField("Prefab To Spawn", waveSetting.prefabToSpawn, typeof(Transform), true);
								if (newPrefab != waveSetting.prefabToSpawn) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Prefab To Spawn");
									waveSetting.prefabToSpawn = newPrefab;
								}
                                break;
                            case WaveSpecifics.SpawnOrigin.PrefabPool:
                                if (poolNames != null)
                                {
                                    var pool = LevelSettings.GetFirstMatchingPrefabPool(waveSetting.prefabPoolName);
                                    if (pool == null)
                                    {
                                        if (string.IsNullOrEmpty(waveSetting.prefabPoolName))
                                        {
                                            DTInspectorUtility.ShowColorWarning("*No Prefab Pool selected.");
                                        }
                                        else
                                        {
                                            DTInspectorUtility.ShowColorWarning("*Prefab Pool '" + waveSetting.prefabPoolName + "' not found. Select one.");
                                        }
                                        waveSetting.prefabPoolIndex = 0;
                                    }
                                    else
                                    {
                                        waveSetting.prefabPoolIndex = poolNames.IndexOf(waveSetting.prefabPoolName);
                                    }

                                    if (poolNames.Count > 0)
                                    {
										var newPoolIndex = EditorGUILayout.Popup("Prefab Pool", waveSetting.prefabPoolIndex, poolNames.ToArray());
										if (newPoolIndex != waveSetting.prefabPoolIndex) {
											UndoHelper.RecordObjectPropertyForUndo(settings, "change Prefab Pool");
											waveSetting.prefabPoolIndex = newPoolIndex;
										}

                                        if (waveSetting.prefabPoolIndex > 0)
                                        {
                                            var matchingPool = LevelSettings.GetFirstMatchingPrefabPool(poolNames[waveSetting.prefabPoolIndex]);
                                            if (matchingPool != null)
                                            {
                                                waveSetting.prefabPoolName = matchingPool.name;
                                            }
                                        } else {
											waveSetting.prefabPoolName = string.Empty;
										}
                                    }
                                    else
                                    {
                                        DTInspectorUtility.ShowColorWarning("*You have no Prefab Pools. Create one first.");
                                    }
                                }
                                else
                                {
                                    DTInspectorUtility.ShowColorWarning(LevelSettings.NO_PREFAB_POOLS_CONTAINER_ALERT);
                                    DTInspectorUtility.ShowColorWarning(LevelSettings.REVERT_LEVEL_SETTINGS_ALERT);
                                }

                                break;
                        }

						newExpanded = EditorGUILayout.BeginToggleGroup("Spawn Limit Controls", waveSetting.enableLimits);
						if (newExpanded != waveSetting.enableLimits) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Spawn Limit Controls");
							waveSetting.enableLimits = newExpanded;
						}
                        if (waveSetting.enableLimits)
                        {
                            DTInspectorUtility.ShowColorWarning("Stop spawning until all spawns from wave satisfy:");
                            
							var newCloser = EditorGUILayout.FloatField("Min. Distance", waveSetting.doNotSpawnIfMemberCloserThan);
							if (newCloser != waveSetting.doNotSpawnIfMemberCloserThan) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Min Distance");
								waveSetting.doNotSpawnIfMemberCloserThan = newCloser;
							}
                            
							var newRand = EditorGUILayout.FloatField("Random Distance", waveSetting.doNotSpawnRandomDistance);
							if (newRand != waveSetting.doNotSpawnRandomDistance) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Distance");
								waveSetting.doNotSpawnRandomDistance = newRand;
							}
                        }
                        EditorGUILayout.EndToggleGroup();

						newExpanded = EditorGUILayout.BeginToggleGroup("Repeat Wave", waveSetting.repeatWaveUntilNew);
						if (newExpanded != waveSetting.repeatWaveUntilNew) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Repeat Wave");
							waveSetting.repeatWaveUntilNew = newExpanded;
						}
                        if (waveSetting.repeatWaveUntilNew)
                        {
                            if (levelWave.waveType == LevelSettings.WaveType.Elimination)
                            {
								var newRepeatMode = (WaveSpecifics.RepeatWaveMode)EditorGUILayout.EnumPopup("Repeat Mode", waveSetting.curWaveRepeatMode);
								if (newRepeatMode != waveSetting.curWaveRepeatMode) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Repeat Mode");
									waveSetting.curWaveRepeatMode = newRepeatMode;
								}
                            } else {
								// only one mode for non-elimination waves.
								var newRepeatMode = (WaveSpecifics.TimedRepeatWaveMode) EditorGUILayout.EnumPopup("Timed Repeat Mode", waveSetting.curTimedRepeatWaveMode);
								if (newRepeatMode != waveSetting.curTimedRepeatWaveMode) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Timed Repeat Mode");
									waveSetting.curTimedRepeatWaveMode = newRepeatMode;
								}
							}

                            switch (waveSetting.curWaveRepeatMode)
                            {
                                case WaveSpecifics.RepeatWaveMode.NumberOfRepetitions:
                                    if (levelWave.waveType == LevelSettings.WaveType.Elimination)
                                    {
										var newRep = EditorGUILayout.IntSlider("Repetitions", waveSetting.repetitions, 2, 15);
										if (newRep != waveSetting.repetitions) {
											UndoHelper.RecordObjectPropertyForUndo(settings, "change Repetitions");
											waveSetting.repetitions = newRep;
										}
                                    }
                                    break;
                                case WaveSpecifics.RepeatWaveMode.UntilWorldVariableAbove:
                                case WaveSpecifics.RepeatWaveMode.UntilWorldVariableBelow:
                                    if (levelWave.waveType != LevelSettings.WaveType.Elimination)
                                    {
                                        break;
                                    }

                                    var missingStatNames = new List<string>();
                                    missingStatNames.AddRange(allStatNames);
                                    missingStatNames.RemoveAll(delegate(string obj)
                                    {
                                        return waveSetting.repeatPassCriteria.HasKey(obj);
                                    });

                                    var newStat = EditorGUILayout.Popup("Add Variable Limit", 0, missingStatNames.ToArray());
                                    if (newStat != 0)
                                    {
                                        AddStatModifier(missingStatNames[newStat], waveSetting);
                                    }

                                    if (waveSetting.repeatPassCriteria.statMods.Count == 0)
                                    {
                                        DTInspectorUtility.ShowColorWarning("*You have no Variable Limits. Wave will not repeat.");
                                    }
                                    else
                                    {
                                        EditorGUILayout.Separator();

                                        int? indexToDelete = null;

                                        for (var i = 0; i < waveSetting.repeatPassCriteria.statMods.Count; i++)
                                        {
                                            var modifier = waveSetting.repeatPassCriteria.statMods[i];
                                            EditorGUILayout.BeginHorizontal();
                                            GUILayout.Space(35);
                                            GUILayout.Label(modifier.statName, GUILayout.MaxWidth(120));

                                            GUILayout.Label(waveSetting.curWaveRepeatMode == WaveSpecifics.RepeatWaveMode.UntilWorldVariableAbove ? "Min" : "Max");
											
											var newValue = EditorGUILayout.IntField(modifier.modValue, GUILayout.MaxWidth(70));
											if (newValue != modifier.modValue) {
												UndoHelper.RecordObjectPropertyForUndo(settings, "change Limit value");
												modifier.modValue = newValue;
											}
                                            GUI.backgroundColor = Color.green;
                                            if (GUILayout.Button(new GUIContent("Delete", "Remove this Limit"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(64)))
                                            {
                                                indexToDelete = i;
                                            }
                                            GUI.backgroundColor = Color.white;
                                            GUILayout.FlexibleSpace();
                                            EditorGUILayout.EndHorizontal();
                                        }

                                        DTInspectorUtility.ShowColorWarning("  *Limits are inclusive: i.e. 'Above' means >=");
                                        if (indexToDelete.HasValue)
                                        {
											UndoHelper.RecordObjectPropertyForUndo(settings, "delete Limit");
                                            waveSetting.repeatPassCriteria.DeleteByIndex(indexToDelete.Value);
                                        }

                                        EditorGUILayout.Separator();
                                    }
                                    break;
                            }

							var newPauseMin = EditorGUILayout.Slider("Repeat Pause Min", waveSetting.repeatPauseMin, 0, 20);
							if (newPauseMin != waveSetting.repeatPauseMin) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Repeat Pause Min");
								waveSetting.repeatPauseMin = newPauseMin;
							}

							var newPauseMax = EditorGUILayout.Slider("Repeat Pause Max", waveSetting.repeatPauseMax, 0, 20);
							if (newPauseMax != waveSetting.repeatPauseMax) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Repeat Pause Max");
								waveSetting.repeatPauseMax = newPauseMax;
							}

                            if (waveSetting.repeatPauseMin > waveSetting.repeatPauseMax)
                            {
                                waveSetting.repeatPauseMax = waveSetting.repeatPauseMin;
                            }
                            else if (waveSetting.repeatPauseMax < waveSetting.repeatPauseMin)
                            {
                                waveSetting.repeatPauseMin = waveSetting.repeatPauseMax;
                            }

							var newItemInc = EditorGUILayout.IntSlider("Spawn Increase", waveSetting.repeatItemIncrease, -50, 50);
							if (newItemInc != waveSetting.repeatItemIncrease) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Spawn Increase");
								waveSetting.repeatItemIncrease = newItemInc;
							}
                            
							var newItemLimit = EditorGUILayout.IntSlider("Spawn Limit", waveSetting.repeatItemLimit, 1, 1000);
							if (newItemLimit != waveSetting.repeatItemLimit) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Spawn Limit");
								waveSetting.repeatItemLimit = newItemLimit;
							}

							var newTimeInc = EditorGUILayout.Slider("Time Increase", waveSetting.repeatTimeIncrease, -10f, 10f);
							if (newTimeInc != waveSetting.repeatTimeIncrease) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Time Increase");
								waveSetting.repeatTimeIncrease = newTimeInc;
							}
                            
							var newTimeLimit = EditorGUILayout.Slider("Time Limit", waveSetting.repeatTimeLimit, .1f, 500f);
							if (newTimeLimit != waveSetting.repeatTimeLimit) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Time Limit");
								waveSetting.repeatTimeLimit = newTimeLimit;
							}
						
						}
                        EditorGUILayout.EndToggleGroup();

                        // show randomizations
                        var variantTag = "Randomization";

						newExpanded = EditorGUILayout.BeginToggleGroup(variantTag, waveSetting.enableRandomizations);
						if (newExpanded != waveSetting.enableRandomizations) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Randomization");
							waveSetting.enableRandomizations = newExpanded;
						}
                        if (waveSetting.enableRandomizations)
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
                            EditorGUILayout.LabelField("Random Rotation");

							var newRandX = GUILayout.Toggle(waveSetting.randomXRotation, "X");
							if (newRandX != waveSetting.randomXRotation) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Random Rotation X");
								waveSetting.randomXRotation = newRandX;
							}
                            GUILayout.Space(10);
                            
							var newRandY = GUILayout.Toggle(waveSetting.randomYRotation, "Y");
							if (newRandY != waveSetting.randomYRotation) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Random Rotation Y");
								waveSetting.randomYRotation = newRandY;
							}
                            GUILayout.Space(10);
                            
							var newRandZ = GUILayout.Toggle(waveSetting.randomZRotation, "Z");
							if (newRandZ != waveSetting.randomZRotation) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Random Rotation Z");
								waveSetting.randomZRotation = newRandZ;
							}
                            EditorGUILayout.EndHorizontal();

                            if (waveSetting.randomXRotation)
                            {
								var newRotMinX = EditorGUILayout.Slider("Rand. X Rot. Min", waveSetting.randomXRotationMin, 0, 360f);
								if (newRotMinX != waveSetting.randomXRotationMin) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Random X Rotation Min");
									waveSetting.randomXRotationMin = newRotMinX;
								}
                                
								var newRotMaxX = EditorGUILayout.Slider("Rand. X Rot. Max", waveSetting.randomXRotationMax, 0, 360f);
								if (newRotMaxX != waveSetting.randomXRotationMax) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Random X Rotation Max");
									waveSetting.randomXRotationMax = newRotMaxX;
								}
                                
								waveSetting.randomXRotationMax = Mathf.Max(waveSetting.randomXRotationMin, waveSetting.randomXRotationMax);
                            }

                            if (waveSetting.randomYRotation)
                            {
								var newRotMinY = EditorGUILayout.Slider("Rand. Y Rot. Min", waveSetting.randomYRotationMin, 0, 360f);
								if (newRotMinY != waveSetting.randomYRotationMin) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Y Rotation Min");
									waveSetting.randomYRotationMin = newRotMinY;
								}

								var newRotMaxY = EditorGUILayout.Slider("Rand. Y Rot. Max", waveSetting.randomYRotationMax, 0, 360f);
								if (newRotMaxY != waveSetting.randomYRotationMax) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Y Rotation Max");
									waveSetting.randomYRotationMax = newRotMaxY;
								}
                                
								waveSetting.randomYRotationMax = Mathf.Max(waveSetting.randomYRotationMin, waveSetting.randomYRotationMax);
                            }

                            if (waveSetting.randomZRotation)
                            {
								var newRotMinZ = EditorGUILayout.Slider("Rand. Z Rot. Min", waveSetting.randomZRotationMin, 0, 360f);
								if (newRotMinZ != waveSetting.randomZRotationMin) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Z Rotation Min");
									waveSetting.randomZRotationMin = newRotMinZ;
								}
                                
								var newRotMaxZ = EditorGUILayout.Slider("Rand. Z Rot. Max", waveSetting.randomZRotationMax, 0, 360f);
								if (newRotMaxZ != waveSetting.randomZRotationMax) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Z Rotation Max");
									waveSetting.randomZRotationMax = newRotMaxZ;
								}
                                
								waveSetting.randomZRotationMax = Mathf.Max(waveSetting.randomZRotationMin, waveSetting.randomZRotationMax);
                            }

                            EditorGUILayout.Separator();

							var newDistX = EditorGUILayout.Slider("Rand. Distance X", waveSetting.randomDistanceX, 0, TriggeredSpawnerInspector.MAX_DISTANCE);
							if (newDistX != waveSetting.randomDistanceX) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Distance X");
								waveSetting.randomDistanceX = newDistX;
							}

							var newDistY = EditorGUILayout.Slider("Rand. Distance Y", waveSetting.randomDistanceY, 0, TriggeredSpawnerInspector.MAX_DISTANCE);
							if (newDistY != waveSetting.randomDistanceY) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Distance Y");
								waveSetting.randomDistanceY = newDistY;
							}
                            
							var newDistZ = EditorGUILayout.Slider("Rand. Distance Z", waveSetting.randomDistanceZ, 0, TriggeredSpawnerInspector.MAX_DISTANCE);
							if (newDistZ != waveSetting.randomDistanceZ) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Distance Z");
								waveSetting.randomDistanceZ = newDistZ;
							}
                        }
                        EditorGUILayout.EndToggleGroup();

                        // show increments
                        var incTag = "Incremental Settings";
						newExpanded = EditorGUILayout.BeginToggleGroup(incTag, waveSetting.enableIncrements);
						if (newExpanded != waveSetting.enableIncrements) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Incremental Settings");
							waveSetting.enableIncrements = newExpanded;
						}
                        if (waveSetting.enableIncrements)
                        {
							var newIncX = EditorGUILayout.Slider("Distance X", waveSetting.incrementPosX, -100f, 100f);
							if (newIncX != waveSetting.incrementPosX) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Distance X");
								waveSetting.incrementPosX = newIncX;
							}
                            
							var newIncY = EditorGUILayout.Slider("Distance Y", waveSetting.incrementPosY, -100f, 100f);
							if (newIncY != waveSetting.incrementPosY) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Distance Y");
								waveSetting.incrementPosY = newIncY;
							}
                            
							var newIncZ = EditorGUILayout.Slider("Distance Z", waveSetting.incrementPosZ, -100f, 100f);
							if (newIncZ != waveSetting.incrementPosZ) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Distance Z");
								waveSetting.incrementPosZ = newIncZ;
							}

                            EditorGUILayout.Separator();

                            if (waveSetting.enableRandomizations && waveSetting.randomXRotation)
                            {
                                DTInspectorUtility.ShowColorWarning("*Rotation X - cannot be used with Random Rotation X.");
                            }
                            else
                            {
								var newIncRotX = EditorGUILayout.Slider("Rotation X", waveSetting.incrementRotationX, -180f, 180f);
								if (newIncRotX != waveSetting.incrementRotationX) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Rotation X");
									waveSetting.incrementRotationX = newIncRotX;
								}
                            }

                            if (waveSetting.enableRandomizations && waveSetting.randomYRotation)
                            {
                                DTInspectorUtility.ShowColorWarning("*Rotation Y - cannot be used with Random Rotation Y.");
                            }
                            else
                            {
								var newIncRotY = EditorGUILayout.Slider("Rotation Y", waveSetting.incrementRotationY, -180f, 180f);
								if (newIncRotY != waveSetting.incrementRotationY) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Rotation Y");
									waveSetting.incrementRotationY = newIncRotY;
								}
                            }

                            if (waveSetting.enableRandomizations && waveSetting.randomZRotation)
                            {
                                DTInspectorUtility.ShowColorWarning("*Rotation Z - cannot be used with Random Rotation Z.");
                            }
                            else
                            {
								var newIncRotZ = EditorGUILayout.Slider("Rotation Z", waveSetting.incrementRotationZ, -180f, 180f);
								if (newIncRotZ != waveSetting.incrementRotationZ) {
									UndoHelper.RecordObjectPropertyForUndo(settings, "change Rotation Z");
									waveSetting.incrementRotationZ = newIncRotZ;
								}
                            }
                        }
                        EditorGUILayout.EndToggleGroup();


                        // show increments
                        incTag = "Post-spawn Nudge Settings";
						newExpanded = EditorGUILayout.BeginToggleGroup(incTag, waveSetting.enablePostSpawnNudge);
						if (newExpanded != waveSetting.enablePostSpawnNudge) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Post-spawn Nudge Settings");
							waveSetting.enablePostSpawnNudge = newExpanded;
						}
                        if (waveSetting.enablePostSpawnNudge)
                        {
							var newForward = EditorGUILayout.Slider("Nudge Forward", waveSetting.postSpawnNudgeForward, -100f, 100f);
							if (newForward != waveSetting.postSpawnNudgeForward) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Nudge Forward");
								waveSetting.postSpawnNudgeForward = newForward;
							}
                            
							var newRight = EditorGUILayout.Slider("Nudge Right", waveSetting.postSpawnNudgeRight, -100f, 100f);
							if (newRight != waveSetting.postSpawnNudgeRight) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Nudge Right");
								waveSetting.postSpawnNudgeRight = newRight;
							}
                            
							var newDown = EditorGUILayout.Slider("Nudge Down", waveSetting.postSpawnNudgeDown, -100f, 100f);
							if (newDown != waveSetting.postSpawnNudgeDown) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Nudge Down");
								waveSetting.postSpawnNudgeDown = newDown;
							}
                        }
                        EditorGUILayout.EndToggleGroup();

                        EditorGUILayout.EndToggleGroup();
                        EditorGUILayout.Separator();
                    }
                }

                if (waveToDelete != null)
                {
					UndoHelper.RecordObjectPropertyForUndo(settings, "delete Wave");
                    settings.waveSpecs.Remove(waveToDelete);
                }

                if (waveToInsertAt > -1)
                {
                    if (levelSettings.LevelTimes.Count == 0)
                    {
                        DTInspectorUtility.ShowAlert("You will not have any Level or Wave #'s to select in your Spawner Wave Settings until you add a Level in LevelSettings. Please do that first.");
                    }
                    else
                    {
                        var newWave = new WaveSpecifics();
						UndoHelper.RecordObjectPropertyForUndo(settings, "add Wave");
						settings.waveSpecs.Insert(waveToInsertAt + 1, newWave);
                    }
                }

                if (waveToMoveUp.HasValue)
                {
                    var item = settings.waveSpecs[waveToMoveUp.Value];
					UndoHelper.RecordObjectPropertyForUndo(settings, "shift up Wave");
					settings.waveSpecs.Insert(waveToMoveUp.Value - 1, item);
                    settings.waveSpecs.RemoveAt(waveToMoveUp.Value + 1);
                }

                if (waveToMoveDown.HasValue)
                {
                    var index = waveToMoveDown.Value + 1;

                    var item = settings.waveSpecs[index];
					UndoHelper.RecordObjectPropertyForUndo(settings, "shift down Wave");
					settings.waveSpecs.Insert(index - 1, item);
                    settings.waveSpecs.RemoveAt(index + 1);
                }
            }
        }
        else
        {
            EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed || isDirty)
        {
            EditorUtility.SetDirty(target);	// or it won't save the data!!
        }

		this.Repaint();
        //DrawDefaultInspector();
    }
	
	private void AddStatModifier(string modifierName, WaveSpecifics spec) {
		if (spec.repeatPassCriteria.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This wave already has a Variable Limit for World Variable: " + modifierName + ". Please modify the existing one instead.");
			return;
		}
	
		UndoHelper.RecordObjectPropertyForUndo(settings, "add Variable Limit");

		spec.repeatPassCriteria.statMods.Add(new WorldVariableModifier() {
			statName = modifierName
		});
	}
	
	private void ExpandCollapseAll(bool isExpand) {
		UndoHelper.RecordObjectPropertyForUndo(settings, "toggle expand / collapse Wave Settings");

		foreach (var wave in settings.waveSpecs) {
			wave.isExpanded = isExpand;
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
		if (GetLevelSettings.LevelTimes.Count <= levelNumber) {
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
		if (GetLevelSettings.LevelTimes.Count <= levelNumber) {
			return new int[0];
		}

		var level = GetLevelSettings.LevelTimes[levelNumber];
		var indexes = new int[level.WaveSettings.Count];
		
		for (var i = 0; i < level.WaveSettings.Count; i++) {
			indexes[i] = i + 1;
		}
		
		return indexes;
	}
	
	private LevelWave GetLevelWaveFromWaveSpec(WaveSpecifics waveSpec) {
		var levelNumber = waveSpec.SpawnLevelNumber;
		var waveNumber = waveSpec.SpawnWaveNumber;
		
		if (GetLevelSettings.LevelTimes.Count <= levelNumber) {
			return null;
		}
		
		var wave = GetLevelSettings.LevelTimes[levelNumber].WaveSettings[waveNumber];
		return wave;
	}
	
	private float SecondsForWave(WaveSpecifics waveSpec) {
		var wave  = GetLevelWaveFromWaveSpec(waveSpec);
		
		return wave.waveType == LevelSettings.WaveType.Timed ? wave.WaveDuration : 99;
	}

	private void AddActiveLimit(string modifierName) {
		if (settings.activeItemCriteria.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This item already has a Active Limit for World Variable: " + modifierName + ". Please modify the existing one instead.");
			return;
		}
		
		UndoHelper.RecordObjectPropertyForUndo(settings, "add Active Limit");

		settings.activeItemCriteria.statMods.Add(new WorldVariableRange() {
			statName = modifierName
		});
	}
	
	private LevelSettings GetLevelSettings {
		get {
			if (levSettings == null) {			
				levSettings = LevelSettings.GetLevelSettings;
			}
			
			return levSettings;
		}
	}
}
