using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Killable inspector.
/// 3 Steps to make a subclass Inspector (if you're not on Unity 4).
/// 
/// 1) Duplicate the KillableInspector file (this one). Open it.
/// 2) Change "Killable" on line 16 and line 18 to the name of your Killable subclass. Also change the 2 instances of "Killable" on line 25 to the same.
/// 3) Change the "KillableInspector" on line 20 to your Killable subclass + "Inspector". Also change the filename to the same.
/// </summary>

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
	[CustomEditor(typeof(Killable))]
#else
	[CustomEditor(typeof(Killable), true)]
#endif
public class KillableInspector : Editor {
	private Killable kill;

	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		EditorGUI.indentLevel = 0;
		
		kill = (Killable) target;
		Transform statsHolder = null;
		var allStatNames = new List<string>() {
			DTInspectorUtility.DROP_DOWN_NONE_OPTION
		};
		
		var ls = LevelSettings.GetLevelSettings;
		if (ls != null) {
			DTInspectorUtility.DrawTexture(ls.logoTexture);
			statsHolder = ls.transform.Find(LevelSettings.PLAYER_STATS_CONTAINER_TRANS_NAME);
			for (var i = 0; i < statsHolder.childCount; i++) {
				var child = statsHolder.GetChild(i);
				allStatNames.Add(child.name);
			}
		}
		
		bool isDirty = false;

		var newHP = EditorGUILayout.IntSlider("Hit Points", kill.HitPoints, 1, Killable.MAX_HIT_POINTS);
		if (newHP != kill.HitPoints) {
			UndoHelper.RecordObjectPropertyForUndo(kill, "change Hit Points");
			kill.HitPoints = newHP;
		}

		var newAP = EditorGUILayout.IntSlider("Attack Points", kill.AttackPoints, 0, Killable.MAX_ATTACK_POINTS);
		if (newAP != kill.AttackPoints) {
			UndoHelper.RecordObjectPropertyForUndo(kill, "change Attack Points");
			kill.AttackPoints = newAP;
		}
		if (Application.isPlaying) {
			kill.currentHitPoints = EditorGUILayout.IntSlider("Remaining Hit Points", kill.currentHitPoints, 0, Killable.MAX_ATTACK_POINTS);
		}

		var newIgnore = EditorGUILayout.Toggle("Ignore Offscreen Hits", kill.ignoreOffscreenHits);
		if (newIgnore != kill.ignoreOffscreenHits) {
			UndoHelper.RecordObjectPropertyForUndo(kill, "toggle Ignore Offscreen Hits");
			kill.ignoreOffscreenHits = newIgnore;
		}

		var newExplosion = (Transform) EditorGUILayout.ObjectField("Explosion Prefab", kill.ExplosionPrefab, typeof(Transform), true);
		if (newExplosion != kill.ExplosionPrefab) {
			UndoHelper.RecordObjectPropertyForUndo(kill, "change Explosion Prefab");
			kill.ExplosionPrefab = newExplosion;
		}

		// retrigger limit section
		var newLimitMode = (TriggeredSpawner.RetriggerLimitMode) EditorGUILayout.EnumPopup("Retrigger Limit Mode", kill.retriggerLimitMode);
		if (newLimitMode != kill.retriggerLimitMode) {
			UndoHelper.RecordObjectPropertyForUndo(kill, "change Retrigger Limit Mode");
			kill.retriggerLimitMode = newLimitMode;
		}
		switch (kill.retriggerLimitMode) {
			case TriggeredSpawner.RetriggerLimitMode.FrameBased:
				var newFrame = EditorGUILayout.IntSlider("Min Frames Between", kill.limitPerXFrames, 1, 120);
				if (newFrame != kill.limitPerXFrames) {
					UndoHelper.RecordObjectPropertyForUndo(kill, "change Min Frames Between");
					kill.limitPerXFrames = newFrame;
				}
				break;
			case TriggeredSpawner.RetriggerLimitMode.TimeBased:
				var newSeconds = EditorGUILayout.Slider("Min Seconds Between", kill.limitPerXSeconds, 0.1f, 10f);
				if (newSeconds != kill.limitPerXSeconds) {
					UndoHelper.RecordObjectPropertyForUndo(kill, "change Min Seconds Between");
					kill.limitPerXSeconds = newSeconds;
				}
				break;
		}

