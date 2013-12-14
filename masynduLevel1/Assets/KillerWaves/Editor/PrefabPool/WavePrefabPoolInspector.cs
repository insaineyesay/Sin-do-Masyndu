using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[CustomEditor(typeof(WavePrefabPool))]
public class WavePrefabPoolInspector : Editor {
	private Transform poolTrans;
	private WavePrefabPool settings;

	private void FindMatchingSpawners(Transform poolTrans, bool selectThem) {
		LevelSettings levSettings = (LevelSettings) GameObject.FindObjectOfType(typeof(LevelSettings));
		
		var spawners = levSettings.GetAllSpawners();
		Debug.Log(spawners.Count + " spawners");
		WaveSyncroPrefabSpawner spawnerScript = null;
		
		var matchSpawners = new List<GameObject>();
		
		StringBuilder sb = new StringBuilder();
		
		foreach (var spawner in spawners) {
			spawnerScript = spawner.GetComponent<WaveSyncroPrefabSpawner>();
			if (!spawnerScript.IsUsingPrefabPool(poolTrans)) {
				continue;
			}
			
			matchSpawners.Add(spawner.gameObject);
			if (sb.Length > 0) {
				sb.Append(", ");
			}
			sb.Append("'" + spawnerScript.name + "'");
		}
		
		if (sb.Length == 0) {
			sb.Append("~None~");
		}
		
		Debug.Log(string.Format("--- Found {0} matching spawner(s) for Prefab Pool: ({1}) ---",
			matchSpawners.Count,
			sb.ToString()));
		if (selectThem) {
			if (matchSpawners.Count > 0) {
				Selection.objects = matchSpawners.ToArray();
			} else {
				Debug.Log("No spawners use this Prefab Pool.");
			}
		}
	}
	
	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		
		settings = (WavePrefabPool)target;
		this.poolTrans = settings.transform;
			
		if (settings.logoTexture != null) {
			DTInspectorUtility.DrawTexture(settings.logoTexture);
		}
		
		var allStatNames = new List<string>() {
			DTInspectorUtility.DROP_DOWN_NONE_OPTION
		};
		
		Transform statsHolder = null;
		
		var myParent = settings.transform.parent;
		Transform levelSettingObj = null;
		LevelSettings levelSettings = null;
		
		if (myParent != null) {
			levelSettingObj = myParent.parent;
			if (levelSettingObj != null) {
				levelSettings = levelSettingObj.GetComponent<LevelSettings>();
			}
		} 
		
		statsHolder = levelSettings.transform.Find(LevelSettings.PLAYER_STATS_CONTAINER_TRANS_NAME);
		for (var i = 0; i < statsHolder.childCount; i++) {
			var child = statsHolder.GetChild(i);
			allStatNames.Add(child.name);
		}
		
