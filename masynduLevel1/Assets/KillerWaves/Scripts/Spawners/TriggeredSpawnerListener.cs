using UnityEngine;
using System.Collections;


[AddComponentMenu("Dark Tonic/Killer Waves/Listeners/Triggered Spawner Listener")]
public class TriggeredSpawnerListener : MonoBehaviour {
	public string sourceSpawnerName = string.Empty;
	
	public virtual void EventPropagating(TriggeredSpawner.EventType eType, Transform transmitterTrans, int receiverSpawnerCount) {
		// your code here.
	}
	
	public virtual void PropagatedEventReceived(TriggeredSpawner.EventType eType, Transform transmitterTrans) {
		// your code here. 
	}
	
	public virtual void ItemFailedToSpawn(TriggeredSpawner.EventType eType, Transform failedPrefabTrans) {
		// your code here. The transform is not spawned. This is just a reference
	}

	public virtual void ItemSpawned(TriggeredSpawner.EventType eType, Transform spawnedTrans) {
		// do something to the Transform.
	}
	
	public virtual void WaveFinishedSpawning(TriggeredSpawner.EventType eType, TriggeredWaveSpecifics spec) {
		// please do not manipulate values in the "spec". It is for your read-only information
	}
	
	public virtual void WaveStart(TriggeredSpawner.EventType eType, TriggeredWaveSpecifics spec) {
		// please do not manipulate values in the "spec". It is for your read-only information
	}
	
	public virtual void WaveRepeat(TriggeredSpawner.EventType eType, TriggeredWaveSpecifics spec) {
		// please do not manipulate values in the "spec". It is for your read-only information
	}
	
	public virtual void SpawnerDespawning(Transform transDespawning) {
		// your code here.
	}
}
