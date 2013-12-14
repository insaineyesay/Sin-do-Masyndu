using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(WorldVariableTracker))]
public class WorldVariableTrackerInspector : Editor {
	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		EditorGUI.indentLevel = 1;
		
		WorldVariableTracker holder = (WorldVariableTracker) target;
		
		var ls = LevelSettings.GetLevelSettings;
		if (ls != null) {
			DTInspectorUtility.DrawTexture(ls.logoTexture);
		}
		
		bool isDirty = false;
		
		var stats = GetPlayerStatsFromChildren(holder.transform);
		
		Transform statToRemove = null;
		bool? willInsert = null;
		
		for (var i = 0; i < stats.Count; i++) {
			var aStat = stats[i];
			
			EditorGUI.indentLevel = 0;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			var newExpand = DTInspectorUtility.Foldout(aStat.isExpanded, aStat.name);
			if (newExpand != aStat.isExpanded) {
				UndoHelper.RecordObjectPropertyForUndo(aStat, "toggle expand Variables");
				aStat.isExpanded = newExpand;
			}
            
			var functionPressed = DTInspectorUtility.AddFoldOutListItemButtons(i, stats.Count, "variable", false);
            EditorGUILayout.EndHorizontal();

			if (aStat.isExpanded) {
				EditorGUI.indentLevel = 1;
				var newName = EditorGUILayout.TextField("Name", aStat.transform.name);
				if (newName != aStat.transform.name) {
					UndoHelper.RecordObjectPropertyForUndo(aStat.gameObject, "change Name");
					aStat.transform.name = newName;
				}

				var newPersist = (WorldVariable.StatPersistanceMode) EditorGUILayout.EnumPopup("Persistance mode", aStat.persistanceMode);
				if (newPersist != aStat.persistanceMode) {
					UndoHelper.RecordObjectPropertyForUndo(aStat, "change Persistance mode");
					aStat.persistanceMode = newPersist;
				}

				var newStart = EditorGUILayout.IntField("Starting value", aStat.startingValue);
				if (newStart != aStat.startingValue) {
					UndoHelper.RecordObjectPropertyForUndo(aStat, "change Starting value");
					aStat.startingValue = newStart;
				}
				
				if (Application.isPlaying) {
					GUI.contentColor = Color.yellow;
					EditorGUILayout.LabelField("Current Value: " + InGameWorldVariable.GetCurrentWorldVariableValue(aStat.name));
					GUI.contentColor = Color.white;
				}

				var newNeg = EditorGUILayout.Toggle("Allow negative?", aStat.allowNegative);
				if (newNeg != aStat.allowNegative) {
					UndoHelper.RecordObjectPropertyForUndo(aStat, "toggle Allow negative");
					aStat.allowNegative = newNeg;
				}

				var newCanEnd = EditorGUILayout.Toggle("Triggers game over?", aStat.canEndGame);
				if (newCanEnd != aStat.canEndGame) {
					UndoHelper.RecordObjectPropertyForUndo(aStat, "toggle Triggers game over");
					aStat.canEndGame = newCanEnd;
				}
				if (aStat.canEndGame) {
					var newMin = EditorGUILayout.IntField("G.O. min value", aStat.endGameMinValue);
					if (newMin != aStat.endGameMinValue) {
						UndoHelper.RecordObjectPropertyForUndo(aStat, "change G.O. min value");
						aStat.endGameMinValue = newMin;
					} 

					var newMax = EditorGUILayout.IntField("G.O. max value", aStat.endGameMaxValue);
					if (newMax != aStat.endGameMaxValue) {
						UndoHelper.RecordObjectPropertyForUndo(aStat, "change G.O. max value");
						aStat.endGameMaxValue = newMax;
					}
				}

				var listenerWasEmpty = aStat.listenerPrefab == null;
				var newListener = (WorldVariableListener) EditorGUILayout.ObjectField("Listener", aStat.listenerPrefab, typeof(WorldVariableListener), true); 
				if (newListener != aStat.listenerPrefab) {
					UndoHelper.RecordObjectPropertyForUndo(aStat, "assign Listener");
					aStat.listenerPrefab = newListener;
					if (listenerWasEmpty && aStat.listenerPrefab != null) {
						// just assigned.
						var listener = aStat.listenerPrefab.GetComponent<WorldVariableListener>();
						if (listener == null) {
							DTInspectorUtility.ShowAlert("You cannot assign a listener that doesn't have a WorldVariableListener script in it.");
							aStat.listenerPrefab = null;
						} else {
							listener.variableName = aStat.transform.name;
						}
					}
				}
			}
			
			switch (functionPressed) {
				case DTInspectorUtility.FunctionButtons.Remove:
					statToRemove = aStat.transform;
					break;
				case DTInspectorUtility.FunctionButtons.Add:
					willInsert = true;
					break;
			}
			
			EditorUtility.SetDirty(aStat);
		}
		
		if (statToRemove != null) {
			isDirty = true;
			RemoveStat(statToRemove);
		}
		if (willInsert.HasValue) {
			isDirty = true;
			InsertStat(holder);
		}
		
		if (GUI.changed || isDirty) {
  			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}

		this.Repaint();
		//DrawDefaultInspector();
    }
	
	private List<WorldVariable> GetPlayerStatsFromChildren(Transform holder) {
		var stats = new List<WorldVariable>();
		
		for (var i = 0; i < holder.childCount; i++) {
			var aTrans = holder.GetChild(i);
			
			var aStat = aTrans.GetComponent<WorldVariable>();
			if (aStat == null) {
				DTInspectorUtility.ShowColorWarning("A prefab under 'PlayerStats' named '" + aTrans.name + "' does");
				DTInspectorUtility.ShowColorWarning("not have a WorldVariable script. Please delete it.");
				continue;
			}
			
			stats.Add(aStat);
		}			
		
		return stats;
	}
	
	private void RemoveStat(Transform stat) {
		if (stat.parent.childCount <= 1) {
			DTInspectorUtility.ShowAlert("You cannot delete the only World Variable. You don't have to use it though.");
			return;
		}
	
		UndoHelper.DestroyForUndo(stat.gameObject);
	}
	
	private void InsertStat(WorldVariableTracker holder) {
		var newStat = (GameObject) GameObject.Instantiate(holder.statPrefab.gameObject, holder.transform.position, Quaternion.identity);

		UndoHelper.CreateObjectForUndo(newStat, "create World Variable");

		newStat.name = "Rename me!";
		newStat.transform.parent = holder.transform;
	}
}