		EditorGUI.indentLevel = 0;
		var newSeq = (WavePrefabPool.PoolDispersalMode) EditorGUILayout.EnumPopup("Spawn Sequence", settings.dispersalMode);
		if (newSeq != settings.dispersalMode) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "change Spawn Sequence");
			settings.dispersalMode = newSeq;
		}
		if (settings.dispersalMode == WavePrefabPool.PoolDispersalMode.Randomized) {
			var newExhaust = EditorGUILayout.Toggle("Exhaust before repeat", settings.exhaustiveList);
			if (newExhaust != settings.exhaustiveList) {
				UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Exhaust before repeat");
				settings.exhaustiveList = newExhaust;
			}
		}
		
		var hadNoListener = settings.listener == null;
		var newListener = (WavePrefabPoolListener) EditorGUILayout.ObjectField("Listener", settings.listener, typeof(WavePrefabPoolListener), true);
		if (newListener != settings.listener) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "assign Listener");
			settings.listener = newListener;
			if (hadNoListener && settings.listener != null) {
				settings.listener.sourcePrefabPoolName = settings.transform.name;
			}
		}

		EditorGUILayout.Separator();
		
		EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(50));
		EditorGUILayout.LabelField("Spawners Using");
		if (GUILayout.Button("List", GUILayout.MinWidth(55))) {
			FindMatchingSpawners(poolTrans, false);
		}
		if (GUILayout.Button("Select", GUILayout.MinWidth(55))) {
			FindMatchingSpawners(poolTrans, true);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Separator();
		
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		var newExpanded = DTInspectorUtility.Foldout(settings.isExpanded, string.Format("Prefab Pool Items ({0})", settings.poolItems.Count));
		if (newExpanded != settings.isExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "toggle expand Prefab Pool Items");
			settings.isExpanded = newExpanded;
		}
        // BUTTONS...
        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
		
        // Add expand/collapse buttons if there are items in the list
        if (settings.poolItems.Count > 0) {
            GUIContent content;
            var collapseIcon = '\u2261'.ToString();
            content = new GUIContent(collapseIcon, "Click to collapse all");
            var masterCollapse = GUILayout.Button(content, EditorStyles.toolbarButton);

            var expandIcon = '\u25A1'.ToString();
            content = new GUIContent(expandIcon, "Click to expand all");
            var masterExpand = GUILayout.Button(content, EditorStyles.toolbarButton);
			if (masterExpand) {
				ExpandCollapseAll(settings, true);
			} 
			if (masterCollapse) {
				ExpandCollapseAll(settings, false);
			}
        } else {
         	GUILayout.FlexibleSpace();
        }
		
        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(50));
        // A little space between button groups
        GUILayout.Space(6);
		
		var isDirty = false;
		var topAdded = false;

		var addText = string.Format("Click to add Pool item{0}.", settings.poolItems.Count > 0 ? " before the first" : "");
		
        // Main Add button
		if (GUILayout.Button(new GUIContent("+", addText), EditorStyles.toolbarButton)) {
			topAdded = true;
		}
		
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndHorizontal();

		DTInspectorUtility.FunctionButtons poolItemButton = DTInspectorUtility.FunctionButtons.None;
		
		int? itemToDelete = null;
		int? itemToInsert = null;
		int? itemToShiftUp = null;
		int? itemToShiftDown = null;
		
		if (settings.isExpanded) {
			for (var i = 0; i < settings.poolItems.Count; i++) {
				var item = settings.poolItems[i];
	
	            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				EditorGUI.indentLevel = 1;
				
				var sName = "";
				if (!item.isExpanded) {
					sName = " (" + item.prefabToSpawn.name + ")";
				}
				
				string sDisabled = "";
				bool itemDisabled = item.activeMode == LevelSettings.ActiveItemMode.Never;
				if (!item.isExpanded && itemDisabled) {
					sDisabled = " - DISABLED";
				}

				var newItemExpanded = DTInspectorUtility.Foldout(item.isExpanded, 
				  string.Format("Pool Item #{0}{1}{2}", (i + 1), sName, sDisabled));
				if (newItemExpanded != item.isExpanded) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "toggle expand Pool Item");
					item.isExpanded = newItemExpanded;
				}

	            poolItemButton = DTInspectorUtility.AddFoldOutListItemButtons(i, settings.poolItems.Count, "Pool Item", true, true);

				switch (poolItemButton) {
					case DTInspectorUtility.FunctionButtons.Remove:
						itemToDelete = i;
						isDirty = true;
						break;
					case DTInspectorUtility.FunctionButtons.Add:
						itemToInsert = i;
						isDirty = true;
						break;
					case DTInspectorUtility.FunctionButtons.ShiftUp:
						itemToShiftUp = i;
						isDirty = true;
						break;
					case DTInspectorUtility.FunctionButtons.ShiftDown:
						itemToShiftDown = i;
						isDirty = true;
						break;
				}
				
				EditorGUI.indentLevel = 0;
				EditorGUILayout.EndHorizontal();

				if (itemDisabled) {
					DTInspectorUtility.ShowColorWarning("*This item is currently disabled and will never spawn.");
				}
				
				if (item.isExpanded) {
					EditorGUI.indentLevel = 0;
					
					if (item.prefabToSpawn == null && !itemDisabled) {
						DTInspectorUtility.ShowColorWarning("*Nothing will spawn when this item is chosen from the pool.");
					}

					var newActive = (LevelSettings.ActiveItemMode) EditorGUILayout.EnumPopup("Active Mode", item.activeMode);
					if (newActive != item.activeMode) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Active Mode");
						item.activeMode = newActive;
					}
					
					switch (item.activeMode) {
						case LevelSettings.ActiveItemMode.IfWorldVariableInRange:
						case LevelSettings.ActiveItemMode.IfWorldVariableOutsideRange:
							var missingStatNames = new List<string>();
							missingStatNames.AddRange(allStatNames);
							missingStatNames.RemoveAll(delegate(string obj) {
								return item.activeItemCriteria.HasKey(obj);
							});
							
							var newStat = EditorGUILayout.Popup("Add Active Limit", 0, missingStatNames.ToArray());
							if (newStat != 0) {
								AddActiveLimit(missingStatNames[newStat], item);
							}

							if (item.activeItemCriteria.statMods.Count == 0) {
								DTInspectorUtility.ShowColorWarning("*You have no Active Limits. Item will never be Active.");
							} else {
								EditorGUILayout.Separator();
								
								int? indexToDelete = null;
								
								for (var j = 0; j < item.activeItemCriteria.statMods.Count; j++) {
									var modifier = item.activeItemCriteria.statMods[j];
									if (modifier.modValueMin > modifier.modValueMax) {
										DTInspectorUtility.ShowColorWarning("  *" + modifier.statName + " Min cannot exceed Max, please fix!");
									}
									EditorGUILayout.BeginHorizontal();
									GUILayout.Space(15);
									var statName = modifier.statName;	
									GUILayout.Label(statName);
								
									GUILayout.FlexibleSpace();
									GUILayout.Label("Min");
									var newMin = EditorGUILayout.IntField(modifier.modValueMin, GUILayout.MaxWidth(60));
									if (newMin != modifier.modValueMin) {
										UndoHelper.RecordObjectPropertyForUndo(settings, "change Active Limit Min");
										modifier.modValueMin = newMin;
									}
									
									GUILayout.Label("Max");
									var newMax = EditorGUILayout.IntField(modifier.modValueMax, GUILayout.MaxWidth(60));
									if (newMax != modifier.modValueMax) {
										UndoHelper.RecordObjectPropertyForUndo(settings, "change Active Limit Max");
										modifier.modValueMax = newMax;
									}
									GUI.backgroundColor = Color.green;
									if (GUILayout.Button(new GUIContent("Delete", "Remove this limit"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(64))) {
										indexToDelete = j;
									}
									GUI.backgroundColor = Color.white;
									GUILayout.Space(5);
									EditorGUILayout.EndHorizontal();
								}
								
								DTInspectorUtility.ShowColorWarning("  *Limits are inclusive: i.e. 'Above' means >=");
								if (indexToDelete.HasValue) {
								UndoHelper.RecordObjectPropertyForUndo(settings, "delete Active Limit");
									item.activeItemCriteria.DeleteByIndex(indexToDelete.Value);
									isDirty = true;
								}
								
								EditorGUILayout.Separator();
							}
						
							break;
					}

					var newPrefab = (Transform) EditorGUILayout.ObjectField("Prefab", item.prefabToSpawn ,typeof(Transform), true);
					if (newPrefab != item.prefabToSpawn) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Prefab");
						item.prefabToSpawn = newPrefab;
					}

					var newWeight = EditorGUILayout.IntSlider("Weight", item.weight, 1, 100);
					if (newWeight != item.weight) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "change Weight");
						item.weight = newWeight;
					}
				}
			}
		}
		
		if (topAdded) {
			var newItem = new WavePrefabPoolItem();
			var index = 0;
			if (settings.poolItems.Count > 0) {
				index = settings.poolItems.Count - 1;
			}

			UndoHelper.RecordObjectPropertyForUndo(settings, "add Prefab Pool Item");
			settings.poolItems.Insert(index, newItem);
		} else if (itemToDelete.HasValue) {
			if (settings.poolItems.Count == 1) {
				DTInspectorUtility.ShowAlert("You cannot delete the only Prefab Pool item. Delete the entire Pool from the hierarchy if you wish.");

			} else {
				UndoHelper.RecordObjectPropertyForUndo(settings, "remove Prefab Pool Item");
				settings.poolItems.RemoveAt(itemToDelete.Value);
			}
		} else if (itemToInsert.HasValue) {
			var newItem = new WavePrefabPoolItem();
			UndoHelper.RecordObjectPropertyForUndo(settings, "add Prefab Pool Item");
			settings.poolItems.Insert(itemToInsert.Value + 1, newItem);
		} 
		
		if (itemToShiftUp.HasValue) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "shift up Prefab Pool Item");
			var item = settings.poolItems[itemToShiftUp.Value];
			settings.poolItems.Insert(itemToShiftUp.Value - 1, item);
			settings.poolItems.RemoveAt(itemToShiftUp.Value + 1);
		}
		
		if (itemToShiftDown.HasValue) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "shift down Prefab Pool Item");
			var index = itemToShiftDown.Value + 1;
			var item = settings.poolItems[index];
			settings.poolItems.Insert(index - 1, item);
			settings.poolItems.RemoveAt(index + 1);
		}
		
		if (GUI.changed || topAdded || isDirty) {
  			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}

		this.Repaint();

		//DrawDefaultInspector();
    }

	private void AddActiveLimit(string modifierName, WavePrefabPoolItem spec) {
		if (spec.activeItemCriteria.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This item already has a Active Limit for World Variable: " + modifierName + ". Please modify the existing one instead.");
			return;
		}

		UndoHelper.RecordObjectPropertyForUndo(settings, "add Active Limit");

		spec.activeItemCriteria.statMods.Add(new WorldVariableRange() {
			statName = modifierName
		});
	}
	
	private void ExpandCollapseAll(WavePrefabPool pool, bool isExpand) {
		UndoHelper.RecordObjectPropertyForUndo(settings, "toggle expand / collapse all");

		foreach (var poolItem in pool.poolItems) {
			poolItem.isExpanded = isExpand;
		}
	}
}
