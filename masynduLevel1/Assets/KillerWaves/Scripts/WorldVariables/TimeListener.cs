using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Killer Waves/Listeners/Time Listener")]
public class TimeListener : WorldVariableListener {
	public float Timer = 0f;

	void Update () {
		Timer += Time.deltaTime;
		}
	
	// PLEASE replace this OnGUI with an update to NGUI instead or whatever you use. This is just used for illustrative purposes.
	void OnGUI() {
		GUI.Label(new Rect(xStart, yStart, 180, 40), "Time : " + Timer);

	}
}
