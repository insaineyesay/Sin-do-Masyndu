using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DTInspectorUtility {
	public const string DROP_DOWN_NONE_OPTION = "-None-";

	private const string ALERT_TITLE = "Killer Waves Alert";
	private const string ALERT_OK_TEXT = "Ok";
	
	private const string FOLD_OUT_TOOLTIP = "Click to expand or collapse";
    private const int CONTROLS_DEFAULT_LABEL_WIDTH = 140;
	
	public enum FunctionButtons { None, Add, Remove, ShiftUp, ShiftDown }
	
    public static FunctionButtons AddFoldOutListItemButtons(int position, int totalPositions, string itemName, bool showAfterText, bool showMoveButtons = false)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));

        // A little space between button groups
        GUILayout.Space(24);
		
		bool upPressed = false;
		bool downPressed = false;
		
		if (showMoveButtons) {
	        if (position > 0) {
				// the up arrow.
				var upArrow = '\u25B2'.ToString();
        		upPressed = GUILayout.Button(new GUIContent(upArrow, "Click to shift " + itemName + " up"),
                                          EditorStyles.toolbarButton, GUILayout.Width(24));
			}

			if (position < totalPositions - 1) {
	        	// The down arrow will move things towards the end of the List
				var dnArrow = '\u25BC'.ToString();
	        	downPressed = GUILayout.Button(new GUIContent(dnArrow, "Click to shift " + itemName + " down"), 
					EditorStyles.toolbarButton, GUILayout.Width(24));
			}
		}

			
        // Add button - Process presses later
        var buttonText = "Click to add new " + itemName;
		if (showAfterText) {
			buttonText += " after this one";
		}
		bool addPressed = GUILayout.Button(new GUIContent("+", buttonText),
                                           EditorStyles.toolbarButton, GUILayout.Width(24));

		// Remove Button - Process presses later
        bool removePressed = GUILayout.Button(new GUIContent("-", "Click to remove last " + itemName),
                                              EditorStyles.toolbarButton, GUILayout.Width(24));

        EditorGUILayout.EndHorizontal();

        // Return the pressed button if any
        if (removePressed == true) {
			return FunctionButtons.Remove;
		}         
		if (addPressed == true) {
			return FunctionButtons.Add;
		}
		if (upPressed) {
			return FunctionButtons.ShiftUp;
		}
		if (downPressed) {
			return FunctionButtons.ShiftDown;
		}

        return FunctionButtons.None;
    }
	
	public static bool Foldout(bool expanded, string label)
    {
        var content = new GUIContent(label, FOLD_OUT_TOOLTIP);
        expanded = EditorGUILayout.Foldout(expanded, content);

        return expanded;
    }
	
	public static void DrawTexture(Texture tex) {
		if (tex == null) {
			Debug.Log("Logo texture missing");
			return;
		}
		
		Rect rect = GUILayoutUtility.GetRect(0f, 0f);
		rect.width = tex.width;
		rect.height = tex.height;
		GUILayout.Space(rect.height);
		GUI.DrawTexture(rect, tex);
	}
	
	public static void ShowColorWarning(string warningText) {
		GUI.color = Color.green;
		EditorGUILayout.LabelField(warningText, EditorStyles.miniLabel);
		GUI.color = Color.white;
	}

	public static bool ConfirmDialog(string text) {
		if (Application.isPlaying) {
			return true;
		}

		return EditorUtility.DisplayDialog(ALERT_TITLE, text, ALERT_OK_TEXT);
	}

	public static void ShowAlert(string text) {
		if (Application.isPlaying) {
			Debug.LogWarning(text);
		} else {
		EditorUtility.DisplayDialog(ALERT_TITLE, text,
			ALERT_OK_TEXT);
		}
	}
	
}
