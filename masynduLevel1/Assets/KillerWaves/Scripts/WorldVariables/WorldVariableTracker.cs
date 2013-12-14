using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class WorldVariableTracker : MonoBehaviour {
	private static Dictionary<string, InGameWorldVariable> _inGamePlayerStats = new Dictionary<string, InGameWorldVariable>();

	public Transform statPrefab;
	private Transform trans;
	
	void Awake() {
		this.useGUILayout = false;
		Init();
	}
	
	private void Init() {
		_inGamePlayerStats.Clear();
		
		this.trans = this.transform;
	
		// set up variables for use
		for (var i = 0; i < this.trans.childCount; i++) {
			var oTrans = this.trans.GetChild(i);
			var oStat = oTrans.GetComponent<WorldVariable>();
			
			if (oStat == null) {
				Debug.LogError("Transform '" + oTrans.name + "' under WorldVariables does not contain a WorldVariable script. Please fix this.");
				continue;
			}
			
			var newStatTracker = new InGameWorldVariable() {
				sourceStat = oStat,
				statName = oStat.name
			};
			
			switch (oStat.persistanceMode) {
				case WorldVariable.StatPersistanceMode.ResetToStartingValue:
					newStatTracker.CurrentValue = oStat.startingValue;
					break;
				case WorldVariable.StatPersistanceMode.KeepFromPrevious:
					// set to value in player prefs	
					break;
			}

			
			if (oStat.listenerPrefab != null) {
				var variable = InGameWorldVariable.GetCurrentWorldVariableValue(oStat.name);
				if (variable != null) {
					oStat.listenerPrefab.UpdateValue(variable.Value);
				}
			}
			
			_inGamePlayerStats.Add(oStat.name, newStatTracker);
		}
	}

	public static void ModifyPlayerStat(Transform sourceTrans, string statName, int valueToAdd) {
		if (!_inGamePlayerStats.ContainsKey(statName)) {
			Debug.LogError(string.Format("Transform '{0}' tried to modify a World Variable called '{1}', which was not found in this scene.", 
				sourceTrans.name,
				statName));
			return;
		}
		
		var stat = _inGamePlayerStats[statName];
		stat.CurrentValue += valueToAdd;
		
		var sourceStat = stat.sourceStat;
		
		if (!sourceStat.allowNegative && stat.CurrentValue < 0) {
			stat.CurrentValue = 0;
		}
		
		if (sourceStat.canEndGame) {
			if (stat.CurrentValue >= sourceStat.endGameMinValue && stat.CurrentValue <= sourceStat.endGameMaxValue) {
				LevelSettings.IsGameOver = true;
			} 
		}
		
		if (sourceStat.listenerPrefab != null) {
			sourceStat.listenerPrefab.UpdateValue(stat.CurrentValue);
		}
		
		//Debug.Log(statName + " = " + stat.CurrentValue);
	}
}
