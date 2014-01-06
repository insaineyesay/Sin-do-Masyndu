using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class TriggeredWaveSpecifics {
	public bool isExpanded = true;
	public bool enableWave = false;
	public int NumberToSpawn = 1;
	public float WaveDelaySeconds = 0;
	public float TimeToSpawnWholeWave = 0;
	public Transform prefabToSpawn;
	public WaveSpecifics.SpawnOrigin spawnSource = WaveSpecifics.SpawnOrigin.Specific;
	public int prefabPoolIndex = 0;
	public string prefabPoolName = null;
	
	public bool enableRepeatWave = false;
	public WaveSpecifics.RepeatWaveMode curWaveRepeatMode = WaveSpecifics.RepeatWaveMode.NumberOfRepetitions;
	public float repeatWavePauseTime = -1f;
	public int maxRepeats = 2;
	public int repeatItemIncrease = 0;
	public int repeatItemLimit = 100;
	public float repeatTimeIncrease = 0f;
	public float repeatTimeLimit = 100f;

	public WorldVariableCollection repeatPassCriteria = new WorldVariableCollection();
	public bool willDespawnOnEvent = false;
	
	public bool useLayerFilter = false;
	public bool useTagFilter = false;
	public List<string> matchingTags = new List<string>() { "Untagged" };
	public List<int> matchingLayers = new List<int>() { 0 };
	
	public bool enableRandomizations;
	public bool randomXRotation;
	public bool randomYRotation;
	public bool randomZRotation;
	public float randomDistanceX = 0f;
	public float randomDistanceY = 0f;
	public float randomDistanceZ = 0f;
	public float randomXRotationMin = -360f;
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
	
	// retrigger limit settings
	public TriggeredSpawner.RetriggerLimitMode retriggerLimitMode = TriggeredSpawner.RetriggerLimitMode.None;
	public int limitPerXFrames = 1;
	public float limitPerXSeconds = 0.1f;
	public int triggeredLastFrame = -200;
	public float triggeredLastTime = -100f;
	
	public enum SpawnSource {
		Specific,
		PrefabPool
	}
	
	public bool IsValid {
		get {
			if (!this.enableWave) {
				return false;
			}
			
			return true;
		}
	}
}