		var newGO = (TriggeredSpawner.GameOverBehavior) EditorGUILayout.EnumPopup("Game Over Behavior", kill.gameOverBehavior);
		if (newGO != kill.gameOverBehavior) {
			UndoHelper.RecordObjectPropertyForUndo(kill, "change Game Over Behavior");
			kill.gameOverBehavior = newGO;
		}

		var newLog = EditorGUILayout.Toggle("Log Events", kill.enableLogging);
		if (newLog != kill.enableLogging) {
			UndoHelper.RecordObjectPropertyForUndo(kill, "toggle Log Events");
			kill.enableLogging = newLog;
		}
		
		var hadNoListener = kill.listener == null;
		var newListener = (KillableListener) EditorGUILayout.ObjectField("Listener", kill.listener, typeof(KillableListener), true);
		if (newListener != kill.listener) {
			UndoHelper.RecordObjectPropertyForUndo(kill, "assign Listener");
			kill.listener = newListener;
			if (hadNoListener && kill.listener != null) {
				kill.listener.sourceKillableName = kill.transform.name;
			}
		}

		EditorGUILayout.Separator();
		
		// layer / tag / limit filters
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

		var newExpanded = DTInspectorUtility.Foldout(kill.filtersExpanded, "Layer and Tag filters");
		if (newExpanded != kill.filtersExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(kill, "toggle expand Layer and Tag filters");
			kill.filtersExpanded = newExpanded;
		}
		EditorGUILayout.EndHorizontal();
		
