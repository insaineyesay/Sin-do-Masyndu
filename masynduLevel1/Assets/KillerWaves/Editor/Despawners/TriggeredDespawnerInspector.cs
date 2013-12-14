using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
	[CustomEditor(typeof(TriggeredDespawner))]
#else
	[CustomEditor(typeof(TriggeredDespawner), true)]
#endif
public class TriggeredDespawnerInspector : Editor {
	private TriggeredDespawner settings;
	
	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		
		settings = (TriggeredDespawner)target;
		
		var ls = LevelSettings.GetLevelSettings;
		if (ls != null) {
			DTInspectorUtility.DrawTexture(ls.logoTexture);
		}
			
		EditorGUI.indentLevel = 0;
		
		List<bool> changedList = new List<bool>();
		changedList.Add(RenderEventTypeControls(settings.invisibleSpec, "Invisible Event", TriggeredSpawner.EventType.Invisible));
		changedList.Add(RenderEventTypeControls(settings.mouseOverSpec, "MouseOver Event", TriggeredSpawner.EventType.MouseOver));
		changedList.Add(RenderEventTypeControls(settings.mouseClickSpec, "MouseClick Event", TriggeredSpawner.EventType.MouseClick));
		changedList.Add(RenderEventTypeControls(settings.collisionSpec, "Collision Event", TriggeredSpawner.EventType.OnCollision));
		changedList.Add(RenderEventTypeControls(settings.triggerEnterSpec, "Trigger Enter Event", TriggeredSpawner.EventType.OnTriggerEnter));
		changedList.Add(RenderEventTypeControls(settings.triggerExitSpec, "Trigger Exit Event", TriggeredSpawner.EventType.OnTriggerExit));

		#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		// not supported
		#else
			changedList.Add(RenderEventTypeControls(settings.collision2dSpec, "2D Collision Event", TriggeredSpawner.EventType.OnCollision2D));
			changedList.Add(RenderEventTypeControls(settings.triggerEnter2dSpec, "2D Trigger Enter Event", TriggeredSpawner.EventType.OnTriggerEnter2D));
			changedList.Add(RenderEventTypeControls(settings.triggerExit2dSpec, "2D Trigger Exit Event", TriggeredSpawner.EventType.OnTriggerExit2D));
		#endif

		changedList.Add(RenderEventTypeControls(settings.onClickSpec, "OnClick Event", TriggeredSpawner.EventType.OnClick));
		
		var hadNoListener = settings.listener == null;
		var newListener = (TriggeredDespawnerListener) EditorGUILayout.ObjectField("Listener", settings.listener, typeof(TriggeredDespawnerListener), true);
		if (newListener != settings.listener) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "assign Listener");
			settings.listener = newListener;

			if (hadNoListener && settings.listener != null) {
				settings.listener.sourceDespawnerName = settings.transform.name;
			}
		}
		
		if (GUI.changed || changedList.Contains(true)) {
  			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}

		this.Repaint();

		//DrawDefaultInspector();
    }
	
	private bool RenderEventTypeControls(EventDespawnSpecifics despawnSettings, string toggleText, TriggeredSpawner.EventType eventType) {
		EditorGUI.indentLevel = 0;
        EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
		
		var newEnabled = EditorGUILayout.Toggle(toggleText, despawnSettings.eventEnabled);
		if (newEnabled != despawnSettings.eventEnabled) {
			UndoHelper.RecordObjectPropertyForUndo(settings, "toggle " + toggleText + " enabled");
			despawnSettings.eventEnabled = newEnabled;
		}
        EditorGUILayout.EndHorizontal();
		 
		var isDirty = false;
		
		if (despawnSettings.eventEnabled) {
			if (TriggeredSpawner.eventsWithTagLayerFilters.Contains(eventType)) {
				var newUseLayerFilter = EditorGUILayout.BeginToggleGroup("Layer filters", despawnSettings.useLayerFilter);
				if (newUseLayerFilter != despawnSettings.useLayerFilter) {
					UndoHelper.RecordObjectPropertyForUndo(settings, "toggle Layer filters");
					despawnSettings.useLayerFilter = newUseLayerFilter;
				}
				if (despawnSettings.useLayerFilter) {
					for (var i = 0; i < despawnSettings.matchingLayers.Count; i++) {
						var newMatch = EditorGUILayout.LayerField("Layer Match " + (i + 1), despawnSettings.matchingLayers[i]);
						if (newMatch != despawnSettings.matchingLayers[i]) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Layer Match");
							despawnSettings.matchingLayers[i] = newMatch;
						}
					}
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(24);
					if (GUILayout.Button(new GUIContent("Add", "Click to add a Layer Match at the end"), GUILayout.Width(60))) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "add Layer Match");
						
						despawnSettings.matchingLayers.Add(0);
						isDirty = true;
					}
					if (despawnSettings.matchingLayers.Count > 1) {
						if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Layer Match"), GUILayout.Width(60))) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "remove Layer Match");

							despawnSettings.matchingLayers.RemoveAt(despawnSettings.matchingLayers.Count - 1);
							isDirty = true;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndToggleGroup();
				
				despawnSettings.useTagFilter = EditorGUILayout.BeginToggleGroup("Tag filter", despawnSettings.useTagFilter);
				if (despawnSettings.useTagFilter) {
					for (var i = 0; i < despawnSettings.matchingTags.Count; i++) {
						var newMatch = EditorGUILayout.TagField("Tag Match " + (i + 1), despawnSettings.matchingTags[i]);
						if (newMatch != despawnSettings.matchingTags[i]) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "change Tag Match");
							despawnSettings.matchingTags[i] = newMatch;
						}
					}
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(24);
					if (GUILayout.Button(new GUIContent("Add", "Click to add a Tag Match at the end"), GUILayout.Width(60))) {
						UndoHelper.RecordObjectPropertyForUndo(settings, "add Tag Match");
						despawnSettings.matchingTags.Add("Untagged");
						isDirty = true;
					}
					if (despawnSettings.matchingTags.Count > 1) {
						if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Tag Match"), GUILayout.Width(60))) {
							UndoHelper.RecordObjectPropertyForUndo(settings, "remove Tag Match");
							despawnSettings.matchingTags.RemoveAt(despawnSettings.matchingLayers.Count - 1);
							isDirty = true;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndToggleGroup();
			} else {
				EditorGUI.indentLevel = 0;
				DTInspectorUtility.ShowColorWarning("No additional properties for this event type.");
			}
		}
		
		return isDirty;
	}

}
