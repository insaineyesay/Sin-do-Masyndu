using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Killer Waves/Spawners/Player Spawner")]
public class PlayerSpawner : MonoBehaviour {
	public Transform PlayerPrefab;
	public Transform RespawnParticlePrefab;
	public Vector3 RespawnParticleOffset = Vector3.zero;
	public float RespawnDelay = 1f;
	public Vector3 spawnPosition;
	
	private Transform Player;
	private float? nextSpawnTime = null;
	private Vector3 playerPosition;
	private bool isDisabled = false;
	
	void Start() {
		if (PlayerPrefab == null) {
			Debug.LogError("No Player Prefab is assigned to PlayerSpawner. PlayerSpawn disabled.");
			this.isDisabled = true;
			return;
		}
		if (RespawnDelay < 0) {
			Debug.LogError("Respawn Delay must be a positive number. PlayerSpawn disabled.");
			this.isDisabled = true;
			return;
		}
		
		this.nextSpawnTime = null;
		this.playerPosition = this.spawnPosition;
		SpawnPlayer();
	}

	void FixedUpdate () {
		if (isDisabled) {
			return;
		}
		
		if (Player == null) {
			if (!this.nextSpawnTime.HasValue) {
				this.nextSpawnTime = Time.time + RespawnDelay;
			} else if (Time.time >= this.nextSpawnTime.Value && !LevelSettings.IsGameOver) {
				SpawnPlayer();
				nextSpawnTime = null;
			}
		} else {
			playerPosition = Player.position;
		}
	}
	
	private void SpawnPlayer() {
		Player = SpawnUtility.Spawn(PlayerPrefab, playerPosition, PlayerPrefab.transform.rotation); 

		var spawnPos = playerPosition + RespawnParticleOffset;
		if (RespawnParticlePrefab != null) {
			SpawnUtility.Spawn(RespawnParticlePrefab, spawnPos, RespawnParticlePrefab.transform.rotation); 
		}
	}
}