		if (kill.filtersExpanded) {
			EditorGUI.indentLevel = 0;
			var newUseLayer = EditorGUILayout.BeginToggleGroup("Layer Filter", kill.useLayerFilter);
			if (newUseLayer != kill.useLayerFilter) {
				UndoHelper.RecordObjectPropertyForUndo(kill, "toggle Layer Filter");
				kill.useLayerFilter = newUseLayer;
			}
			if (kill.useLayerFilter) {
				for (var i = 0; i < kill.matchingLayers.Count; i++) {
					var newLayer = EditorGUILayout.LayerField("Layer Match " + (i + 1), kill.matchingLayers[i]);
					if (newLayer != kill.matchingLayers[i]) {
						UndoHelper.RecordObjectPropertyForUndo(kill, "change Layer Match");
						kill.matchingLayers[i] = newLayer;
					}
				}
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(24);
				if (GUILayout.Button(new GUIContent("Add", "Click to add a Layer Match at the end"), GUILayout.Width(60))) {
					UndoHelper.RecordObjectPropertyForUndo(kill, "add Layer Match");
					kill.matchingLayers.Add(0);
					isDirty = true;
				}
				if (kill.matchingLayers.Count > 1) {
					if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Layer Match"), GUILayout.Width(60))) {
						UndoHelper.RecordObjectPropertyForUndo(kill, "remove Layer Match");
						kill.matchingLayers.RemoveAt(kill.matchingLayers.Count - 1);
						isDirty = true;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndToggleGroup();

			newExpanded = EditorGUILayout.BeginToggleGroup("Tag Filter", kill.useTagFilter);
			if (newExpanded != kill.useTagFilter) {
				UndoHelper.RecordObjectPropertyForUndo(kill, "toggle Tag Filter");
				kill.useTagFilter = newExpanded;
			}
			if (kill.useTagFilter) {
				for (var i = 0; i < kill.matchingTags.Count; i++) {
					var newTag = EditorGUILayout.TagField("Tag Match " + (i + 1), kill.matchingTags[i]);
					if (newTag != kill.matchingTags[i]) {
						UndoHelper.RecordObjectPropertyForUndo(kill, "change Tag Match");
						kill.matchingTags[i] = newTag;
					}
				}
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(24);
				if (GUILayout.Button(new GUIContent("Add", "Click to add a Tag Match at the end"), GUILayout.Width(60))) {
					UndoHelper.RecordObjectPropertyForUndo(kill, "add Tag Match");
					kill.matchingTags.Add("Untagged");
					isDirty = true;
				}
				if (kill.matchingTags.Count > 1) {
					if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Tag Match"), GUILayout.Width(60))) {
						UndoHelper.RecordObjectPropertyForUndo(kill, "remove Tag Match");
						kill.matchingTags.RemoveAt(kill.matchingLayers.Count - 1);
						isDirty = true;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndToggleGroup();
		}
		
		// despawn trigger section
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		newExpanded = DTInspectorUtility.Foldout(kill.showVisibilitySettings, "Despawn Triggers");
		if (newExpanded != kill.showVisibilitySettings) {
			UndoHelper.RecordObjectPropertyForUndo(kill, "toggle expand Despawn Triggers");
			kill.showVisibilitySettings = newExpanded;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel = 0;
		if (kill.showVisibilitySettings) {
			var newOffscreen = EditorGUILayout.Toggle("Invisible Event", kill.despawnWhenOffscreen);
			if (newOffscreen != kill.despawnWhenOffscreen) {
				UndoHelper.RecordObjectPropertyForUndo(kill, "toggle Invisible Event");
				kill.despawnWhenOffscreen = newOffscreen;
			}

			var newNotVisible = EditorGUILayout.Toggle("Not Visible Too Long", kill.despawnIfNotVisible);
			if (newNotVisible != kill.despawnIfNotVisible) {
				UndoHelper.RecordObjectPropertyForUndo(kill, "toggle Not Visible Too Long");
				kill.despawnIfNotVisible = newNotVisible;
			}

			if (kill.despawnIfNotVisible) {
				var notVisibleTime = EditorGUILayout.Slider("Not Visible Max Time", kill.despawnIfNotVisibleForTime, .1f, 15f);
				if (notVisibleTime != kill.despawnIfNotVisibleForTime) {
					UndoHelper.RecordObjectPropertyForUndo(kill, "change Not Visible Max Time");
					kill.despawnIfNotVisibleForTime = notVisibleTime;
				}
			}

			var newClick = EditorGUILayout.Toggle("OnClick Event", kill.despawnOnClick);
			if (newClick != kill.despawnOnClick) {
				UndoHelper.RecordObjectPropertyForUndo(kill, "toggle OnClick Event");
				kill.despawnOnClick = newClick;
			}

			var newDespawn = (Killable.DespawnMode) EditorGUILayout.EnumPopup("HP Despawn Mode", kill.despawnMode);
			if (newDespawn != kill.despawnMode) {
				UndoHelper.RecordObjectPropertyForUndo(kill, "change HP Despawn Mode");
				kill.despawnMode = newDespawn;
			}
		}
		
		var poolNames = LevelSettings.GetSortedPrefabPoolNames();
		
		// damage prefab section
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		newExpanded = DTInspectorUtility.Foldout(kill.damagePrefabExpanded, "Damage Prefab Settings");
		if (newExpanded != kill.damagePrefabExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(kill, "toggle expand Damage Prefab Settings");
			kill.damagePrefabExpanded = newExpanded;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel = 0;
		if (kill.damagePrefabExpanded) {
			var newSpawnMode = (Killable.DamagePrefabSpawnMode) EditorGUILayout.EnumPopup("Spawn Frequency", kill.damagePrefabSpawnMode);
			if (newSpawnMode != kill.damagePrefabSpawnMode) {
				UndoHelper.RecordObjectPropertyForUndo(kill, "change Spawn Frequency");
				kill.damagePrefabSpawnMode = newSpawnMode;
			}

			if (kill.damagePrefabSpawnMode != Killable.DamagePrefabSpawnMode.None) {
				if (kill.damagePrefabSpawnMode == Killable.DamagePrefabSpawnMode.PerGroupHitPointsLost) {
					var newDmgGroup = EditorGUILayout.IntSlider("Group H.P. Amount", kill.damageGroupSize, 1, 50);
					if (newDmgGroup != kill.damageGroupSize) {
						UndoHelper.RecordObjectPropertyForUndo(kill, "change Group H.P. Amount");
						kill.damageGroupSize = newDmgGroup;
					}
				}

				var newDmgQty = EditorGUILayout.IntSlider("Spawn Quantity", kill.damagePrefabSpawnQty, 1, 20);
				if (newDmgQty != kill.damagePrefabSpawnQty) {
					UndoHelper.RecordObjectPropertyForUndo(kill, "change Spawn Quantity");
					kill.damagePrefabSpawnQty = newDmgQty;
				}

				var newDmgSource = (Killable.SpawnSource) EditorGUILayout.EnumPopup("Damage Prefab Type", kill.damagePrefabSource);
				if (newDmgSource != kill.damagePrefabSource) {
					UndoHelper.RecordObjectPropertyForUndo(kill, "change Damage Prefab Type");
					kill.damagePrefabSource = newDmgSource;
				}
				switch (kill.damagePrefabSource) {
					case Killable.SpawnSource.PrefabPool:
						if (poolNames != null) {
							var pool = LevelSettings.GetFirstMatchingPrefabPool(kill.damagePrefabPoolName);
							if (pool == null) {
								if (string.IsNullOrEmpty(kill.damagePrefabPoolName)) {
									DTInspectorUtility.ShowColorWarning("*No Damage Prefab Pool selected.");
								} else {
									DTInspectorUtility.ShowColorWarning("*Damage Prefab Pool '" + kill.damagePrefabPoolName + "' not found. Select one.");
								}
								kill.damagePrefabPoolIndex = 0;
							} else {
								kill.damagePrefabPoolIndex = poolNames.IndexOf(kill.damagePrefabPoolName);
							}
	
							if (poolNames.Count > 0) {
								var newPoolIndex = EditorGUILayout.Popup("Damage Prefab Pool", kill.damagePrefabPoolIndex, poolNames.ToArray());
								if (newPoolIndex != kill.damagePrefabPoolIndex) {
									UndoHelper.RecordObjectPropertyForUndo(kill, "change Damage Prefab Pool");
									kill.damagePrefabPoolIndex = newPoolIndex;
								}
							
								if (kill.damagePrefabPoolIndex > 0) {						
									var matchingPool = 	LevelSettings.GetFirstMatchingPrefabPool(poolNames[kill.damagePrefabPoolIndex]);
									if (matchingPool != null) {	
										kill.damagePrefabPoolName = matchingPool.name;
									} 
								} else {
									kill.damagePrefabPoolName = string.Empty;
								}
							} else {
								DTInspectorUtility.ShowColorWarning("*You have no Prefab Pools. Create one first.");
							}
						} else {
							DTInspectorUtility.ShowColorWarning(LevelSettings.NO_PREFAB_POOLS_CONTAINER_ALERT);
							DTInspectorUtility.ShowColorWarning(LevelSettings.REVERT_LEVEL_SETTINGS_ALERT);
						}

						break;
					case Killable.SpawnSource.Specific:
						var newSpecific = (Transform) EditorGUILayout.ObjectField("Damage Prefab", kill.damagePrefabSpecific, typeof(Transform), true);
						if (newSpecific != kill.damagePrefabSpecific) {
							UndoHelper.RecordObjectPropertyForUndo(kill, "change Damage Prefab");
							kill.damagePrefabSpecific = newSpecific;
						}
						if (kill.damagePrefabSpecific == null) {
							DTInspectorUtility.ShowColorWarning("*Please assign a Damage prefab.");
						}
						break;
				}

				EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
	            EditorGUILayout.LabelField("Random Rotation");

				var newRandomX = GUILayout.Toggle(kill.damagePrefabRandomizeXRotation, "X");
				if (newRandomX != kill.damagePrefabRandomizeXRotation) {
					UndoHelper.RecordObjectPropertyForUndo(kill, "toggle Random X Rotation");
					kill.damagePrefabRandomizeXRotation = newRandomX;
				}
				GUILayout.Space(10);
				var newRandomY = GUILayout.Toggle(kill.damagePrefabRandomizeYRotation, "Y");
				if (newRandomY != kill.damagePrefabRandomizeYRotation) {
					UndoHelper.RecordObjectPropertyForUndo(kill, "toggle Random Y Rotation");
					kill.damagePrefabRandomizeYRotation = newRandomY;
				}
				GUILayout.Space(10);
				var newRandomZ = GUILayout.Toggle(kill.damagePrefabRandomizeZRotation, "Z");
				if (newRandomZ != kill.damagePrefabRandomizeZRotation) {
					UndoHelper.RecordObjectPropertyForUndo(kill, "toggle Random Z Rotation");
					kill.damagePrefabRandomizeZRotation = newRandomZ;
				}
	            EditorGUILayout.EndHorizontal();
				
			} else {
				DTInspectorUtility.ShowColorWarning("Change Spawn Frequency to show more settings.");
			}
		}		
		
		// death prefab section
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		newExpanded = DTInspectorUtility.Foldout(kill.deathPrefabSettingsExpanded, "Death Prefab Settings");
		if (newExpanded != kill.deathPrefabSettingsExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(kill, "toggle expand Death Prefab Settings");
			kill.deathPrefabSettingsExpanded = newExpanded;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel = 0;
		if (kill.deathPrefabSettingsExpanded) {
			var newDeathSource = (WaveSpecifics.SpawnOrigin) EditorGUILayout.EnumPopup("Death Prefab Type", kill.deathPrefabSource);
			if (newDeathSource != kill.deathPrefabSource) {
				UndoHelper.RecordObjectPropertyForUndo(kill, "change Death Prefab Type");
				kill.deathPrefabSource = newDeathSource;
			}
			switch (kill.deathPrefabSource) {
				case WaveSpecifics.SpawnOrigin.PrefabPool:
					if (poolNames != null) {
						var pool = LevelSettings.GetFirstMatchingPrefabPool(kill.deathPrefabPoolName);
						if (pool == null) {
							if (string.IsNullOrEmpty(kill.deathPrefabPoolName)) {
								DTInspectorUtility.ShowColorWarning("*No Death Prefab Pool selected.");
							} else {
								DTInspectorUtility.ShowColorWarning("*Death Prefab Pool '" + kill.deathPrefabPoolName + "' not found. Select one.");
							}
							kill.deathPrefabPoolIndex = 0;
						} else {
							kill.deathPrefabPoolIndex = poolNames.IndexOf(kill.deathPrefabPoolName);
						}

						if (poolNames.Count > 0) {
							var newDeathPool = EditorGUILayout.Popup("Death Prefab Pool", kill.deathPrefabPoolIndex, poolNames.ToArray());
							if (newDeathPool != kill.deathPrefabPoolIndex) {
								UndoHelper.RecordObjectPropertyForUndo(kill, "change Death Prefab Pool");
								kill.deathPrefabPoolIndex = newDeathPool;
							}
						
							if (kill.deathPrefabPoolIndex > 0) {						
								var matchingPool = 	LevelSettings.GetFirstMatchingPrefabPool(poolNames[kill.deathPrefabPoolIndex]);
								if (matchingPool != null) {	
									kill.deathPrefabPoolName = matchingPool.name;
								}
							} else {
								kill.deathPrefabPoolName = string.Empty;
							}
						} else {
							DTInspectorUtility.ShowColorWarning("*You have no Prefab Pools. Create one first.");
						}
					} else {
						DTInspectorUtility.ShowColorWarning(LevelSettings.NO_PREFAB_POOLS_CONTAINER_ALERT);
						DTInspectorUtility.ShowColorWarning(LevelSettings.REVERT_LEVEL_SETTINGS_ALERT);
					}
					break;
				case WaveSpecifics.SpawnOrigin.Specific:
					var newDeathSpecific = (Transform) EditorGUILayout.ObjectField("Death Prefab", kill.deathPrefabSpecific, typeof(Transform), true);
					if (newDeathSpecific != kill.deathPrefabSpecific) {
						UndoHelper.RecordObjectPropertyForUndo(kill, "change Death Prefab");
						kill.deathPrefabSpecific = newDeathSpecific;
					}
					break;
			}

			var newPercent = EditorGUILayout.IntSlider("Spawn % Chance", kill.deathPrefabSpawnPercentage, 0, 100);			
			if (newPercent != kill.deathPrefabSpawnPercentage) {
				UndoHelper.RecordObjectPropertyForUndo(kill, "change Spawn % Chance");
				kill.deathPrefabSpawnPercentage = newPercent;
			}

			var newDeathQty = EditorGUILayout.IntSlider("Spawn Quantity", kill.deathPrefabQuantity, 1, 50);			
			if (newDeathQty != kill.deathPrefabQuantity) {
				UndoHelper.RecordObjectPropertyForUndo(kill, "change Spawn Quantity");
				kill.deathPrefabQuantity = newDeathQty;
			}

			if (kill.rigidbody == null || kill.rigidbody.isKinematic) {
				DTInspectorUtility.ShowColorWarning("*Inherit Velocity can only be used on gravity rigidbodies");
			} else {
				var newKeep = EditorGUILayout.Toggle("Inherit Velocity", kill.deathPrefabKeepVelocity);
				if (newKeep != kill.deathPrefabKeepVelocity) {
					UndoHelper.RecordObjectPropertyForUndo(kill, "toggle Inherit Velocity");
					kill.deathPrefabKeepVelocity = newKeep;
				}
			}

			var newMode = (Killable.RotationMode) EditorGUILayout.EnumPopup("Rotation Mode", kill.rotationMode);
			if (newMode != kill.rotationMode) {
				UndoHelper.RecordObjectPropertyForUndo(kill, "change Rotation Mode");
				kill.rotationMode = newMode;
			}
			if (kill.rotationMode == Killable.RotationMode.CustomRotation) {
				var newCustomRot = EditorGUILayout.Vector3Field("Custom Rotation Euler", kill.deathPrefabCustomRotation);
				if (newCustomRot != kill.deathPrefabCustomRotation) {
					UndoHelper.RecordObjectPropertyForUndo(kill, "change Custom Rotation Euler");
					kill.deathPrefabCustomRotation = newCustomRot;
				}
			}
		}
		
		// player stat modifiers
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		newExpanded = DTInspectorUtility.Foldout(kill.despawnStatModifiersExpanded, "World Variable Modifier Scenarios");
		if (newExpanded != kill.despawnStatModifiersExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(kill, "toggle expand World Variable Modifier Scenarios");
			kill.despawnStatModifiersExpanded = newExpanded;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel = 0;
		if (kill.despawnStatModifiersExpanded) {
			EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
			EditorGUILayout.LabelField("If \"" + Killable.DESTROYED_TEXT + "\"");
			GUI.backgroundColor = Color.green;
			if (GUILayout.Button(new GUIContent("Add Else"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(80))) {
				AddModifierElse(kill);
			}
			GUI.backgroundColor = Color.white;
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel = 0;
			
			var missingStatNames = new List<string>();
			missingStatNames.AddRange(allStatNames);
			missingStatNames.RemoveAll(delegate(string obj) {
				return kill.playerStatDespawnModifiers.HasKey(obj);	
			});
			
			var newStat = EditorGUILayout.Popup("Add Variable Modifer", 0, missingStatNames.ToArray());
			if (newStat != 0) {
				AddStatModifier(missingStatNames[newStat], kill.playerStatDespawnModifiers);
			}
			
			if (kill.playerStatDespawnModifiers.statMods.Count == 0) {
				DTInspectorUtility.ShowColorWarning("*You currently are using no modifiers for this prefab.");
			} else {
				EditorGUILayout.Separator();
				
				int? indexToDelete = null;
				
				for (var i = 0; i < kill.playerStatDespawnModifiers.statMods.Count; i++) {
					var modifier = kill.playerStatDespawnModifiers.statMods[i];
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(13);
					GUILayout.Label(modifier.statName, GUILayout.MaxWidth(100));
 
					GUILayout.Space(19);
					var newVal = EditorGUILayout.IntField(modifier.modValue, GUILayout.MaxWidth(70));
					if (newVal != modifier.modValue) {
						UndoHelper.RecordObjectPropertyForUndo(kill, "change Modifier value");
						modifier.modValue = newVal;
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
					UndoHelper.RecordObjectPropertyForUndo(kill, "delete Modifier");
					kill.playerStatDespawnModifiers.DeleteByIndex(indexToDelete.Value);
				}
				
				EditorGUILayout.Separator();
			}

			// alternate cases
			WorldVariableCollection alternate = null;
			int? iElseToDelete = null;
			for (var i = 0; i < kill.alternateModifiers.Count; i++) {
				alternate = kill.alternateModifiers[i];
				
				EditorGUI.indentLevel = 0;
				EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
				GUILayout.Label("Else If", GUILayout.Width(40));
				var newScen = EditorGUILayout.TextField(alternate.scenarioName, GUILayout.MaxWidth(150));
				if (newScen != alternate.scenarioName) {
					UndoHelper.RecordObjectPropertyForUndo(kill, "change Scenario name");
					alternate.scenarioName = newScen;
				}
				GUILayout.FlexibleSpace();
				GUI.backgroundColor = Color.green;
				if (GUILayout.Button(new GUIContent("Delete Else"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(80))) {
					iElseToDelete = i;
				}
				GUI.backgroundColor = Color.white;
				EditorGUILayout.EndHorizontal();

				EditorGUI.indentLevel = 0;
				// display modifers
				
				missingStatNames = new List<string>();
				missingStatNames.AddRange(allStatNames);
				missingStatNames.RemoveAll(delegate(string obj) {
					return alternate.HasKey(obj);	
				});				
				
				var newMod = EditorGUILayout.Popup("Add Variable Modifer", 0, missingStatNames.ToArray());
				if (newMod != 0) {
					AddStatModifier(missingStatNames[newMod], alternate);
				}
				
				if (alternate.statMods.Count == 0) {
					DTInspectorUtility.ShowColorWarning("*You currently are using no Modifiers for this prefab.");
				} else {
					EditorGUILayout.Separator();
					
					int? indexToDelete = null;
					
					for (var j = 0; j < alternate.statMods.Count; j++) {
						var modifier = alternate.statMods[j];
						EditorGUILayout.BeginHorizontal();
						GUILayout.Space(13);
						GUILayout.Label(modifier.statName, GUILayout.MaxWidth(100));
	 
						GUILayout.Space(19);
						var newVal = EditorGUILayout.IntField(modifier.modValue, GUILayout.MaxWidth(70));
						if (newVal != modifier.modValue) {
							UndoHelper.RecordObjectPropertyForUndo(kill, "change Modifier value");
							modifier.modValue = newVal;
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
						UndoHelper.RecordObjectPropertyForUndo(kill, "delete Modifier");
						alternate.DeleteByIndex(indexToDelete.Value);
					}
					
					EditorGUILayout.Separator();
				}
			}
			
			if (iElseToDelete.HasValue) {
				UndoHelper.RecordObjectPropertyForUndo(kill, "delete Scenario");
				kill.alternateModifiers.RemoveAt(iElseToDelete.Value);
			}
		} 
			
		if (GUI.changed || isDirty) {
  			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}
		
		//DrawDefaultInspector();
    }
	
	private void AddModifierElse(Killable kil) {
		UndoHelper.RecordObjectPropertyForUndo(kil, "add Else");

		kil.alternateModifiers.Add(new WorldVariableCollection());
	}
	
	private void AddStatModifier(string modifierName, WorldVariableCollection modifiers) {
		if (modifiers.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This Killable already has a modifier for World Variable: " + modifierName + ". Please modify that instead.");
			return;
		}
	
		UndoHelper.RecordObjectPropertyForUndo(kill, "add Modifier");

		modifiers.statMods.Add(new WorldVariableModifier() {
			statName = modifierName
		});
	}

}