using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(WorldVariable))]
public class WorldVariableInspector : Editor {
	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		EditorGUI.indentLevel = 0;
		
		WorldVariable stat = (WorldVariable) target;
		
		var ls = LevelSettings.GetLevelSettings;
		if (ls != null) {
			DTInspectorUtility.DrawTexture(ls.logoTexture);
		}
		
		bool isDirty = false;

		var newName = EditorGUILayout.TextField("Name", stat.transform.name);
		if (newName != stat.transform.name) {
			UndoHelper.RecordObjectPropertyForUndo(stat.gameObject, "change Name");
			stat.transform.name = newName;
		}

		var newPersist = (WorldVariable.StatPersistanceMode) EditorGUILayout.EnumPopup("Persistance mode", stat.persistanceMode);
		if (newPersist != stat.persistanceMode) {
			UndoHelper.RecordObjectPropertyForUndo(stat, "change Persistance Mode");
			stat.persistanceMode = newPersist;
		}

		var newStart = EditorGUILayout.IntField("Starting value", stat.startingValue);
		if (newStart != stat.startingValue) {
			UndoHelper.RecordObjectPropertyForUndo(stat, "change Starting value");
			stat.startingValue = newStart;
		}
		
		if (Application.isPlaying) {
			GUI.contentColor = Color.yellow;
			EditorGUILayout.LabelField("Current Value: " + InGameWorldVariable.GetCurrentWorldVariableValue(stat.name));
			GUI.contentColor = Color.white;
		}
		
		var newNeg = EditorGUILayout.Toggle("Allow negative?", stat.allowNegative);
		if (newNeg != stat.allowNegative) {
			UndoHelper.RecordObjectPropertyForUndo(stat, "toggle Allow negative");
			stat.allowNegative = newNeg;
		}

		var newCanEnd = EditorGUILayout.Toggle("Triggers game over?", stat.canEndGame);
		if (newCanEnd != stat.canEndGame) {
			UndoHelper.RecordObjectPropertyForUndo(stat, "toggle Triggers game over");
			stat.canEndGame = newCanEnd;
		}
		if (stat.canEndGame) {
			var newMin = EditorGUILayout.IntField("G.O. min value", stat.endGameMinValue);
			if (newMin != stat.endGameMinValue) {
				UndoHelper.RecordObjectPropertyForUndo(stat, "change G.O. min value");
				stat.endGameMinValue = newMin;
			}

			var newMax = EditorGUILayout.IntField("G.O. max value", stat.endGameMaxValue);
			if (newMax != stat.endGameMaxValue) {
				UndoHelper.RecordObjectPropertyForUndo(stat, "change G.O. max value");
				stat.endGameMaxValue = newMax;
			}
		}

		var listenerWasEmpty = stat.listenerPrefab == null;
		var newListener = (WorldVariableListener) EditorGUILayout.ObjectField("Listener", stat.listenerPrefab, typeof(WorldVariableListener), true); 
		if (newListener != stat.listenerPrefab) {
			UndoHelper.RecordObjectPropertyForUndo(stat, "assign Listener");
			stat.listenerPrefab = newListener;
			if (listenerWasEmpty && stat.listenerPrefab != null) {
				// just assigned.
				var listener = stat.listenerPrefab.GetComponent<WorldVariableListener>();
				if (listener == null) {
					DTInspectorUtility.ShowAlert("You cannot assign a listener that doesn't have a WorldVariableListener script in it.");
					stat.listenerPrefab = null;
				} else {
					listener.variableName = stat.transform.name;
				}
			}
		}
			
		if (GUI.changed || isDirty) {
  			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}

		this.Repaint();

		//DrawDefaultInspector();
    }
}
