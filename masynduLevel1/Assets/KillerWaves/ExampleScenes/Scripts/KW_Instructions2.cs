using UnityEngine;
using System.Collections;

public class KW_Instructions2 : MonoBehaviour {
	void OnGUI() {
		GUI.Label(new Rect(10, 10, 760, 60), "Make sure to play this one with 'Maximize on Play' on or it will be impossible to play. " +
			"This scene has triggered spawners of different types. Left / right arrow keys and mouse click to fire. " +
			"You will see triggered projectile waves, triggered attack waves and spawners of spawners. All with no code on your end! " +
			"Notice the decreasing attack on repeat of the Triggered Spawners on the first enemy.");
		
		if(GUI.Button(new Rect(70, 70, 150, 50), "Scene1")) {
			Application.LoadLevel(1);
		}
		if(GUI.Button(new Rect(220, 70, 150, 50), "Main menu")) {
			Application.LoadLevel(0);
		}
	}
}
