using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Killer Waves/Listeners/Current HP World ValueListener SubClass")]
public class CurrentHPWorldValueListenerSubClass : WorldVariableListener {


	// PLEASE replace this OnGUI with an update to NGUI instead or whatever you use. This is just used for illustrative purposes.
	void OnGUI() {
		Killable something = gameObject.GetComponent<Killable> ();
		int curHP = something.currentHitPoints;

		GUI.Label (new Rect (xStart, yStart, 180, 40), variableName + ": " + curHP);

	}

}
