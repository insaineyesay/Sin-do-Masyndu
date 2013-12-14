using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Dark Tonic/Killer Waves/World Variables/World Variable Resetter")]
public class WorldVariableResetter : MonoBehaviour {
	public List<WorldVariableToReset> varsToReset = new List<WorldVariableToReset>() {
		new WorldVariableToReset() {
			statName = "RENAME ME!"
		}
	};
	
	// Use this for initialization
	void Awake () {
		for (var i = 0; i < varsToReset.Count; i++) {
			var varToReset = varsToReset[i];
			InGameWorldVariable.SetCurrentWorldVariableValue(varToReset.statName, varToReset.resetValueTo);
		}
	}
}
