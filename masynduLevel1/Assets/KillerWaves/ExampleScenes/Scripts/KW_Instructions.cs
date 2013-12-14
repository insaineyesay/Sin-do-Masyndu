using UnityEngine;
using System.Collections;

public class KW_Instructions : MonoBehaviour {
	void OnGUI() {
		GUI.Label(new Rect(10, 10, 760, 100), "This scene has 6 waves of various settings. Left / right arrow keys and mouse click to fire. Feel free to tweak the settings. Wave 1 will repeat until you have 1000 score. " +
			"Wave 3 will be skipped if you have 3000 score or more. Notice the music changing in wave 3, and the 'prefab pool' in wave 7 which allows for spawning of mutiple different prefabs from the same spawner wave. " +
			"Wave 7 repeats forever and changes the items in its prefab pool depending on your score! " +
			"I have included a KillableListenerSubclass as an example which listens for events on the Player prefab. It's in the Main Camera prefab. Also note that the player gets 5000 Experience Points for completing each of the first two levels. "
			+ "For support, check the readme file for links! Happy gaming!");
		if(GUI.Button(new Rect(70, 70, 150, 50), "Scene2")) {
			Application.LoadLevel(2);
		}
		GUI.Label(new Rect(10, 120, 100, 30), "Score: " + InGameWorldVariable.GetCurrentWorldVariableValue("Score"));
		GUI.Label(new Rect(10, 160, 100, 30), "Exp: " + InGameWorldVariable.GetCurrentWorldVariableValue("Experience Points"));
		GUI.Label(new Rect(10, 190, 100, 30), "Health: " + InGameWorldVariable.GetCurrentWorldVariableValue("Health"));
		GUI.Label(new Rect(10, 220, 100, 30), "lives: " + InGameWorldVariable.GetCurrentWorldVariableValue("Lives"));
	}
}
