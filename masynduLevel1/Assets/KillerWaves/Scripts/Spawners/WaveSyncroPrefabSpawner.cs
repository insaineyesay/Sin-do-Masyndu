using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WaveSyncroPrefabSpawner : MonoBehaviour {
	public Texture logoTexture;
	public List<WaveSpecifics> waveSpecs = new List<WaveSpecifics>();
	public bool isExpanded = true;

	public LevelSettings.ActiveItemMode activeMode = LevelSettings.ActiveItemMode.Always;
	public WorldVariableRangeCollection activeItemCriteria = new WorldVariableRangeCollection();
	
	public TriggeredSpawner.GameOverBehavior gameOverBehavior = TriggeredSpawner.GameOverBehavior.Disable;
	public TriggeredSpawner.WavePauseBehavior wavePauseBehavior = TriggeredSpawner.WavePauseBehavior.Disable;
	public WaveSyncroSpawnerListener listener;
	
	private int currentWaveSize;
	private float currentWaveLength;
	private bool waveFinishedSpawning;
	private int countSpawned; 
	private float singleSpawnTime;
	private float lastSpawnTime;
	private WaveSpecifics currentWave;
	private float waveStartTime; 
	private Transform trans;
	private List<Transform> spawnedWaveMembers = new List<Transform>();
	private float? repeatTimer;
	private float repeatWaitTime;
	private int waveRepetitionNumber;
	private bool spawnerValid;
	private WavePrefabPool wavePool = null;
	private int instanceId;
	
	private float currentRandomLimitDistance;
	
	// Use this for initialization
	void Awake () {
		this.trans = this.transform;
		this.waveFinishedSpawning = true;
		this.repeatTimer = null;
		this.spawnerValid = true;
		this.waveRepetitionNumber = 0;
		this.instanceId = GetInstanceID();
		
		this.CheckForDuplicateWaveLevelSettings();
	}
	
	private bool SpawnerIsPaused {
		get {
			return LevelSettings.WavesArePaused && wavePauseBehavior == TriggeredSpawner.WavePauseBehavior.Disable;
		}
	}
	
	private bool GameIsOverForSpawner {
		get {
			return LevelSettings.IsGameOver && this.gameOverBehavior == TriggeredSpawner.GameOverBehavior.Disable;
		}
	}
	
	private void CheckForDuplicateWaveLevelSettings() {
		var waveLevelCombos = new List<string>();
		foreach (var wave in waveSpecs) {
			var combo = wave.SpawnLevelNumber + ":" + wave.SpawnWaveNumber;
			if (waveLevelCombos.Contains(combo)) {
				Debug.LogError(string.Format("Spawner '{0}' contains more than one wave setting for level: {1} and wave: {2}. Spawner aborting until this is fixed.",
					this.name, 
					wave.SpawnLevelNumber + 1, 
					wave.SpawnWaveNumber + 1
					));
				this.spawnerValid = false;
				
				break;
			}
			
			waveLevelCombos.Add(combo);
		}
	}
	
	public bool WaveChange() {
		if (!this.spawnerValid) {
			return false;
		}
		
		bool setupNew = SetupNextWave(true);
		if (setupNew) {
			if (listener != null) {
				listener.WaveStart(this.currentWave);
			}
		}
		
		return setupNew;
	}
	
	public void WaveRepeat() {
		if (SetupNextWave(false)) {
			if (listener != null) {
				listener.WaveRepeat(this.currentWave);
			}
		}
	}
	
	void Update() {
		if (GameIsOverForSpawner || SpawnerIsPaused || this.currentWave == null || !this.spawnerValid) {
			return;
		}
		
		this.StartNextEliminationWave();

		if (this.waveFinishedSpawning 
			|| (Time.time - this.waveStartTime < this.currentWave.WaveDelaySeconds)
			|| (Time.time - this.lastSpawnTime <= this.singleSpawnTime && this.singleSpawnTime > Time.deltaTime)) {
			
			return;
		}
		
		int numberToSpawn = 1;
		if (this.singleSpawnTime < Time.deltaTime) {
			if (this.singleSpawnTime == 0) {
				numberToSpawn = currentWaveSize;
			} else {
				numberToSpawn = (int) Math.Ceiling(Time.deltaTime / this.singleSpawnTime);
			}
		}
		
		for (var i = 0; i < numberToSpawn; i++) {
			if (this.CanSpawnOne()) {
				SpawnOne();
			}
			
			if (this.countSpawned >= currentWaveSize) {
				if (LevelSettings.IsLoggingOn) {
					Debug.Log(string.Format("Spawner '{0}' finished spawning wave# {1} on level# {2}.",
						this.name,
						this.currentWave.SpawnWaveNumber + 1,
						this.currentWave.SpawnLevelNumber + 1));
				}
				this.waveFinishedSpawning = true;
				
				if (this.listener != null) {
					this.listener.WaveFinishedSpawning(this.currentWave);
				}
			}
			
			this.lastSpawnTime = Time.time;
		}
	}
	
	public void SpawnOneItem() {
		SpawnOne(true);
	}
	
	private void SpawnOne(bool fromExternalScript = false) {
		if (fromExternalScript && this.currentWave == null) {
			return; // no active spawner for this wave.
		}
		
		Transform prefabToSpawn = this.GetSpawnable(this.currentWave);		
		
		if (this.currentWave.spawnSource == WaveSpecifics.SpawnOrigin.PrefabPool && prefabToSpawn == null) {
			return;
		}
		
		if (prefabToSpawn == null) {
			if (fromExternalScript) {
				Debug.LogWarning("Cannot 'spawn one' from spawner: " + this.trans.name + " because it has either no settings or selected prefab to spawn for the current wave.");
				return;
			}
			
			Debug.LogError(string.Format("Spawner '{0}' has no prefab to spawn for wave# {1} on level# {2}.", 
				this.name, 
				this.currentWave.SpawnWaveNumber + 1,
				this.currentWave.SpawnLevelNumber + 1));

			this.spawnerValid = false;
			return;
		}

		var spawnPosition = this.GetSpawnPosition(this.trans.position, this.countSpawned);
		
		var spawnedPrefab = SpawnUtility.Spawn(prefabToSpawn, 
			spawnPosition, this.GetSpawnRotation(prefabToSpawn, this.countSpawned));
	
		if (spawnedPrefab == null) {
			Debug.Log("Could not spawn: " + prefabToSpawn + " : " + Time.time); // in case you might want to increase your pool size so this doesn't happen. If not, comment out this line.
			if (listener != null) {
				listener.ItemFailedToSpawn(prefabToSpawn);
			}
			return;
		}
		
		this.AddSpawnTracker(spawnedPrefab);
		this.AfterSpawn(spawnedPrefab);
		if (!IsStrictTimedWaveRepeat) {
			this.spawnedWaveMembers.Add(spawnedPrefab);
		}
		LevelSettings.AddWaveSpawnedItem(spawnedPrefab);
		this.countSpawned++;
		
		if (this.currentWave.enableLimits) {
			currentRandomLimitDistance = UnityEngine.Random.Range(-this.currentWave.doNotSpawnRandomDistance, this.currentWave.doNotSpawnRandomDistance);
		}
	}
	
	protected virtual Vector3 GetSpawnPosition(Vector3 pos, int itemSpawnedIndex) {
		var addVector = Vector3.zero;

		if (this.currentWave.enableRandomizations) {
			addVector.x = UnityEngine.Random.Range(-currentWave.randomDistanceX, currentWave.randomDistanceX);
			addVector.y = UnityEngine.Random.Range(-currentWave.randomDistanceY, currentWave.randomDistanceY);
			addVector.z = UnityEngine.Random.Range(-currentWave.randomDistanceZ, currentWave.randomDistanceZ);
		}
		
		if (this.currentWave.enableIncrements && itemSpawnedIndex > 0) {
			addVector.x += (currentWave.incrementPosX * itemSpawnedIndex);
			addVector.y += (currentWave.incrementPosY * itemSpawnedIndex);
			addVector.z += (currentWave.incrementPosZ * itemSpawnedIndex);
		}
		
		return pos + addVector;
	}
	
	protected virtual Quaternion GetSpawnRotation(Transform prefabToSpawn, int itemSpawnedIndex) {
		Vector3 euler = prefabToSpawn.rotation.eulerAngles;

		if (this.currentWave.enableRandomizations && this.currentWave.randomXRotation) {
			euler.x = UnityEngine.Random.Range(this.currentWave.randomXRotationMin, this.currentWave.randomXRotationMax);
		} else if (this.currentWave.enableIncrements && itemSpawnedIndex > 0) {
			euler.x += (itemSpawnedIndex * this.currentWave.incrementRotationX);
		}
		
		if (this.currentWave.enableRandomizations && this.currentWave.randomYRotation) {
			euler.y = UnityEngine.Random.Range(this.currentWave.randomYRotationMin, this.currentWave.randomYRotationMax);
		} else if (this.currentWave.enableIncrements && itemSpawnedIndex > 0) {
			euler.y += (itemSpawnedIndex * this.currentWave.incrementRotationY);
		}
		
		if (this.currentWave.enableRandomizations &&  this.currentWave.randomZRotation) {
			euler.z = UnityEngine.Random.Range(this.currentWave.randomZRotationMin, this.currentWave.randomZRotationMax);
		} else if (this.currentWave.enableIncrements && itemSpawnedIndex > 0) {
			euler.z += (itemSpawnedIndex * this.currentWave.incrementRotationZ);
		}

		return Quaternion.Euler(euler);
	}
	
	private void AddSpawnTracker(Transform spawnedTrans) {
		var tracker = spawnedTrans.GetComponent<SpawnTracker>();
		if (tracker == null) {
			spawnedTrans.gameObject.AddComponent(typeof(SpawnTracker));
			tracker = spawnedTrans.GetComponent<SpawnTracker>();
		}
		
		tracker.SourceSpawner = this;
	}
	
	protected virtual void AfterSpawn(Transform spawnedTrans) {
		if (this.currentWave.enablePostSpawnNudge) {
			spawnedTrans.Translate(Vector3.forward * this.currentWave.postSpawnNudgeForward);
			spawnedTrans.Translate(Vector3.right * this.currentWave.postSpawnNudgeRight);
			spawnedTrans.Translate(Vector3.down * this.currentWave.postSpawnNudgeDown);
		}
		
		if (this.listener != null) {
			listener.ItemSpawned(spawnedTrans);
		}
	}
	
	protected virtual bool CanSpawnOne() {
		if (this.currentWave.enableLimits) {
			var allSpawnedAreFarEnoughAway = SpawnUtility.SpawnedMembersAreAllBeyondDistance(this.trans, this.spawnedWaveMembers, 
				this.currentWave.doNotSpawnIfMemberCloserThan + currentRandomLimitDistance);
			return allSpawnedAreFarEnoughAway;
		}
		
		return true;
	}
	
	public bool IsUsingPrefabPool(Transform poolTrans) {
		foreach (var _wave in this.waveSpecs) {
			if (_wave.spawnSource == WaveSpecifics.SpawnOrigin.PrefabPool && _wave.prefabPoolName == poolTrans.name) {
				return true;
			}
		}
		
		return false;
	}
	
	public WaveSpecifics FindWave(int levelToMatch, int waveToMatch) {
		foreach (var _wave in this.waveSpecs) {
			if (_wave.SpawnLevelNumber != levelToMatch || _wave.SpawnWaveNumber != waveToMatch) {
				continue;
			}
			
			// found the match, get outa here!!
			return _wave;
		}
		
		return null;
	}
	
	private void LogAdjustments(int adjustments) {
		if (adjustments > 0) {
			Debug.Log(string.Format("Adjusted {0} wave(s) in spawner '{1}' to match new Level/Wave numbers", adjustments, this.name));
		}
	}
	
	public void DeleteLevel(int level) {
		var deadWaves = new List<WaveSpecifics>();
		
		foreach (var wrongWave in this.waveSpecs) {
			if (wrongWave.SpawnLevelNumber == level) {
				deadWaves.Add(wrongWave);
			}
		}
		
		foreach (var dead in deadWaves) {
			this.waveSpecs.Remove(dead);
		}
		
		if (deadWaves.Count > 0) {
			Debug.Log(string.Format("Deleted {0} matching wave(s) in spawner '{1}'", deadWaves.Count, this.name));
		}

		int adjusted = 0;
		foreach (var wrongWave in this.waveSpecs) {
			if (wrongWave.SpawnLevelNumber > level) {
				wrongWave.SpawnLevelNumber--;
				adjusted++;
			}
		}
		
		LogAdjustments(adjusted);
	}
	
	public void InsertLevel(int level) {
		int adjustments = 0;
 		
		foreach (var wrongWave in this.waveSpecs) {
			if (wrongWave.SpawnLevelNumber >= level) {
				wrongWave.SpawnLevelNumber++;
				adjustments++;
			}
		}
		
		LogAdjustments(adjustments);
	}
	
	public void InsertWave(int newWaveNumber, int level) {
		int adjustments = 0;
		
		foreach (var wrongWave in this.waveSpecs) {
			if (wrongWave.SpawnLevelNumber == level && wrongWave.SpawnWaveNumber >= newWaveNumber) {
				wrongWave.SpawnWaveNumber++;
				adjustments++;
			}
		}
		
		LogAdjustments(adjustments);
	}
	
	public void DeleteWave(int level, int wav) {
		var matchingWave = FindWave(level, wav);
		if (matchingWave != null) {
			this.waveSpecs.Remove(matchingWave);
			Debug.Log(string.Format("Deleted matching wave in spawner '{0}'", this.name));
		}
		
		int adjustments = 0;
		
		// move same level, higher waves back one.
		foreach (var wrongWave in this.waveSpecs) {
			if (wrongWave.SpawnLevelNumber == level && wrongWave.SpawnWaveNumber > wav) {
				wrongWave.SpawnWaveNumber--;
				adjustments++;
			}
		}
		
		LogAdjustments(adjustments);
	}
	
	private bool SetupNextWave(bool scanForWave) {
		this.repeatTimer = null;
		
		if (this.activeMode == LevelSettings.ActiveItemMode.Never) { // even in repeating waves.
			return false;
		}
		
		if (scanForWave) {
			// find wave
			this.currentWave = FindWave(LevelSettings.CurrentLevel, LevelSettings.CurrentLevelWave);
		
			// validate for all things that could go wrong!
			if (currentWave == null || !currentWave.enableWave) {
				return false;
			}
	
			// check "active mode" for conditions
			switch (activeMode) {
				case LevelSettings.ActiveItemMode.Never:
					return false;
				case LevelSettings.ActiveItemMode.IfWorldVariableInRange:
					if (activeItemCriteria.statMods.Count == 0) {
						return false;
					}
					for (var i = 0; i < activeItemCriteria.statMods.Count; i++) {
						var stat = activeItemCriteria.statMods[i];
						var varVal = InGameWorldVariable.GetCurrentWorldVariableValue(stat.statName);
					
						if (stat.modValueMin > stat.modValueMax) {
							Debug.LogError("The Min cannot be greater than the Max for Active Item Limit in Syncro Spawner '" + this.trans.name + "'.");
							return false;
						}
					
						if (varVal < stat.modValueMin || varVal > stat.modValueMax) {
							return false;
						}
					}	
					
					break;
				case LevelSettings.ActiveItemMode.IfWorldVariableOutsideRange:
					if (activeItemCriteria.statMods.Count == 0) {
						return false;
					}
					for (var i = 0; i < activeItemCriteria.statMods.Count; i++) {
						var stat = activeItemCriteria.statMods[i];
						var varVal = InGameWorldVariable.GetCurrentWorldVariableValue(stat.statName);
						
						if (stat.modValueMin > stat.modValueMax) {
							Debug.LogError("The Min cannot be greater than the Max for Active Item Limit in Syncro Spawner '" + this.trans.name + "'.");
							return false;
						}
					
						if (varVal >= stat.modValueMin && varVal <= stat.modValueMax) {
							return false;
						}
					}	

					break;
			}
			
			if (currentWave.MinToSpawn == 0 || currentWave.MaxToSpawn == 0){
				return false;	
			}
			
			if (scanForWave && currentWave.WaveDelaySeconds + currentWave.TimeToSpawnWholeWave >= LevelSettings.CurrentWaveInfo.WaveDuration && LevelSettings.CurrentWaveInfo.waveType == LevelSettings.WaveType.Timed) {
				Debug.LogError(string.Format("Wave TimeToSpawnWholeWave plus Wave DelaySeconds must be less than the current LevelSettings wave duration, occured in spawner: {0}, wave# {1}, level {2}.",
					this.name, 
					currentWave.SpawnWaveNumber + 1, 
					currentWave.SpawnLevelNumber + 1));
				return false;
			}
			
			if (currentWave.MinToSpawn > currentWave.MaxToSpawn) {
				Debug.LogError(string.Format("Wave MinToSpawn cannot be greater than Wave MaxToSpawn, occured in spawner: {0}, wave# {1}, level {2}.", 
					this.name, 
					currentWave.SpawnWaveNumber + 1, 
					currentWave.SpawnLevelNumber + 1));
				return false;
			}
			
			if (currentWave.repeatWaveUntilNew && currentWave.repeatPauseMin > currentWave.repeatPauseMax) {
				Debug.LogError(string.Format("Wave Repeat Pause Min cannot be greater than Wave Repeat Pause Max, occurred in spawner: {0}, wave# {1}, level {2}.",
					this.name,
					currentWave.SpawnWaveNumber + 1,
					currentWave.SpawnLevelNumber + 1));
				return false;
			}
		}

		if (LevelSettings.IsLoggingOn) {
			Debug.Log(string.Format("{0} matching wave from spawner: {1}, wave# {2}, level {3}.",
				scanForWave ? "Starting" : "Repeating",
				this.name,
				this.currentWave.SpawnWaveNumber + 1,
				this.currentWave.SpawnLevelNumber + 1));
		}
		
		if (this.currentWave.spawnSource == WaveSpecifics.SpawnOrigin.PrefabPool) {
			var poolTrans = LevelSettings.GetFirstMatchingPrefabPool(this.currentWave.prefabPoolName);
			if (poolTrans == null) {
				Debug.LogError(string.Format("Spawner '{0}' wave# {1}, level {2} is trying to use a Prefab Pool that can't be found.", 
					this.name,
					currentWave.SpawnWaveNumber + 1,
					currentWave.SpawnLevelNumber + 1));
				spawnerValid = false;
				return false;
			}
			
			wavePool = poolTrans;
		} else {
			wavePool = null;
		}
		
		this.spawnedWaveMembers.Clear();
		
		this.currentWaveSize = UnityEngine.Random.Range(currentWave.MinToSpawn, currentWave.MaxToSpawn);
		this.currentWaveLength = (float) currentWave.TimeToSpawnWholeWave;

		if (this.currentWave.repeatWaveUntilNew) {
			if (LevelSettings.CurrentWaveInfo.waveType != LevelSettings.WaveType.Elimination) {
				this.currentWave.repetitions = int.MaxValue;
			}

			this.currentWaveSize += (this.waveRepetitionNumber * this.currentWave.repeatItemIncrease);
			this.currentWaveSize = Math.Min(this.currentWaveSize, this.currentWave.repeatItemLimit); // cannot exceed limits
			
			currentWaveLength += (this.waveRepetitionNumber * this.currentWave.repeatTimeIncrease);
			this.currentWaveLength = Math.Min(this.currentWaveLength, this.currentWave.repeatTimeLimit); // cannot exceed limits
		}
		
		currentWaveLength = Math.Max(0f, currentWaveLength);

		if (scanForWave) { // not on wave repeat!
			this.waveRepetitionNumber = 0;
		}
		
		this.waveStartTime = Time.time;
		this.waveFinishedSpawning = false;
		this.countSpawned = 0;
		this.singleSpawnTime = currentWaveLength / (float) this.currentWaveSize;

		if (this.currentWave.enableLimits) {
			currentRandomLimitDistance = UnityEngine.Random.Range(-this.currentWave.doNotSpawnRandomDistance, this.currentWave.doNotSpawnRandomDistance);
		}
		
		return true;
	}
	
	protected virtual Transform GetSpawnable(WaveSpecifics wave) {	
		if (wave == null) {
			return null;
		}
		
		switch (wave.spawnSource) {
			case WaveSpecifics.SpawnOrigin.Specific:
				return wave.prefabToSpawn;
			case WaveSpecifics.SpawnOrigin.PrefabPool:			
				return wavePool.GetRandomWeightedTransform();
		}
		
		return null;
	}

	private bool IsStrictTimedWaveRepeat {
		get {
			var isTimedWave = LevelSettings.PreviousWaveInfo.waveType == LevelSettings.WaveType.Timed;
			var isStrictRepeat = this.currentWave.curTimedRepeatWaveMode == WaveSpecifics.TimedRepeatWaveMode.StrictTimeStyle;
			return isTimedWave && isStrictRepeat;
		}
	}
	
	public void RemoveSpawnedMember(Transform transMember) {
		if (this.spawnedWaveMembers.Count == 0) {
			// don't remove, this is a strict timed wave.
			return;
		}
		
		if (this.spawnedWaveMembers.Contains(transMember)) {
			this.spawnedWaveMembers.Remove(transMember);
			LevelSettings.RemoveWaveSpawnedItem(transMember);
		}

		this.StartNextEliminationWave();
	}
	
	private void StartNextEliminationWave() {
		if (GameIsOverForSpawner || this.currentWave == null || !this.spawnerValid || !this.waveFinishedSpawning || !currentWave.IsValid) {
			return;
		}

		var hasNoneLeft = this.spawnedWaveMembers.Count == 0;

		if (!hasNoneLeft) {
			return;
		}

		if (LevelSettings.PreviousWaveInfo.waveType == LevelSettings.WaveType.Elimination && (waveRepetitionNumber + 1 >= currentWave.repetitions || !currentWave.repeatWaveUntilNew)) {
			LevelSettings.EliminationSpawnerCompleted(instanceId);
		} else if (currentWave.repeatWaveUntilNew) {
			if (!this.repeatTimer.HasValue) {
				this.repeatTimer = Time.time;
				this.repeatWaitTime = UnityEngine.Random.Range(this.currentWave.repeatPauseMin, this.currentWave.repeatPauseMax);
			} else if (Time.time - this.repeatTimer.Value > this.repeatWaitTime) {
				waveRepetitionNumber++;
				
				MaybeRepeatWave();
			}
		}
	}
	
	private void MaybeRepeatWave() {
		var allPassed = true;

		if (LevelSettings.PreviousWaveInfo.waveType == LevelSettings.WaveType.Elimination) {
			switch (this.currentWave.curWaveRepeatMode) {
				case WaveSpecifics.RepeatWaveMode.NumberOfRepetitions:
				case WaveSpecifics.RepeatWaveMode.Endless:
					WaveRepeat();
					break;
				case WaveSpecifics.RepeatWaveMode.UntilWorldVariableAbove:
					for (var i = 0; i < this.currentWave.repeatPassCriteria.statMods.Count; i++) {
						var stat = this.currentWave.repeatPassCriteria.statMods[i];
						var varVal = InGameWorldVariable.GetCurrentWorldVariableValue(stat.statName);
						
						if (varVal >= stat.modValue) {
							continue;
						}
	
						allPassed = false;	
						break;
					}	
				
					if (!allPassed) {
						WaveRepeat();
					} else {
						LevelSettings.EliminationSpawnerCompleted(instanceId); // since this never happens above due to infinite repetitions
					}
					break;
				case WaveSpecifics.RepeatWaveMode.UntilWorldVariableBelow:
					for (var i = 0; i < this.currentWave.repeatPassCriteria.statMods.Count; i++) {
						var stat = this.currentWave.repeatPassCriteria.statMods[i];
						var varVal = InGameWorldVariable.GetCurrentWorldVariableValue(stat.statName);
						
						if (varVal <= stat.modValue) {
							continue;
						}
	
						allPassed = false;	
						break;
					}	
				
					if (!allPassed) {
						WaveRepeat();
					} else {
						LevelSettings.EliminationSpawnerCompleted(instanceId); // since this never happens above due to infinite repetitions
					}
					break;
			}
		} else if (LevelSettings.PreviousWaveInfo.waveType == LevelSettings.WaveType.Timed) {
			switch (this.currentWave.curTimedRepeatWaveMode) {
				case WaveSpecifics.TimedRepeatWaveMode.EliminationStyle:
					WaveRepeat();
					break;
				case WaveSpecifics.TimedRepeatWaveMode.StrictTimeStyle:
					WaveRepeat();
					break;
			}
		}
	}
}