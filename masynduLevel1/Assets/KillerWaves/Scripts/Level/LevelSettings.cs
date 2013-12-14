using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LevelSettings : MonoBehaviour {
	public const string PLAYER_STATS_CONTAINER_TRANS_NAME = "WorldVariables";
	public const string SPAWNER_CONTAINER_TRANS_NAME = "Spawners";
	public const string PREFAB_POOLS_CONTAINER_TRANS_NAME = "PrefabPools";
	public const string REVERT_LEVEL_SETTINGS_ALERT = "Please revert your LevelSettings prefab.";
	public const string NO_SPAWN_CONTAINER_ALERT = "You have no '" + SPAWNER_CONTAINER_TRANS_NAME + "' prefab under LevelSettings. " + REVERT_LEVEL_SETTINGS_ALERT;
	public const string NO_PREFAB_POOLS_CONTAINER_ALERT = "You have no '" + PREFAB_POOLS_CONTAINER_TRANS_NAME + "' prefab under LevelSettings.";
	
	private const float WAVE_CHECK_INTERVAL = .3f; // reduce this to check for spawner activations more often. This is set to 3x a second.
	
	public bool useMusicSettings = true;	
	public bool gameStatsExpanded = false;
	public LevelSettingsListener listener;
	public Texture logoTexture; 
	public Transform RedSpawnerTrans;
	public Transform GreenSpawnerTrans;
	public Transform PrefabPoolTrans;
	public string newSpawnerName = "spawnerName";
	public string newPrefabPoolName = "EnemiesPool";
	public SpawnerType newSpawnerType = SpawnerType.Green;
	public LevelWaveMusicSettings gameOverMusicSettings = new LevelWaveMusicSettings();
	public bool levelsAreExpanded = true;
	public bool createSpawnersExpanded = true;
	public bool createPrefabPoolsExpanded = true;
	public bool disableSyncroSpawners = false;
	public bool enableWaveWarp = false;
	public int warpLevelNumber = 0;
	public int warpWaveNumber = 0;
	public bool isLoggingOn = false;
	public List<LevelSpecifics> LevelTimes = new List<LevelSpecifics>();
	public bool useWaves = true;
	
	private static LevelSettings _lsInstance;
	private static Dictionary<int, List<LevelWave>> waveSettingsByLevel;
	private static int currentLevel;
	private static int currentLevelWave;
	private static bool gameIsOver;
	private static bool wavesArePaused = false;
	private static LevelWave previousWave;
	private static Dictionary<int, WaveSyncroPrefabSpawner> eliminationSpawnersUnkilled = new  Dictionary<int, WaveSyncroPrefabSpawner>();
	private static bool skipCurentWave = false;
	private static List<int> spawnedItemsRemaining = new List<int>();

	private List<WaveSyncroPrefabSpawner> syncroSpawners = new List<WaveSyncroPrefabSpawner>();
	private Transform trans;
	private bool isValid;
	private float lastWaveChangeTime;
	private bool hasFirstWaveBeenStarted;
	
	private static int waveTimeRemaining;
	
	public enum WaveMusicMode {
		KeepPreviousMusic,
		PlayNew,
		Silence
	}

	public enum ActiveItemMode {
		Always,
		Never,
		IfWorldVariableInRange,
		IfWorldVariableOutsideRange
	}
	
	public enum SkipWaveMode {
		None,
		Always,
		IfWorldVariableValueAbove,
		IfWorldVariableValueBelow,
	}
	
	public enum WaveType {
		Timed,
		Elimination
	}
	
	public enum SpawnerType {
		Green,
		Red
	}
	
	void Awake() {
		this.useGUILayout = false;
		this.trans = this.transform;

		hasFirstWaveBeenStarted = false;
		isValid = true;
		wavesArePaused = false;
		int iLevel = 0;		
		currentLevel = 0;
		currentLevelWave = 0;
		previousWave = null;
		skipCurentWave = false;
		
		if (LevelTimes.Count == 0) {
			Debug.LogError("NO LEVEL / WAVE TIMES DEFINED. ABORTING.");
			isValid = false;
			return;
		} else if (LevelTimes[0].WaveSettings.Count == 0) {
			Debug.LogError("NO LEVEL 1 / WAVE 1 TIME DEFINED! ABORTING.");
			isValid = false;
			return;
		}
		
		var levelSettingScripts = GameObject.FindObjectsOfType(typeof(LevelSettings));
		if (levelSettingScripts.Length > 1) {
			Debug.LogError("You have more than one LevelWaveSettings prefab in your scene. Please delete all but one. Aborting.");
			isValid = false;
			return;
		}
		
		waveSettingsByLevel = new Dictionary<int, List<LevelWave>>();
		
		var waveLs = new List<LevelWave>();
		
		for (var i = 0; i < LevelTimes.Count; i++) {
			var level = LevelTimes[i];
			
			if (level.WaveSettings.Count == 0) {
				Debug.LogError("NO WAVES DEFINED FOR LEVEL: " + (iLevel + 1));
				isValid = false;
				continue;
			}
			
			waveLs = new List<LevelWave>();
			LevelWave newLevelWave = null;
			
			foreach (var waveSetting in level.WaveSettings) {
				if (waveSetting.WaveDuration <= 0) {
					Debug.LogError("WAVE DURATION CANNOT BE ZERO OR LESS - OCCURRED IN LEVEL " + (i + 1) + ".");
					isValid = false;
					return;
				}
				
				newLevelWave = new LevelWave() {
					waveType = waveSetting.waveType,
					WaveDuration = waveSetting.WaveDuration,
					musicSettings = new LevelWaveMusicSettings() {
						WaveMusicMode = waveSetting.musicSettings.WaveMusicMode,
						WaveMusicVolume = waveSetting.musicSettings.WaveMusicVolume,
						WaveMusic = waveSetting.musicSettings.WaveMusic,
						FadeTime = waveSetting.musicSettings.FadeTime
					},
					waveName = waveSetting.waveName,
					waveDefeatVariableModifiers = waveSetting.waveDefeatVariableModifiers,
					waveBeatBonusesEnabled = waveSetting.waveBeatBonusesEnabled,
					skipWaveType = waveSetting.skipWaveType,
					skipWavePassCriteria = waveSetting.skipWavePassCriteria
				};
				
				if (waveSetting.waveType == WaveType.Elimination) {
					newLevelWave.WaveDuration = 500; // super long to recognize this problem if it occurs.
				}
				
				waveLs.Add(newLevelWave);
			}
			
			if (i == LevelTimes.Count - 1) { // extra bogus wave so that the real last wave will get run
				newLevelWave = new LevelWave() {
					musicSettings = new LevelWaveMusicSettings() {
						WaveMusicMode = WaveMusicMode.KeepPreviousMusic,
						WaveMusic = null
					},
					WaveDuration = 1
				};
				
				waveLs.Add(newLevelWave);
			}
			
			waveSettingsByLevel.Add(iLevel, waveLs);

			iLevel++;
		}
		
		WaveSyncroPrefabSpawner spawner = null;
		
		foreach (var gObj in GetAllSpawners()) {
			spawner = gObj.GetComponent<WaveSyncroPrefabSpawner>();
			
			syncroSpawners.Add(spawner);
		}
		
		waveTimeRemaining = 0;
		spawnedItemsRemaining.Clear();
		
		gameIsOver = false;
	}
	
	void Start() {
		if (isValid && useWaves) {
			StartCoroutine(this.CoStart());
		}
	}
	
	public static LevelSettings Instance {
		get {
			if (_lsInstance == null) {
				_lsInstance = (LevelSettings) GameObject.FindObjectOfType(typeof(LevelSettings));
			}
			
			return _lsInstance;
		}
	}
	
	IEnumerator CoStart() {
    	while (true) {
        	yield return StartCoroutine(this.CoUpdate());
		}
	}
	
	IEnumerator CoUpdate() {
		yield return new WaitForSeconds(WAVE_CHECK_INTERVAL);

		if (gameIsOver || wavesArePaused) {
            yield break;
		}
		
		WaveType waveType;
		
		//check if level or wave is done.
		if (hasFirstWaveBeenStarted && !skipCurentWave) {
			int timeToCompare = 0;
			
			timeToCompare = ActiveWaveInfo.WaveDuration;
			waveType = ActiveWaveInfo.waveType;

			switch (waveType) {
				case WaveType.Timed:
					var tempTime = (int)(timeToCompare - (Time.time - this.lastWaveChangeTime));
					if (tempTime != TimeRemainingInCurrentWave) {
						TimeRemainingInCurrentWave = tempTime;
					}
					if (Time.time - this.lastWaveChangeTime < timeToCompare) {
                        yield break;
					} else {
						EndCurrentWaveNormally();
					}
					break;
				case WaveType.Elimination:
					if (eliminationSpawnersUnkilled.Count > 0) {
                        yield break;
					}
				
					EndCurrentWaveNormally();
					break;
			}
		}

		if (skipCurentWave && listener != null) {
			listener.WaveEndedEarly(CurrentWaveInfo);
		}
		
		bool waveSkipped = false;
		
		do {
			var waveInfo = CurrentWaveInfo;
	
			if (!disableSyncroSpawners) {
				// notify all synchro spawners
				waveSkipped = SpawnOrSkipNewWave(waveInfo);
				if (waveSkipped) {
					if (isLoggingOn) {
						Debug.Log("Wave skipped - wave# is: " + (currentLevelWave + 1) + " on Level: " + (currentLevel + 1));
					}
				} else {
					waveSkipped = false;
				}
			} else {
				waveSkipped = false;
			}
	
			LevelWaveMusicSettings musicSpec = null;
			
			// change music maybe
			if (currentLevel > 0 && currentLevelWave == 0) {
				if (isLoggingOn) {
					Debug.Log("Level up - new level# is: " + (currentLevel + 1) + " . Wave 1 starting, occurred at time: " + Time.time);
				}
				
				musicSpec = waveInfo.musicSettings;
			} else if (currentLevel > 0 || currentLevelWave > 0) {
				if (isLoggingOn) {
					Debug.Log("Wave up - new wave# is: " + (currentLevelWave + 1) + " on Level: " + (currentLevel + 1) + ". Occured at time: " + Time.time);
				}
	
				musicSpec = waveInfo.musicSettings;
			} else if (currentLevel == 0 && currentLevelWave == 0) {
				musicSpec = waveInfo.musicSettings;
			}
			
			previousWave = CurrentWaveInfo;
			currentLevelWave++;
			
			if (currentLevelWave >= WaveLengths.Count) {
				currentLevelWave = 0;
				currentLevel++;
				
				if (!gameIsOver && currentLevel >= waveSettingsByLevel.Count)  {
					IsGameOver = true;
					
					musicSpec = gameOverMusicSettings;
					Win();
				}
			}
			
			PlayMusicIfSet(musicSpec);
		} 
		while (waveSkipped);
		
		lastWaveChangeTime = Time.time;
		hasFirstWaveBeenStarted = true;
		skipCurentWave = false;
	}
	
	void OnApplicationQuit() {
		SpawnUtility.AppIsShuttingDown = true; // very important!! Dont' take this out, false debug info will show up.
		InGameWorldVariable.FlushAll();
	}
	
	// Return true to skip wave, false means we started spawning the wave.
	private bool SpawnOrSkipNewWave(LevelWave waveInfo) {
		var skipThisWave = true;

		if (enableWaveWarp) {
			// check for Custom Start Wave and skip all before it
			if (CurrentLevel < warpLevelNumber) {
				return true; // skip
			} else if (CurrentLevel == warpLevelNumber && CurrentLevelWave < warpWaveNumber) {
				return true; // skip
			}
		}
		
		if (waveInfo.skipWavePassCriteria.statMods.Count == 0 || waveInfo.skipWaveType == SkipWaveMode.None) {
			skipThisWave = false;
		}

		if (skipThisWave) {
			switch (waveInfo.skipWaveType) {
				case SkipWaveMode.Always:
					break;
				case SkipWaveMode.IfWorldVariableValueAbove:
					for (var i = 0; i < waveInfo.skipWavePassCriteria.statMods.Count; i++) {
						var stat = waveInfo.skipWavePassCriteria.statMods[i];
						var varVal = InGameWorldVariable.GetCurrentWorldVariableValue(stat.statName);
						
						if (varVal < stat.modValue) {
							skipThisWave = false;
							break;
						}
					}	
				
					break;
				case SkipWaveMode.IfWorldVariableValueBelow:
					for (var i = 0; i < waveInfo.skipWavePassCriteria.statMods.Count; i++) {
						var stat = waveInfo.skipWavePassCriteria.statMods[i];
						var varVal = InGameWorldVariable.GetCurrentWorldVariableValue(stat.statName);
						
						if (varVal > stat.modValue) {
							skipThisWave = false;
							break;
						}
					}	
				
					break;
			}
		}

		if (skipThisWave) {
			if (listener != null) {
				listener.WaveSkipped(waveInfo);
			}
			return true;
		}
		
		SpawnNewWave(waveInfo);
		return false;
	}
	
	private void SpawnNewWave(LevelWave waveInfo) {
		eliminationSpawnersUnkilled.Clear();
		spawnedItemsRemaining.Clear();
		WaveRemainingItemsChanged();
		
		foreach(var syncro in syncroSpawners) {
			if (!syncro.WaveChange()) { // returns true if wave found.
				continue;
			}
		
			switch (waveInfo.waveType) {
				case WaveType.Elimination:
					eliminationSpawnersUnkilled.Add(syncro.GetInstanceID(), syncro);
					break;
				case WaveType.Timed:
					TimeRemainingInCurrentWave = CurrentWaveInfo.WaveDuration;
					break;
			}
		}
		
		if (this.listener != null) {
			this.listener.WaveStarted(CurrentWaveInfo);
		}
	}
	
	public static void EliminationSpawnerCompleted(int instanceId) {
		eliminationSpawnersUnkilled.Remove(instanceId);
	}
	
	public static void EndWave() {
		skipCurentWave = true;
	}
	
	public void EndCurrentWaveNormally() {
		// check for wave end bonuses
		if (ActiveWaveInfo.waveBeatBonusesEnabled && ActiveWaveInfo.waveDefeatVariableModifiers.statMods.Count > 0) {
			if (this.listener != null) {
				listener.WaveCompleteBonusesStart(ActiveWaveInfo.waveDefeatVariableModifiers.statMods);
			}
			
			WorldVariableModifier mod = null;
			
			for (var i = 0; i < ActiveWaveInfo.waveDefeatVariableModifiers.statMods.Count; i++) {
				mod = ActiveWaveInfo.waveDefeatVariableModifiers.statMods[i];
				WorldVariableTracker.ModifyPlayerStat(this.trans, mod.statName, mod.modValue);
			}
		}
		
		if (listener != null) {
			listener.WaveEnded(CurrentWaveInfo);
		}
	}
	
	private void Win() {
		if (listener != null) {
			listener.Win();
		}
	}
	
	private static void PlayMusicIfSet(LevelWaveMusicSettings musicSpec) {
		if (LevelSettings.Instance.useMusicSettings && musicSpec != null) {
			WaveMusicChanger.WaveUp(musicSpec);
		}
	}
	
	public List<Transform> GetAllSpawners() {
		var spawnContainer = this.transform.FindChild(SPAWNER_CONTAINER_TRANS_NAME);
		
		if (spawnContainer == null) {
			Debug.LogError(NO_SPAWN_CONTAINER_ALERT);
			return null;
		}
		
		var spawners = new List<Transform>();
		for (var i = 0; i < spawnContainer.childCount; i++) {
			spawners.Add(spawnContainer.GetChild(i));
		}
		
		return spawners;
	}
	
	#region Properties
	public static int LastLevel {
		get {
			return waveSettingsByLevel.Count;
		}
	}
	
	public static int CurrentLevelWave {
		get {
			return currentLevelWave;
		}
	}
	
	public static int CurrentLevel {
		get {
			return currentLevel;
		}
	}
	
	public static int CurrentWaveLength {
		get {
			return 0;
		}
	}
	
	public static List<LevelWave> WaveLengths {
		get {
			return  waveSettingsByLevel[currentLevel];
		}
	}
	
	public static LevelWave CurrentWaveInfo {
		get {
			var waveInfo = WaveLengths[currentLevelWave];
			return waveInfo;
		}
	}
	
	public static LevelWave PreviousWaveInfo {
		get {
			return previousWave;
		}
	}
	
	public static LevelWave ActiveWaveInfo { // This is the only one you would read from code. "CurrentWaveInfo" is to be used by spawners only.
		get {
			LevelWave wave;
			if (previousWave != null) {
				wave = previousWave;
			} else {
				wave = CurrentWaveInfo;
			}
			
			return wave;
		}
	}
	
	public static bool IsLoggingOn {
		get {
			return LevelSettings.Instance != null && LevelSettings.Instance.isLoggingOn;
		}
	}
	
	public static LevelSettingsListener Listener {
		get {
			if (SpawnUtility.AppIsShuttingDown) {
				return null;
			}
			
			return LevelSettings.Instance.listener;
		}
	}
	
	public static bool IsGameOver {
		get {
			return gameIsOver;
		}
		set {
			bool wasGameOver = gameIsOver;
			gameIsOver = value;
			
			if (gameIsOver) {
				if (!wasGameOver) {
					if (Listener != null) {
						Listener.GameOver();
					}
				}
				
				var musicSpec = LevelSettings.Instance.gameOverMusicSettings;
				
				PlayMusicIfSet(musicSpec);
			}
		}
	}
	
	public static void AddWaveSpawnedItem(Transform _trans) {
		var id = _trans.GetInstanceID();
		
		if (spawnedItemsRemaining.Contains(id)) {
			return;
		}
		
		spawnedItemsRemaining.Add (id);
		WaveRemainingItemsChanged();
	}

	public static void RemoveWaveSpawnedItem(Transform _trans) {
		var id = _trans.GetInstanceID();
		
		if (!spawnedItemsRemaining.Contains(id)) {
			return;
		}
		
		spawnedItemsRemaining.Remove(id);
		WaveRemainingItemsChanged();
	}
	
	private static int WaveRemainingItemCount {
		get {
			return spawnedItemsRemaining.Count;
		} 
	}
	
	private static void WaveRemainingItemsChanged() {
		if (Listener != null) {				
			Listener.WaveItemsRemainingChanged(WaveRemainingItemCount);
		}
	}
	
	public static int TimeRemainingInCurrentWave {
		get {
			LevelWave wave = ActiveWaveInfo;
				
			switch (wave.waveType) {
				case WaveType.Elimination:
					return -1;
				case WaveType.Timed:
					return waveTimeRemaining;
			}
			
			return -1;
		} 
		set {
			waveTimeRemaining = value;
			
			if (ActiveWaveInfo.waveType == WaveType.Timed && Listener != null) {
				Listener.WaveTimeRemainingChanged(waveTimeRemaining);
			}
		}
	}

	public static List<string> GetSortedPrefabPoolNames() {
		var poolsHolder = GetPoolsHolder;

		if (poolsHolder == null) {
			//Debug.LogError(LevelSettings.NO_PREFAB_POOLS_CONTAINER_ALERT);
			return null;
		}
		
		var pools = new List<string>();
		
		for (var i = 0; i < poolsHolder.childCount; i++) {
			var oChild = poolsHolder.GetChild(i);
			pools.Add(oChild.name);
		}
		
		pools.Sort();
		
		pools.Insert(0, "-None-");
		
		return pools;
	}
	
	public static LevelSettings GetLevelSettings {
		get {
			return (LevelSettings) GameObject.FindObjectOfType(typeof(LevelSettings));		
		}
	}
	
	private static Transform GetPoolsHolder {
		get {
			var lev = GetLevelSettings;
			if (lev == null) {
				return null;
			}
			
			return lev.transform.FindChild(PREFAB_POOLS_CONTAINER_TRANS_NAME);
		}
	}
	
	public static WavePrefabPool GetFirstMatchingPrefabPool(string poolName) {
		var poolsHolder = GetPoolsHolder;

		if (poolsHolder == null) {
			return null;
		}
		
		var oChild = poolsHolder.FindChild(poolName);
		
		if (oChild == null) {
			return null;
		}
		
		return oChild.GetComponent<WavePrefabPool>();
	}
	
	public static void PauseWave() {
		wavesArePaused = true;
	}

	public static void UnpauseWave() {
		wavesArePaused = false;
	}
	
	public static bool WavesArePaused {
		get {
			return wavesArePaused;
		}
	}
	
	#endregion
}
