using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Killer Waves/Despawners/Timed Despawner")]
public class TimedDespawner : MonoBehaviour {
	public float LifeSeconds = 5;
	public bool StartTimerOnSpawn = true;
	public Texture logoTexture;
	public TimedDespawnerListener listener;
	
	private float startTime;
	private bool isTimerStarted = false;
	private Transform trans;
	
	void Awake() {
		this.trans = this.transform;
		this.AwakeOrSpawn();
	}
	
	void OnSpawned() {
		this.AwakeOrSpawn();
	}
	
	void AwakeOrSpawn() {
		this.isTimerStarted = false;
		
		if (this.StartTimerOnSpawn) { 
			this.StartTimer();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!this.isTimerStarted) {
			return;
		}
		if (Time.time - this.startTime > LifeSeconds) {
			this.isTimerStarted = false;
			
			if (listener != null) {
				listener.Despawning(this.trans);
			}
			SpawnUtility.Despawn(trans);
		}
	}
	
	public void StartTimer() {
		this.startTime = Time.time;
		this.isTimerStarted = true;
	}
}
