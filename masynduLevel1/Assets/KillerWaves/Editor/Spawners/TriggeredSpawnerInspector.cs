using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
	[CustomEditor(typeof(TriggeredSpawner))]
#else
	[CustomEditor(typeof(TriggeredSpawner), true)]
#endif
public class TriggeredSpawnerInspector : Editor {
	public const int MAX_DISTANCE = 5000;
	private TriggeredSpawner settings;
	private List<string> allStatNames;
	
	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		
		settings = (TriggeredSpawner)target;

		allStatNames = new List<string>() {
			DTInspectorUtility.DROP_DOWN_NONE_OPTION
		};
		
		var ls = LevelSettings.GetLevelSettings;
		if (ls != null) {
			DTInspectorUtility.DrawTexture(ls.logoTexture);

			Transform statsHolder = null;
			
			statsHolder = ls.transform.Find(LevelSettings.PLAYER_STATS_CONTAINER_TRANS_NAME);
			for (var i = 0; i < statsHolder.childCount; i++) {
				var child = statsHolder.GetChild(i);
				allStatNames.Add(child.name);
			}
		}

		EditorGUI.indentLevel = 0;
		bool isDirty = false;

		var newMinimal = EditorGUILayout.Toggle("Minimal Mode", settings.hideUnusedEvents);
		if (newMinimal != settings.hideUnusedEvents) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Minimal Mode");
			settings.hideUnusedEvents = newMinimal;
		}
		if (settings.hideUnusedEvents) {
			var unusedEvents = getUnusedEventTypes();
			
			var newEventindex = EditorGUILayout.Popup("Event To Activate", 0, unusedEvents.ToArray());
			
			if (newEventindex > 0) {
				isDirty = true;
				ActivateEvent(newEventindex, unusedEvents);	
			}
		}

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
					AddActiveLimit(missingStatNames[newStat], settings);
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
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Modifier Min");
							modifier.modValueMin = newMin;
						}

						GUILayout.Label("Max");
						var newMax = EditorGUILayout.IntField(modifier.modValueMax, GUILayout.MaxWidth(60));
						if (newMax != modifier.modValueMax) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Modifier Max");
							modifier.modValueMax = newMax;
						}
						GUI.backgroundColor = Color.green;
						if (GUILayout.Button(new GUIContent("Delete", "Remove this limit"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(64))) {
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
						UndoHelper.RecordObjectPropertyForUndo(settings, "delete Modifier");
						settings.activeItemCriteria.DeleteByIndex(indexToDelete.Value);
						isDirty = true;
					}
					
					EditorGUILayout.Separator();
				}
			
				break;
		}
		
		var childSpawnerCount = TriggeredSpawner.GetChildSpawners(settings.transform).Count;
		
		var newSource = (TriggeredSpawner.SpawnerEventSource) EditorGUILayout.EnumPopup("Trigger Source", settings.eventSourceType);
		if (newSource != settings.eventSourceType) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "change Trigger Source");
			settings.eventSourceType = newSource;
		}
		if (childSpawnerCount > 0) {
			var newTransmit = EditorGUILayout.Toggle("Propagate Triggers", settings.transmitEventsToChildren);
			if (newTransmit != settings.transmitEventsToChildren) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Propagate Triggers");
				settings.transmitEventsToChildren = newTransmit;
			}
		} else {
			DTInspectorUtility.ShowColorWarning("*Cannot propagate events with no child spawners");
		}

		var newGO = (TriggeredSpawner.GameOverBehavior) EditorGUILayout.EnumPopup("Game Over Behavior", settings.gameOverBehavior);
		if (newGO != settings.gameOverBehavior) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "change Game Over Behavior");
			settings.gameOverBehavior = newGO;
		}

		var newPause = (TriggeredSpawner.WavePauseBehavior) EditorGUILayout.EnumPopup("Wave Pause Behavior", settings.wavePauseBehavior);
		if (newPause != settings.wavePauseBehavior) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "change Wave Pause Behavior");
			settings.wavePauseBehavior = newPause;
		}

		var hadNoListener = settings.listener == null;
		var newListener = (TriggeredSpawnerListener) EditorGUILayout.ObjectField("Listener", settings.listener, typeof(TriggeredSpawnerListener), true);
		if (newListener != settings.listener) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "assign Listener");
			settings.listener = newListener;
			if (hadNoListener && settings.listener != null) {
				settings.listener.sourceSpawnerName = settings.transform.name;
			}
		}

		EditorGUILayout.Separator();
		
		EditorGUI.indentLevel = -1;
		
		List<bool> changedList = new List<bool>();
		if (!settings.hideUnusedEvents || settings.enableWave.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate Enabled Event");
			changedList.Add(RenderTriggeredWave(settings.enableWave, "Enabled Event", TriggeredSpawner.EventType.OnEnabled));
		}
		if (!settings.hideUnusedEvents || settings.disableWave.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate Disabled Event");
			changedList.Add(RenderTriggeredWave(settings.disableWave, "Disabled Event", TriggeredSpawner.EventType.OnDisabled));
		}
		if (!settings.hideUnusedEvents || settings.visibleWave.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate Visible Event");
			changedList.Add(RenderTriggeredWave(settings.visibleWave, "Visible Event", TriggeredSpawner.EventType.Visible));
		}
		if (!settings.hideUnusedEvents || settings.invisibleWave.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate Invisible Event");
			changedList.Add(RenderTriggeredWave(settings.invisibleWave, "Invisible Event", TriggeredSpawner.EventType.Invisible));
		}
		if (!settings.hideUnusedEvents || settings.mouseOverWave.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate MouseOver Event");
			changedList.Add(RenderTriggeredWave(settings.mouseOverWave, "MouseOver Event", TriggeredSpawner.EventType.MouseOver));
		}
		if (!settings.hideUnusedEvents || settings.mouseClickWave.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate MouseClick Event");
			changedList.Add(RenderTriggeredWave(settings.mouseClickWave, "MouseClick Event", TriggeredSpawner.EventType.MouseClick));
		}
		if (!settings.hideUnusedEvents || settings.collisionWave.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate Collision Event");
			changedList.Add(RenderTriggeredWave(settings.collisionWave, "Collision Event", TriggeredSpawner.EventType.OnCollision));
		}
		if (!settings.hideUnusedEvents || settings.triggerEnterWave.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate Trigger Enter Event");
			changedList.Add(RenderTriggeredWave(settings.triggerEnterWave, "Trigger Enter Event", TriggeredSpawner.EventType.OnTriggerEnter));
		}
		if (!settings.hideUnusedEvents || settings.triggerExitWave.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate Trigger Exit Event");
			changedList.Add(RenderTriggeredWave(settings.triggerExitWave, "Trigger Exit Event", TriggeredSpawner.EventType.OnTriggerExit));
		}

		#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
			// not supported
		#else
			// Unity 4.3 Events
			if (!settings.hideUnusedEvents || settings.collision2dWave.enableWave) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "activate 2D Collision Event");
				changedList.Add(RenderTriggeredWave(settings.collision2dWave, "2D Collision Event", TriggeredSpawner.EventType.OnCollision2D));
			}

			if (!settings.hideUnusedEvents || settings.triggerEnter2dWave.enableWave) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "activate 2D Trigger Enter Event");
				changedList.Add(RenderTriggeredWave(settings.triggerEnter2dWave, "2D Trigger Enter Event", TriggeredSpawner.EventType.OnTriggerEnter2D));
			}

			if (!settings.hideUnusedEvents || settings.triggerExit2dWave.enableWave) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "activate 2D Trigger Exit Event");
				changedList.Add(RenderTriggeredWave(settings.triggerExit2dWave, "2D Trigger Exit Event", TriggeredSpawner.EventType.OnTriggerExit2D));
			}

		#endif

		// code triggered event
		if (!settings.hideUnusedEvents || settings.codeTriggeredWave1.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate Code-Triggered 1 Event");
			changedList.Add(RenderTriggeredWave(settings.codeTriggeredWave1, "Code-Triggered 1", TriggeredSpawner.EventType.CodeTriggered1));
		}
		if (!settings.hideUnusedEvents || settings.codeTriggeredWave2.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate Code-Triggered 2 Event");
			changedList.Add(RenderTriggeredWave(settings.codeTriggeredWave2, "Code-Triggered 2", TriggeredSpawner.EventType.CodeTriggered2));
		}

		// Pool Manager events
		if (!settings.hideUnusedEvents || settings.spawnedWave.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate OnSpawned Event");
			changedList.Add(RenderTriggeredWave(settings.spawnedWave, "OnSpawned Event", TriggeredSpawner.EventType.OnSpawned));
		}
		if (!settings.hideUnusedEvents || settings.despawnedWave.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate OnDespawned Event");
			changedList.Add(RenderTriggeredWave(settings.despawnedWave, "OnDespawned Event", TriggeredSpawner.EventType.OnDespawned));
		}
		
		// NGUI events
		if (!settings.hideUnusedEvents || settings.clickWave.enableWave) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "activate OnClick Event");
			changedList.Add(RenderTriggeredWave(settings.clickWave, "OnClick Event", TriggeredSpawner.EventType.OnClick));
		}
		
		if (GUI.changed || changedList.Contains(true) || isDirty) {
  			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}

		this.Repaint();
		//DrawDefaultInspector();
    }
	
	private bool RenderTriggeredWave(TriggeredWaveSpecifics waveSetting, string toggleText, TriggeredSpawner.EventType eventType) {
		if (settings.activeMode == LevelSettings.ActiveItemMode.Never) {
			toggleText += " - DISABLED";
		}

		EditorGUI.indentLevel = 0;
        EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);

		if (settings.activeMode == LevelSettings.ActiveItemMode.Never) {
			EditorGUILayout.LabelField(toggleText);
			return false;
		} else {
			waveSetting.enableWave = EditorGUILayout.Toggle(toggleText, waveSetting.enableWave);
		}

        EditorGUILayout.EndHorizontal();
		 
		var poolNames = PoolNames;
		var isDirty = false;
		
		if (waveSetting.enableWave) {
			var newSource = (WaveSpecifics.SpawnOrigin) EditorGUILayout.EnumPopup("Prefab Type", waveSetting.spawnSource);
			if (newSource != waveSetting.spawnSource) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "change Prefab Type");
				waveSetting.spawnSource = newSource;
			}
			switch (waveSetting.spawnSource) {
				case WaveSpecifics.SpawnOrigin.Specific:
					var newSpecific = (Transform) EditorGUILayout.ObjectField("Prefab To Spawn", waveSetting.prefabToSpawn, typeof(Transform), true);
					if (newSpecific != waveSetting.prefabToSpawn) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Prefab To Spawn");
						waveSetting.prefabToSpawn = newSpecific;
					}
					break;
				case WaveSpecifics.SpawnOrigin.PrefabPool:
					if (poolNames != null) {
						var pool = LevelSettings.GetFirstMatchingPrefabPool(waveSetting.prefabPoolName);
						if (pool == null) {
							if (string.IsNullOrEmpty(waveSetting.prefabPoolName)) {
								DTInspectorUtility.ShowColorWarning("*No Prefab Pool selected.");
							} else {
								DTInspectorUtility.ShowColorWarning("*Prefab Pool '" + waveSetting.prefabPoolName + "' not found. Select one.");
							}
							waveSetting.prefabPoolIndex = 0;
						} else {
							waveSetting.prefabPoolIndex = poolNames.IndexOf(waveSetting.prefabPoolName);
						}

						if (poolNames.Count > 0) {
							var newPool = EditorGUILayout.Popup("Prefab Pool", waveSetting.prefabPoolIndex, poolNames.ToArray());
							if (newPool != waveSetting.prefabPoolIndex) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Prefab Pool");
								waveSetting.prefabPoolIndex = newPool;
							}
						
							if (waveSetting.prefabPoolIndex > 0) {						
								var matchingPool = 	LevelSettings.GetFirstMatchingPrefabPool(poolNames[waveSetting.prefabPoolIndex]);
								if (matchingPool != null) {	
									waveSetting.prefabPoolName = matchingPool.name;
								}
							} else {
								waveSetting.prefabPoolName = string.Empty;
							}
						} else {
							DTInspectorUtility.ShowColorWarning("*You have no Prefab Pools. Create one first.");
						}
					} else {
						DTInspectorUtility.ShowColorWarning(LevelSettings.NO_PREFAB_POOLS_CONTAINER_ALERT);
						DTInspectorUtility.ShowColorWarning(LevelSettings.REVERT_LEVEL_SETTINGS_ALERT);
					}
				
					break;
			}

			var newNumber = EditorGUILayout.IntSlider("Number To Spawn", waveSetting.NumberToSpawn, 0, 100);
			if (newNumber != waveSetting.NumberToSpawn) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "change Number To Spawn");
				waveSetting.NumberToSpawn = newNumber;
			}
		
			if (!TriggeredSpawner.eventsWithInflexibleWaveLength.Contains(eventType)) {
				var newTime = EditorGUILayout.Slider("Time To Spawn All", waveSetting.TimeToSpawnWholeWave, 0f, 50);
				if (newTime != waveSetting.TimeToSpawnWholeWave) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "change Time To Spawn All");
					waveSetting.TimeToSpawnWholeWave = newTime;
				}
			} 
			
			if (!TriggeredSpawner.eventsWithInflexibleWaveLength.Contains(eventType)) {
				var newDelay = EditorGUILayout.Slider("Delay Wave (sec)", waveSetting.WaveDelaySeconds, 0f, 50f);
				if (newDelay != waveSetting.WaveDelaySeconds) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "change Delay Wave");
					waveSetting.WaveDelaySeconds = newDelay;
				}
			} 

			if (!TriggeredSpawner.eventsWithInflexibleWaveLength.Contains(eventType)) {
				var newRetrigger = (TriggeredSpawner.RetriggerLimitMode) EditorGUILayout.EnumPopup("Retrigger Limit Mode", waveSetting.retriggerLimitMode);
				if (newRetrigger != waveSetting.retriggerLimitMode) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "change Retrigger Limit Mode");
					waveSetting.retriggerLimitMode = newRetrigger;
				}
				switch (waveSetting.retriggerLimitMode) {
					case TriggeredSpawner.RetriggerLimitMode.FrameBased:
						var newFrameLimit = EditorGUILayout.IntSlider("Min Frames Between", waveSetting.limitPerXFrames, 1, 120);
						if (newFrameLimit != waveSetting.limitPerXFrames) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Min Frame Between");
							waveSetting.limitPerXFrames = newFrameLimit;
						}
						break;
					case TriggeredSpawner.RetriggerLimitMode.TimeBased:
						var newSecondLimit = EditorGUILayout.Slider("Min Seconds Between", waveSetting.limitPerXSeconds, 0.1f, 10f);
						if (newSecondLimit != waveSetting.limitPerXSeconds) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Min Seconds Between");
							waveSetting.limitPerXSeconds = newSecondLimit;
						}
						break;
				}
			}
			
			if (TriggeredSpawner.eventsThatCanTriggerDespawn.Contains(eventType)) {
				var newWillDespawn = EditorGUILayout.Toggle("Despawn This", waveSetting.willDespawnOnEvent);
				if (newWillDespawn != waveSetting.willDespawnOnEvent) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Despawn This");
					waveSetting.willDespawnOnEvent = newWillDespawn;
				}
			}			
			
			if (TriggeredSpawner.eventsWithTagLayerFilters.Contains(eventType)) {
				var newLayer = EditorGUILayout.BeginToggleGroup("Layer Filter", waveSetting.useLayerFilter);
				if (newLayer != waveSetting.useLayerFilter) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Layer Filter");
					waveSetting.useLayerFilter = newLayer;
				}
				if (waveSetting.useLayerFilter) {
					for (var i = 0; i < waveSetting.matchingLayers.Count; i++) {
						var newMatch = EditorGUILayout.LayerField("Layer Match " + (i + 1), waveSetting.matchingLayers[i]);
						if (newMatch != waveSetting.matchingLayers[i]) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Layer Match");
							waveSetting.matchingLayers[i] = newMatch;
						}
					}
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(24);
					if (GUILayout.Button(new GUIContent("Add", "Click to add a Layer Match at the end"), GUILayout.Width(60))) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "add Layer Match");
						waveSetting.matchingLayers.Add(0);
						isDirty = true;
					}
					if (waveSetting.matchingLayers.Count > 1) {
						if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Layer Match"), GUILayout.Width(60))) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "remove Layer Match");
							waveSetting.matchingLayers.RemoveAt(waveSetting.matchingLayers.Count - 1);
							isDirty = true;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndToggleGroup();
				
				var newTag = EditorGUILayout.BeginToggleGroup("Tag Filter", waveSetting.useTagFilter);
				if (newTag != waveSetting.useTagFilter) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Tag Filter");
					waveSetting.useTagFilter = newTag;
				}
				if (waveSetting.useTagFilter) {
					for (var i = 0; i < waveSetting.matchingTags.Count; i++) {
						var newMatch = EditorGUILayout.TagField("Tag Match " + (i + 1), waveSetting.matchingTags[i]);
						if (newMatch != waveSetting.matchingTags[i]) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Tag Match");
							waveSetting.matchingTags[i] = newMatch;
						}
					}
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(24);
					if (GUILayout.Button(new GUIContent("Add", "Click to add a Tag Match at the end"), GUILayout.Width(60))) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "add Tag Match");
						waveSetting.matchingTags.Add("Untagged");
						isDirty = true;
					}
					if (waveSetting.matchingTags.Count > 1) {
						if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Tag Match"), GUILayout.Width(60))) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "remove Tag Match");
							waveSetting.matchingTags.RemoveAt(waveSetting.matchingLayers.Count - 1);
							isDirty = true;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndToggleGroup();
			}
			
			if (TriggeredSpawner.eventsThatCanRepeatWave.Contains(eventType)) {
				var newRepeat = EditorGUILayout.BeginToggleGroup("Repeat Wave", waveSetting.enableRepeatWave);
				if (newRepeat != waveSetting.enableRepeatWave) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Repeat Wave");
					waveSetting.enableRepeatWave = newRepeat;
				}
				if (waveSetting.enableRepeatWave) {
					var newRepeatMode = (WaveSpecifics.RepeatWaveMode) EditorGUILayout.EnumPopup("Repeat Mode", waveSetting.curWaveRepeatMode);
					if (newRepeatMode != waveSetting.curWaveRepeatMode) { 
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Repeat Mode");
						waveSetting.curWaveRepeatMode = newRepeatMode;
					}

					switch (waveSetting.curWaveRepeatMode) {
						case WaveSpecifics.RepeatWaveMode.NumberOfRepetitions:
							var newWaveRepeats = EditorGUILayout.IntSlider("Wave Repetitions", waveSetting.maxRepeats, 2, 100);
							if (newWaveRepeats != waveSetting.maxRepeats) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "change Wave Repetitions");
								waveSetting.maxRepeats = newWaveRepeats;
							}
							break;
						case WaveSpecifics.RepeatWaveMode.UntilWorldVariableAbove:
						case WaveSpecifics.RepeatWaveMode.UntilWorldVariableBelow:
							var missingStatNames = new List<string>();
							missingStatNames.AddRange(allStatNames);
							missingStatNames.RemoveAll(delegate(string obj) {
								return waveSetting.repeatPassCriteria.HasKey(obj);
							});
							
							var newStat = EditorGUILayout.Popup("Add Variable Limit", 0, missingStatNames.ToArray());
							if (newStat != 0) {
								AddStatModifier(missingStatNames[newStat], waveSetting);
							}

							if (waveSetting.repeatPassCriteria.statMods.Count == 0) {
								DTInspectorUtility.ShowColorWarning("*You have no Variable Limits. Wave will not repeat.");
							} else {
								EditorGUILayout.Separator();
								
								int? indexToDelete = null;
								
								for (var i = 0; i < waveSetting.repeatPassCriteria.statMods.Count; i++) {
									var modifier = waveSetting.repeatPassCriteria.statMods[i];
									EditorGUILayout.BeginHorizontal();
									GUILayout.Space(35);
									GUILayout.Label(modifier.statName, GUILayout.MaxWidth(120));
				 
									GUILayout.Space(19);
									GUILayout.Label(waveSetting.curWaveRepeatMode == WaveSpecifics.RepeatWaveMode.UntilWorldVariableAbove ? "Min" : "Max");
									
									var newValue = EditorGUILayout.IntField(modifier.modValue, GUILayout.MaxWidth(70));
									if (newValue != modifier.modValue) {
										UndoHelper.RecordObjectPropertyForUndo(settings, "change Modifier value");
										modifier.modValue = newValue;
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
									UndoHelper.RecordObjectPropertyForUndo(settings, "delete Modifier");
									waveSetting.repeatPassCriteria.DeleteByIndex(indexToDelete.Value);
								}
								
								EditorGUILayout.Separator();
							}
							
							break;
					}

					var newPause = EditorGUILayout.Slider("Pause Before Repeat", waveSetting.repeatWavePauseTime, .1f, 20);
					if (newPause != waveSetting.repeatWavePauseTime) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Pause Before Repeat");
						waveSetting.repeatWavePauseTime = newPause;
					}

					var newIncrease = EditorGUILayout.IntSlider("Spawn Increase", waveSetting.repeatItemIncrease, -50, 50);
					if (newIncrease != waveSetting.repeatItemIncrease) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Spawn Increase");
						waveSetting.repeatItemIncrease = newIncrease;
					}

					var newItemLimit = EditorGUILayout.IntSlider("Spawn Limit", waveSetting.repeatItemLimit, 1, 1000);
					if (newItemLimit != waveSetting.repeatItemLimit) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Spawn Limit");
						waveSetting.repeatItemLimit = newItemLimit;
					}

					var newTimeIncrease = EditorGUILayout.Slider("Time Increase", waveSetting.repeatTimeIncrease, -10f, 10f);
					if (newTimeIncrease != waveSetting.repeatTimeIncrease) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Time Increase");
						waveSetting.repeatTimeIncrease = newTimeIncrease;
					}
                    
					var newTimeLimit = EditorGUILayout.Slider("Time Limit", waveSetting.repeatTimeLimit, .1f, 500f);
					if (newTimeLimit != waveSetting.repeatTimeLimit) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Time Limit");
						waveSetting.repeatTimeLimit = newTimeLimit;
					}
				}
				EditorGUILayout.EndToggleGroup();
			}
			
			// show randomizations
			var variantTag = "Randomization";

			var newRand = EditorGUILayout.BeginToggleGroup(variantTag, waveSetting.enableRandomizations);
			if (newRand != waveSetting.enableRandomizations) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Randomization");
				waveSetting.enableRandomizations = newRand;
			}
			if (waveSetting.enableRandomizations) {
				EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
	            EditorGUILayout.LabelField("Random Rotation");

				var newRandX = GUILayout.Toggle(waveSetting.randomXRotation, "X");
				if (newRandX != waveSetting.randomXRotation) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Random X Rotation");
					waveSetting.randomXRotation = newRandX;
				}
				GUILayout.Space(10);
				var newRandY = GUILayout.Toggle(waveSetting.randomYRotation, "Y");
				if (newRandY != waveSetting.randomYRotation) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Random Y Rotation");
					waveSetting.randomYRotation = newRandY;
				}
				GUILayout.Space(10);
				var newRandZ = GUILayout.Toggle(waveSetting.randomZRotation, "Z");
				if (newRandZ != waveSetting.randomZRotation) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Random Z Rotation");
					waveSetting.randomZRotation = newRandZ;
				}
	            EditorGUILayout.EndHorizontal();
				
				if (waveSetting.randomXRotation) {
					var newMinX = EditorGUILayout.Slider("Rand. X Rot. Min", waveSetting.randomXRotationMin, 0, 360f);
					if (newMinX != waveSetting.randomXRotationMin) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Random X Rotation Min");
						waveSetting.randomXRotationMin = newMinX;
					}

					var newMaxX = EditorGUILayout.Slider("Rand. X Rot. Max", waveSetting.randomXRotationMax, 0, 360f);
					if (newMaxX != waveSetting.randomXRotationMax) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Random X Rotation Max");
						waveSetting.randomXRotationMax = newMaxX;
					}

					waveSetting.randomXRotationMax = Mathf.Max(waveSetting.randomXRotationMin, waveSetting.randomXRotationMax);
				}
				if (waveSetting.randomYRotation) {
					var newMinY = EditorGUILayout.Slider("Rand. Y Rot. Min", waveSetting.randomYRotationMin, 0, 360f);
					if (newMinY != waveSetting.randomYRotationMin) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Y Rotation Min");
						waveSetting.randomYRotationMin = newMinY;
					}

					var newMaxY = EditorGUILayout.Slider("Rand. Y Rot. Max", waveSetting.randomYRotationMax, 0, 360f);
					if (newMaxY != waveSetting.randomYRotationMax) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Y Rotation Max");
						waveSetting.randomYRotationMax = newMaxY;
					}

					waveSetting.randomYRotationMax = Mathf.Max(waveSetting.randomYRotationMin, waveSetting.randomYRotationMax);
				}
				if (waveSetting.randomZRotation) {
					var newMinZ = EditorGUILayout.Slider("Rand. Z Rot. Min", waveSetting.randomZRotationMin, 0, 360f);
					if (newMinZ != waveSetting.randomZRotationMin) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Z Rotation Min");
						waveSetting.randomZRotationMin = newMinZ;
					}

					var newMaxZ = EditorGUILayout.Slider("Rand. Z Rot. Max", waveSetting.randomZRotationMax, 0, 360f);
					if (newMaxZ != waveSetting.randomZRotationMax) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Z Rotation Max");
						waveSetting.randomZRotationMax = newMaxZ;
					}

					waveSetting.randomZRotationMax = Mathf.Max(waveSetting.randomZRotationMin, waveSetting.randomZRotationMax);
				}
					
				EditorGUILayout.Separator();

				var newRandDistX = EditorGUILayout.Slider("Rand. Distance X", waveSetting.randomDistanceX, 0, MAX_DISTANCE);
				if (newRandDistX != waveSetting.randomDistanceX) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Distance X");
					waveSetting.randomDistanceX = newRandDistX;
				}

				var newRandDistY = EditorGUILayout.Slider("Rand. Distance Y", waveSetting.randomDistanceY, 0, MAX_DISTANCE);
				if (newRandDistY != waveSetting.randomDistanceY) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Distance Y");
					waveSetting.randomDistanceY = newRandDistY;
				}

				var newRandDistZ = EditorGUILayout.Slider("Rand. Distance Z", waveSetting.randomDistanceZ, 0, MAX_DISTANCE);
				if (newRandDistZ != waveSetting.randomDistanceZ) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "change Random Distance Z");
					waveSetting.randomDistanceZ = newRandDistZ;
				}
			}
			EditorGUILayout.EndToggleGroup();
		
			
			// show increments
			var incTag = "Incremental Settings";
			var newIncrements = EditorGUILayout.BeginToggleGroup(incTag, waveSetting.enableIncrements);
			if (newIncrements != waveSetting.enableIncrements) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Incremental Settings");
				waveSetting.enableIncrements = newIncrements;
			}
			if (waveSetting.enableIncrements) {
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
				
				if (waveSetting.enableRandomizations && waveSetting.randomXRotation) {
					DTInspectorUtility.ShowColorWarning("*Rotation X - cannot be used with Random Rotation X.");
				} else {
					var newIncRotX = EditorGUILayout.Slider("Rotation X", waveSetting.incrementRotationX, -180f, 180f);
					if (newIncRotX != waveSetting.incrementRotationX) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Rotation X");
						waveSetting.incrementRotationX = newIncRotX;
					}
				}

				if (waveSetting.enableRandomizations && waveSetting.randomYRotation) {
					DTInspectorUtility.ShowColorWarning("*Rotation Y - cannot be used with Random Rotation Y.");
				} else {
					var newIncRotY = EditorGUILayout.Slider("Rotation Y", waveSetting.incrementRotationY, -180f, 180f);
					if (newIncRotY != waveSetting.incrementRotationY) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Rotation Y");
						waveSetting.incrementRotationY = newIncRotY;
					}
				}

				if (waveSetting.enableRandomizations && waveSetting.randomZRotation) {
					DTInspectorUtility.ShowColorWarning("*Rotation Z - cannot be used with Random Rotation Z.");
				} else {
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
			var newPostEnabled = EditorGUILayout.BeginToggleGroup(incTag, waveSetting.enablePostSpawnNudge);
			if (newPostEnabled != waveSetting.enablePostSpawnNudge) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Post-spawn Nudge Settings");
				waveSetting.enablePostSpawnNudge = newPostEnabled;
			}
			if (waveSetting.enablePostSpawnNudge) {
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
		}
		
		return isDirty;
	}

	private List<string> getUnusedEventTypes() {
		var unusedEvents = new List<string>();
		unusedEvents.Add("-None-");
		if (!settings.enableWave.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.OnEnabled.ToString());
		}
		if (!settings.disableWave.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.OnDisabled.ToString());
		}
		if (!settings.visibleWave.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.Visible.ToString());
		}
		if (!settings.invisibleWave.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.Invisible.ToString());
		}
		if (!settings.mouseOverWave.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.MouseOver.ToString());
		}
		if (!settings.mouseClickWave.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.MouseClick.ToString());
		}
		if (!settings.collisionWave.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.OnCollision.ToString());
		}
		if (!settings.triggerEnterWave.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.OnTriggerEnter.ToString());
		}
		if (!settings.triggerExitWave.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.OnTriggerExit.ToString());
		}
		if (!settings.codeTriggeredWave1.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.CodeTriggered1.ToString());
		}
		if (!settings.codeTriggeredWave2.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.CodeTriggered2.ToString());
		}
		if (!settings.spawnedWave.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.OnSpawned.ToString());
		}
		if (!settings.despawnedWave.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.OnDespawned.ToString());
		}
		if (!settings.clickWave.enableWave) {
			unusedEvents.Add(TriggeredSpawner.EventType.OnClick.ToString());
		}

		#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		// not supported
		#else
			if (!settings.collision2dWave.enableWave) {
				unusedEvents.Add(TriggeredSpawner.EventType.OnCollision2D.ToString());
			}
			if (!settings.triggerEnter2dWave.enableWave) {
				unusedEvents.Add(TriggeredSpawner.EventType.OnTriggerEnter2D.ToString());
			}
			if (!settings.triggerExit2dWave.enableWave) {
				unusedEvents.Add(TriggeredSpawner.EventType.OnTriggerExit2D.ToString());
			}
		#endif

		return unusedEvents;
	}

	private void ActivateEvent(int index, List<string> unusedEvents) {
		var item = unusedEvents[index];
		var eventType = (TriggeredSpawner.EventType) Enum.Parse(typeof(TriggeredSpawner.EventType), item);
	
		UndoHelper.RecordObjectPropertyForUndo(settings, "activate Event");

		switch (eventType) {
			case TriggeredSpawner.EventType.CodeTriggered1:
				settings.codeTriggeredWave1.enableWave = true;
				break;
			case TriggeredSpawner.EventType.CodeTriggered2:
				settings.codeTriggeredWave2.enableWave = true;
				break;
			case TriggeredSpawner.EventType.Invisible:
				settings.invisibleWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.MouseClick:
				settings.mouseClickWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.MouseOver:
				settings.mouseOverWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.OnClick:
				settings.clickWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.OnCollision:
				settings.collisionWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.OnDespawned:
				settings.despawnedWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.OnDisabled:
				settings.disableWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.OnEnabled:
				settings.enableWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.OnSpawned:
				settings.spawnedWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.OnTriggerEnter:
				settings.triggerEnterWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.OnTriggerExit:
				settings.triggerExitWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.Visible:
				settings.visibleWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.OnCollision2D:
				settings.collision2dWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.OnTriggerEnter2D:
				settings.triggerEnter2dWave.enableWave = true;
				break;
			case TriggeredSpawner.EventType.OnTriggerExit2D:
				settings.triggerExit2dWave.enableWave = true;
				break;
		}
	}

	private void AddStatModifier(string modifierName, TriggeredWaveSpecifics spec) {
		if (spec.repeatPassCriteria.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This wave already has a Variable Limit for World Variable: " + modifierName + ". Please modify the existing one instead.");
			return;
		}
	
		UndoHelper.RecordObjectPropertyForUndo(settings, "add Variable Limit");

		spec.repeatPassCriteria.statMods.Add(new WorldVariableModifier() {
			statName = modifierName
		});
	}

	private void AddActiveLimit(string modifierName, TriggeredSpawner spec) {
		if (spec.activeItemCriteria.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This item already has a Active Limit for World Variable: " + modifierName + ". Please modify the existing one instead.");
			return;
		}

		UndoHelper.RecordObjectPropertyForUndo(settings, "add Active Limit");

		spec.activeItemCriteria.statMods.Add(new WorldVariableRange() {
			statName = modifierName
		});
	}
	
	private static List<string> PoolNames {
		get {
			return LevelSettings.GetSortedPrefabPoolNames();
		}
	}
}
