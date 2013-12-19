using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Killer Waves/Listeners/World Variable Listener")]
public class WorldVariableListener : MonoBehaviour {
	public string variableName = "";
	private int variableValue = 0;	
	
	public int xStart = 50; // ALSO delete this when you get rid of the OnGUI section. You won't need it.
	public int yStart = 50; // starting y axis value of gui label

	public virtual void UpdateValue(int newValue) {
		variableValue = newValue;
	}
	
	// PLEASE replace this OnGUI with an update to NGUI instead or whatever you use. This is just used for illustrative purposes.
	void OnGUI() {
		GUI.Label(new Rect(xStart, yStart, 180, 40), variableName + ": " + variableValue);

	}
}
