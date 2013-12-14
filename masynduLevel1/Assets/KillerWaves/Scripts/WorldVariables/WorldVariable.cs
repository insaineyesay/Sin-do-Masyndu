using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class WorldVariable : MonoBehaviour {
	public bool isExpanded = true;
	public bool allowNegative = false;
	public int startingValue = 0;
	public bool canEndGame = false;
	public int endGameMinValue = 0;
	public int endGameMaxValue = 0;
	public StatPersistanceMode persistanceMode = StatPersistanceMode.ResetToStartingValue;
	public WorldVariableListener listenerPrefab;
	
	void Awake() {
		this.useGUILayout = false;
	}
	
	public enum StatPersistanceMode {
		ResetToStartingValue,
		KeepFromPrevious
	}
}
