using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Killer Waves/Listeners/Timed Despawner Listener")]
public class TimedDespawnerListener : MonoBehaviour {
	public string sourceDespawnerName;
	public static bool hasUpgrade;
	
	public virtual void Despawning(Transform transDespawning) {
		// Your code here.

	}
}
