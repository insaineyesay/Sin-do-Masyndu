using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Killer Waves/Listeners/Syncro Spawner Listener")]
public class WaveSyncroSpawnerListener : MonoBehaviour {
	public string sourceSpawnerName = string.Empty;

	public virtual void ItemFailedToSpawn(Transform failedPrefabTrans) {
		// your code here. The transform is not spawned. This is just a reference
	}

	public virtual void ItemSpawned(Transform spawnedTrans) {
		// do something to the Transform.
	}
	
	public virtual void WaveFinishedSpawning(WaveSpecifics spec) {
		// Please do not manipulate values in the "spec". It is for your read-only information
	}
	
	public virtual void WaveStart(WaveSpecifics spec) {
		// Please do not manipulate values in the "spec". It is for your read-only information
	}
	
	public virtual void WaveRepeat(WaveSpecifics spec) {
		// Please do not manipulate values in the "spec". It is for your read-only information
	}
}
