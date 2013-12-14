using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Killer Waves/Listeners/Triggered Despawner Listener")]
public class TriggeredDespawnerListener : MonoBehaviour {
	public string sourceDespawnerName;
	
	public virtual void Despawning(TriggeredSpawner.EventType eType, Transform transDespawning) {
		// Your code here.
	}
}
