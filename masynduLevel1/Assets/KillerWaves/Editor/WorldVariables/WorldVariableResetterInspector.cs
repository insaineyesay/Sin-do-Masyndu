using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(WorldVariableResetter))]
public class WorldVariableResetterInspector : Editor {
	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		EditorGUI.indentLevel = 0;
		
		WorldVariableResetter stats = (WorldVariableResetter) target;
		
		var ls = LevelSettings.GetLevelSettings;
		if (ls != null) {
			DTInspectorUtility.DrawTexture(ls.logoTexture);
		}
		
		bool isDirty = false;
		
		int? indexToRemove = null;
		int? indexToInsertAt = null;

		for (var i = 0; i < stats.varsToReset.Count; i++) {
			var stat = stats.varsToReset[i];

			EditorGUI.indentLevel = 0;
			
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            // Display foldout with current state
			var newExpanded = DTInspectorUtility.Foldout(stat.isExpanded, stat.statName + " Reset");
			if (newExpanded != stat.isExpanded) {
				UndoHelper.RecordObjectPropertyForUndo(stats, "toggle expand Variable");
				stat.isExpanded = newExpanded;
			}

            var buttonPressed = DTInspectorUtility.AddFoldOutListItemButtons(i, stats.varsToReset.Count, "Variable Reset", false);
            EditorGUILayout.EndHorizontal();

			var newName = EditorGUILayout.TextField("Variable Name", stat.statName);
			if (newName != stat.statName) {
				UndoHelper.RecordObjectPropertyForUndo(stats, "change Variable Name");
				stat.statName = newName;
			}

			var newVal = EditorGUILayout.IntField("Reset Value", stat.resetValueTo);
			if (newVal != stat.resetValueTo) {
				UndoHelper.RecordObjectPropertyForUndo(stats, "change Reset Value");
				stat.resetValueTo = newVal;
			}

			switch (buttonPressed) {
				case DTInspectorUtility.FunctionButtons.Remove:
					indexToRemove = i;
					break;
				case DTInspectorUtility.FunctionButtons.Add:
					indexToInsertAt = i;
					break;
			}
		}
		
		if (indexToRemove.HasValue) {
			if (stats.varsToReset.Count <= 1) {
				DTInspectorUtility.ShowAlert("You cannot delete the last variable reset. You can remove the script if you don't need it.");				
			} else {
				UndoHelper.RecordObjectPropertyForUndo(stats, "delete Variable Reset");
				stats.varsToReset.RemoveAt(indexToRemove.Value);
				isDirty = true;
			}
		}
		if (indexToInsertAt.HasValue) {
			UndoHelper.RecordObjectPropertyForUndo(stats, "add Variable Reset");
			stats.varsToReset.Insert(indexToInsertAt.Value + 1, new WorldVariableToReset() { statName = "RENAME ME!"});
			isDirty = true;
		}
		
		if (GUI.changed || isDirty) {
  			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}

		this.Repaint();

		//DrawDefaultInspector();
    }
}
