using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class WaveSpecifics {
	public bool isExpanded = true;
	public bool enableWave = true;
	public int SpawnLevelNumber = 0;
	public int SpawnWaveNumber = 0; 
	public int MinToSpawn = 1;
	public int MaxToSpawn = 2;
	public float WaveDelaySeconds = 0;
	public float TimeToSpawnWholeWave = 3;
	public Transform prefabToSpawn;
	public SpawnOrigin spawnSource = SpawnOrigin.Specific;
	public int prefabPoolIndex = 0;
	public string prefabPoolName = "";
	public bool repeatWaveUntilNew = false;
	public RepeatWaveMode curWaveRepeatMode = RepeatWaveMode.NumberOfRepetitions;
	public TimedRepeatWaveMode curTimedRepeatWaveMode = TimedRepeatWaveMode.EliminationStyle;
	public float repeatPauseMin = 0f;
	public float repeatPauseMax = 0f;
	public int repeatItemIncrease = 0;
	public float repeatTimeIncrease = 0f;
	public int repeatItemLimit = 100;
	public float repeatTimeLimit = 100f;

	public int repetitions = 2;
	public WorldVariableCollection repeatPassCriteria = new WorldVariableCollection();
	
	public bool enableLimits = false;
	public float doNotSpawnIfMemberCloserThan = 5;
	public float doNotSpawnRandomDistance = 0f;
	
	public bool enableRandomizations;
	public bool randomXRotation;
	public bool randomYRotation;
	public bool randomZRotation;
	public float randomDistanceX = 0f;
	public float randomDistanceY = 0f;
	public float randomDistanceZ = 0f;
	
	public float randomXRotationMin = 0f;
	public float randomXRotationMax = 360f;
	public float randomYRotationMin = 0f;
	public float randomYRotationMax = 360f;
	public float randomZRotationMin = 0f;
	public float randomZRotationMax = 360f;
	
	public bool enableIncrements;
	public float incrementPosX = 0f;
	public float incrementPosY = 0f;
	public float incrementPosZ = 0f;
	public float incrementRotationX = 0f;
	public float incrementRotationY = 0f;
	public float incrementRotationZ = 0f;
	
	public bool enablePostSpawnNudge = false;
	public float postSpawnNudgeForward = 0f;
	public float postSpawnNudgeRight = 0f;
	public float postSpawnNudgeDown = 0f;
	
	public enum RepeatWaveMode {
		Endless,
		NumberOfRepetitions,
		UntilWorldVariableAbove,
		UntilWorldVariableBelow
	}
	
	public enum TimedRepeatWaveMode {
		EliminationStyle,
		StrictTimeStyle
	}
	
	public enum SpawnOrigin {
		Specific,
		PrefabPool
	}
	
	public bool IsValid {
		get {
			if (!this.enableWave) {
				return false;
			}
			
			if (this.repeatPauseMin > this.repeatPauseMax) {
				return false;
			}
			
			if (this.MinToSpawn > this.MaxToSpawn) {
				return false;
			}
			
			return true;
		}
	}
}
