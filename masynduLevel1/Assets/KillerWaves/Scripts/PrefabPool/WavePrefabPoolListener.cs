using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Dark Tonic/Killer Waves/Listeners/Prefab Pool Listener")]
public class WavePrefabPoolListener : MonoBehaviour {
	public string sourcePrefabPoolName;
	
	public virtual void PrefabGrabbedFromPool(Transform transGrabbed) {
		// your code here
	}
	
	public virtual void PoolRefilling() {
		// your code here
	}
}
