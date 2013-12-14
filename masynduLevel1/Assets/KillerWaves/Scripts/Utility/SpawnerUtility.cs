using UnityEngine;
using System.Collections;

public static class SpawnerUtility {
	public static void ActivateWave(Transform transSpawner, int levelNumber, int waveNumber) {
		var spawner = transSpawner.GetComponent<WaveSyncroPrefabSpawner>();
		ActivateWave(spawner, levelNumber, waveNumber);
	}
	
	public static void ActivateWave(WaveSyncroPrefabSpawner spawner, int levelNumber, int waveNumber) {
		ChangeSpawnerWaveStatus(spawner, levelNumber, waveNumber, true);
	}
	
	public static void DeactivateWave(Transform transSpawner, int levelNumber, int waveNumber) {
		var spawner = transSpawner.GetComponent<WaveSyncroPrefabSpawner>();
		DeactivateWave(spawner, levelNumber, waveNumber);
	}
	
	public static void DeactivateWave(WaveSyncroPrefabSpawner spawner, int levelNumber, int waveNumber) {
		ChangeSpawnerWaveStatus(spawner, levelNumber, waveNumber, false);
	}	
	
	private static void ChangeSpawnerWaveStatus(WaveSyncroPrefabSpawner spawner, int levelNumber, int waveNumber, bool isActivate) {
		var statusText = isActivate ? "activate" : "deactivate";
		
		if (spawner == null) {
			Debug.LogError(string.Format("Spawner was NULL. Cannot {0} wave# {1} in level# {2}",
				statusText,
				waveNumber,
				levelNumber));
			return;
		}
		
		foreach (var wave in spawner.waveSpecs) {
			if (wave.SpawnLevelNumber + 1 == levelNumber && wave.SpawnWaveNumber + 1 == waveNumber) {			
				if (LevelSettings.IsLoggingOn) {
					Debug.Log(string.Format("Logging '{0}' in spawner '{1}' for wave# {2}, level# {3}.",
						statusText,
						spawner.name,
						waveNumber, 
						levelNumber));
				}
				wave.enableWave = isActivate;
				return;
			}
		}
		
		Debug.LogWarning(string.Format("Could not locate a wave matching wave# {0}, level# {1}, in spawner '{2}'.",
			waveNumber, levelNumber, spawner.name));
	}
}
