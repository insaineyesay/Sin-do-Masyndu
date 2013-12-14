using UnityEngine;
using System.Collections;

public class SpawnTracker : MonoBehaviour {
	private WaveSyncroPrefabSpawner sourceSpawner;
	private Transform trans;
	
	void Awake() {
		this.trans = this.transform;
	}
	
	void OnEnable() {
		//Debug.Log("enabled");
	}
	
	void OnDisable() {
		this.sourceSpawner.RemoveSpawnedMember(this.trans);
		this.sourceSpawner = null;
	}
	
	public WaveSyncroPrefabSpawner SourceSpawner { 
		get {
			return this.sourceSpawner;
		}
		set {
			this.sourceSpawner = value;
		}
	}
}
