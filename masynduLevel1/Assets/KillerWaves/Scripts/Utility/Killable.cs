using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Dark Tonic/Killer Waves/Utility/Killable")]
public class Killable : MonoBehaviour {
	public const string DESTROYED_TEXT = "Destroyed";
	public const float COROUTINE_INTERVAL = .2f;
	public const int MAX_HIT_POINTS = 100000;
	public const int MAX_ATTACK_POINTS = 100000;
	
	public TriggeredSpawner.GameOverBehavior gameOverBehavior = TriggeredSpawner.GameOverBehavior.Disable;
	public int HitPoints = 1;
	public int AttackPoints = 1;
	public Transform ExplosionPrefab;
	public KillableListener listener;

	public bool enableLogging = false;
	public bool filtersExpanded = true;
	public bool useLayerFilter = false;
	public bool useTagFilter = false;
	public bool showVisibilitySettings = true; 
	public bool despawnWhenOffscreen = false;
	public bool despawnOnClick = false;
	public bool despawnIfNotVisible = false;
	public float despawnIfNotVisibleForTime = 5f;
	public bool ignoreOffscreenHits = true;
	public List<string> matchingTags = new List<string>() { "Untagged" };
	public List<int> matchingLayers = new List<int>() { 0 };
	public DespawnMode despawnMode = DespawnMode.ZeroHitPoints;
	
	// death player stat mods
	public bool despawnStatModifiersExpanded = false;
	public WorldVariableCollection playerStatDespawnModifiers = new WorldVariableCollection();
	public List<WorldVariableCollection> alternateModifiers = new List<WorldVariableCollection>();
	
	// damage prefab settings
	public bool damagePrefabExpanded = false;
	public SpawnSource damagePrefabSource = SpawnSource.None;
	public int damagePrefabPoolIndex = 0;
	public string damagePrefabPoolName = null;
	public Transform damagePrefabSpecific;
	public DamagePrefabSpawnMode damagePrefabSpawnMode = DamagePrefabSpawnMode.None;
	public int damagePrefabSpawnQty = 1;
	public int damageGroupSize = 1;
	private int damageTaken = 0;
	private int damagePrefabsSpawned = 0;
	private WavePrefabPool damagePrefabWavePool = null;
	public bool damagePrefabRandomizeXRotation = false;
	public bool damagePrefabRandomizeYRotation = false;
	public bool damagePrefabRandomizeZRotation = false;
	
	// death prefab settings
	public WaveSpecifics.SpawnOrigin deathPrefabSource = WaveSpecifics.SpawnOrigin.Specific;
	public int deathPrefabPoolIndex = 0;
	public string deathPrefabPoolName = null;
	public bool deathPrefabSettingsExpanded = false;
	public Transform deathPrefabSpecific;
	public int deathPrefabSpawnPercentage = 100;
	public int deathPrefabQuantity = 1;
	public RotationMode rotationMode = RotationMode.UseDeathPrefabRotation;
	public bool deathPrefabKeepVelocity = true;
	public Vector3 deathPrefabCustomRotation = Vector3.zero;
	private WavePrefabPool deathPrefabWavePool = null;
	
	// retrigger limit settings
	public TriggeredSpawner.RetriggerLimitMode retriggerLimitMode = TriggeredSpawner.RetriggerLimitMode.None;
	public int limitPerXFrames = 1;
	public float limitPerXSeconds = 0.1f;
	public int triggeredLastFrame = -200;
	public float triggeredLastTime = -100f;
	
	
	private Transform trans;
	private Rigidbody body;
	public int currentHitPoints;
	public bool isVisible;
	private bool becameVisible = false;
	private float spawnTime;
	private bool isDespawning = false;
	
	public enum SpawnSource {
		None,
		Specific,
		PrefabPool
	}
	
	public enum DamagePrefabSpawnMode {
		None,
		PerHit,
		PerHitPointLost,
		PerGroupHitPointsLost
	}
	
	public enum RotationMode {
		CustomRotation,
		InheritExistingRotation,
		UseDeathPrefabRotation
	}
	
	public enum DespawnMode {
		ZeroHitPoints,
		LostAnyHitPoints,
		None
	}
	
	void Awake() {
		this.trans = this.transform;
		this.body = this.rigidbody;
		SpawnedOrAwake();
	}
	
	void OnSpawned() {
		SpawnedOrAwake();
	}
	
	protected virtual void SpawnedOrAwake() {
		// anything you want to do each time this is spawned.
		this.isVisible = false;
		this.becameVisible = false;
		this.isDespawning = false;
		this.currentHitPoints = HitPoints;
		this.spawnTime = Time.time;
		
		damageTaken = 0;
		damagePrefabsSpawned = 0;
		
		if (deathPrefabPoolName != null && deathPrefabSource == WaveSpecifics.SpawnOrigin.PrefabPool) {
			deathPrefabWavePool = LevelSettings.GetFirstMatchingPrefabPool(deathPrefabPoolName);
			if (deathPrefabWavePool == null) {
				Debug.Log("Death Prefab Pool '" + deathPrefabPoolName + "' not found for Killable '" + this.name + "'.");
			}
		}
		if (damagePrefabSpawnMode != DamagePrefabSpawnMode.None && damagePrefabPoolName != null && damagePrefabSource == SpawnSource.PrefabPool) {
			damagePrefabWavePool = LevelSettings.GetFirstMatchingPrefabPool(damagePrefabPoolName);
			if (damagePrefabWavePool == null) {
				Debug.Log("Damage Prefab Pool '" + damagePrefabWavePool + "' not found for Killable '" + this.name + "'.");
			}
		}
		
		if (damagePrefabSpawnMode != DamagePrefabSpawnMode.None && damagePrefabSource == SpawnSource.Specific && damagePrefabSpecific == null) {
			Debug.LogError(string.Format("Damage Prefab for '{0}' is not assigned.", this.trans.name));			
		}
	}
	
	void Start() {
		StartCoroutine(this.CoStart());
	}
	
	IEnumerator CoStart() {
    	while (true) {
        	yield return StartCoroutine(this.CoUpdate());
		}
	}
	
	IEnumerator CoUpdate() {
		yield return new WaitForSeconds(COROUTINE_INTERVAL);
		
		if (!this.despawnIfNotVisible || this.becameVisible) {
            yield break;
		}
		
		if (Time.time - this.spawnTime > this.despawnIfNotVisibleForTime) {
			this.Despawn(TriggeredSpawner.EventType.Invisible);
		}
	}
	
	void OnClick() {
		_OnClick();
	}
	
	protected virtual void _OnClick() {
		if (this.despawnOnClick) {
			this.Despawn(TriggeredSpawner.EventType.OnClick);
		}
	}
	
	void OnBecameVisible() {
		BecameVisible();
	}
	
	public virtual void BecameVisible() {
		if (this.isVisible) {
			return; // to fix Unity bug.
		}
		
		this.isVisible = true;
		this.becameVisible = true;
	}
	
	void OnBecameInvisible() {
		BecameInvisible();
	}
	
	public virtual void BecameInvisible() {
		this.isVisible = false;
		
		if (despawnWhenOffscreen) {
			this.Despawn(TriggeredSpawner.EventType.Invisible);
		}
	}

	#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		// not supported
	#else
		void OnCollisionEnter2D(Collision2D coll) {
			CollisionEnter2D(coll);
		}
		
		public virtual void CollisionEnter2D(Collision2D collision) {
			if (!IsValidHit(collision.gameObject.layer, collision.gameObject.tag)) {
				return;
			}
			
			var enemy = collision.gameObject.GetComponent<Killable>();
			
			CheckForAttackPoints(enemy);
		}

		void OnTriggerEnter2D(Collider2D other) {
			TriggerEnter2D(other);
		}

		public virtual void TriggerEnter2D(Collider2D other) {
			if (!IsValidHit(other.gameObject.layer, other.gameObject.tag)) {
				return;
			}
		
			var enemy = other.GetComponent<Killable>();
		
			CheckForAttackPoints(enemy);
		}
	#endif

	void OnCollisionEnter(Collision collision) {
		CollisionEnter(collision);
	}
	
	public virtual void CollisionEnter(Collision collision) {
		if (!IsValidHit(collision.gameObject.layer, collision.gameObject.tag)) {
			return;
		}
		
		var enemy = collision.gameObject.GetComponent<Killable>();
		
		CheckForAttackPoints(enemy);
	}
	
	void OnTriggerEnter(Collider other) {
		TriggerEnter(other);
	}
	
	public virtual void TriggerEnter(Collider other) {
		if (!IsValidHit(other.gameObject.layer, other.gameObject.tag)) {
			return;
		}
		
		var enemy = other.GetComponent<Killable>();
		
		CheckForAttackPoints(enemy);
	}
	
	private void CheckForAttackPoints(Killable enemy) {
		if (enemy == null) {
			LogIfEnabled("Not taking any damage because we collided with a non-Killable object.");
			return;
		}

		if (this.listener != null) {
			this.listener.TakingDamage(enemy.AttackPoints, enemy);
		}
		
		this.TakeDamage(enemy.AttackPoints);		
	}

	private bool GameIsOverForKillable {
		get {
			return LevelSettings.IsGameOver && this.gameOverBehavior == TriggeredSpawner.GameOverBehavior.Disable;
		}
	}
	
	private bool IsValidHit(int layer, string tag) {
		if (GameIsOverForKillable) {
			LogIfEnabled("Invalid hit because game is over for Killable. Modify Game Over Behavior to get around this.");
			return false;
		}

		// check filters for matches if turned on
		if (useLayerFilter && !matchingLayers.Contains(layer)) {
			LogIfEnabled("Invalid hit because layer of other object is not in the Layer Filter.");
			return false;
		}
		
		if (useTagFilter && !matchingTags.Contains(tag)) {
			LogIfEnabled("Invalid hit because tag of other object is not in the Tag Filter.");
			return false;
		}

		if (!this.isVisible && this.ignoreOffscreenHits) {
			LogIfEnabled("Invalid hit because Killable is set to ignore offscreen hits and is invisible or offscreen right now. Consider using the KillableChildVisibility script if the Renderer is in a child object.");
			return false;
		}
		
		switch (this.retriggerLimitMode) {
			case TriggeredSpawner.RetriggerLimitMode.FrameBased:
				if (Time.frameCount - this.triggeredLastFrame < this.limitPerXFrames) {
					LogIfEnabled("Invalid hit - has been limited by frame count. Not taking damage from current hit.");
					return false;	
				}
				break;
			case TriggeredSpawner.RetriggerLimitMode.TimeBased:
				if (Time.time - this.triggeredLastTime < this.limitPerXSeconds) {
					LogIfEnabled("Invalid hit - has been limited by time since last hit. Not taking damage from current hit.");
					return false;	
				}
				break;
		}
		

		return true;
	}
	
	public virtual void TakeDamage(int damagePoints) {
		LogIfEnabled("Taking " + damagePoints + " points damage!");
		this.currentHitPoints -= damagePoints;
		
		if (this.currentHitPoints < 0) {
			this.currentHitPoints = 0;
		}
		
		switch (this.retriggerLimitMode) {
			case TriggeredSpawner.RetriggerLimitMode.FrameBased:
				this.triggeredLastFrame = Time.frameCount;
				break;
			case TriggeredSpawner.RetriggerLimitMode.TimeBased:
				this.triggeredLastTime = Time.time;
				break;
		}
	
		SpawnDamagePrefabs(damagePoints);
		
		switch (this.despawnMode) {
			case DespawnMode.ZeroHitPoints:
				if (this.currentHitPoints > 0) {
					return;
				}
				break;
			case DespawnMode.None:
				return;
		}
		
		this.DestroyKillable();
	}
	
	private void SpawnDamagePrefabs(int damagePoints) {
		var numberToSpawn = 0;
		
		switch (this.damagePrefabSpawnMode) {
			case DamagePrefabSpawnMode.None:
				return;
			case DamagePrefabSpawnMode.PerHit:
				numberToSpawn = 1;
				break;
			case DamagePrefabSpawnMode.PerHitPointLost:
				numberToSpawn = Math.Min(this.HitPoints, damagePoints);
				break;
			case DamagePrefabSpawnMode.PerGroupHitPointsLost:
				damageTaken += damagePoints;
				var numberOfGroups = (int) Math.Floor(damageTaken / (float)damageGroupSize);
				numberToSpawn = numberOfGroups - damagePrefabsSpawned;
				break;
		}
		
		if (numberToSpawn == 0) {
			return;
		}
		
		numberToSpawn *= damagePrefabSpawnQty;
		
		//Debug.Log("spawn: " + numberToSpawn);
		
		for (var i = 0; i < numberToSpawn; i++) {
			var prefabToSpawn = CurrentDamagePrefab;
			if (damagePrefabSource == SpawnSource.PrefabPool && prefabToSpawn == null) {
				// empty element in Prefab Pool
				continue;
			}
			
			var spawnedDamagePrefab = SpawnUtility.Spawn(prefabToSpawn, this.trans.position, Quaternion.identity);
			if (spawnedDamagePrefab == null) {
				if (this.listener != null) {
					this.listener.DamagePrefabSpawned(spawnedDamagePrefab);
				}
			} else {
				if (this.listener != null) {
					this.listener.DamagePrefabFailedToSpawn(prefabToSpawn);
				}
				
				// affect the spawned object.
				Vector3 euler = prefabToSpawn.rotation.eulerAngles;
		
				if (this.damagePrefabRandomizeXRotation) {
					euler.x = UnityEngine.Random.Range(0f, 360f);			
				}
				if (this.damagePrefabRandomizeYRotation) {
					euler.y = UnityEngine.Random.Range(0f, 360f);			
				}
				if (this.damagePrefabRandomizeZRotation) {
					euler.z = UnityEngine.Random.Range(0f, 360f);			
				}
				
				spawnedDamagePrefab.rotation = Quaternion.Euler(euler);
			}
		}
		
		// clean up
		damagePrefabsSpawned += numberToSpawn;
	}

	public virtual void DestroyKillable(string scenarioName = DESTROYED_TEXT) {
		if (listener != null) {
			listener.DestroyingKillable(this);
			scenarioName = listener.DeterminingScenario(this, scenarioName);
		}
		
		if (ExplosionPrefab != null) {
			SpawnUtility.Spawn(ExplosionPrefab, this.trans.position, Quaternion.identity);
		}
		
		if (deathPrefabSource == WaveSpecifics.SpawnOrigin.Specific && deathPrefabSpecific == null) {
			// no death prefab.
		} else {
			SpawnDeathPrefabs();
		}
		
		// modify world variables
		if (scenarioName == DESTROYED_TEXT) {
			ModifyWorldVariables(this.playerStatDespawnModifiers);
		} else {
			WorldVariableCollection scenario = alternateModifiers.Find(delegate(WorldVariableCollection obj) {
				return obj.scenarioName == scenarioName;	
			});
			
			if (scenario == null) {
				Debug.LogWarning("Scenario: '" + scenarioName + "' not found in Killable '" + this.trans.name + "'. No World Variables modified by destruction.");
			} else {
				ModifyWorldVariables (scenario);
			}
		}
		
		this.Despawn(TriggeredSpawner.EventType.LostHitPoints);
	}
	
	private void ModifyWorldVariables(WorldVariableCollection modCollection) {
		if (modCollection.statMods.Count > 0 && this.listener != null) {
			this.listener.ModifyingWorldVariables(modCollection.statMods);
		}
		foreach (var modifier in modCollection.statMods) {
			WorldVariableTracker.ModifyPlayerStat(this.trans, modifier.statName, modifier.modValue);
		}
	}
	
	private void SpawnDeathPrefabs() {
		if (UnityEngine.Random.Range(0, 100) < this.deathPrefabSpawnPercentage) {
			for (var i = 0; i < this.deathPrefabQuantity; i++) {
				Transform deathPre = CurrentDeathPrefab;
				
				if (deathPrefabSource == WaveSpecifics.SpawnOrigin.PrefabPool && deathPre == null) {
					continue; // nothing to spawn
				}

				var spawnRotation = deathPre.transform.rotation;
				switch (this.rotationMode) {
					case RotationMode.InheritExistingRotation:
						spawnRotation = this.trans.rotation;
						break;
					case RotationMode.CustomRotation:
						spawnRotation = Quaternion.Euler(deathPrefabCustomRotation);
						break;
				}

				var spawnedDeathPrefab = SpawnUtility.Spawn(deathPre, this.trans.position, spawnRotation);
			
				if (spawnedDeathPrefab != null) {
					if (this.listener != null) {
						this.listener.DeathPrefabSpawned(spawnedDeathPrefab);
					}
					
					if (spawnedDeathPrefab.rigidbody != null && !spawnedDeathPrefab.rigidbody.isKinematic
						&& body != null && !body.isKinematic) {
					
						spawnedDeathPrefab.rigidbody.velocity = body.velocity;
					}
				} else {
					if (this.listener != null) {
						this.listener.DeathPrefabFailedToSpawn(deathPre);
					}
				}
			}
		}
	}
	
	public virtual void Despawn(TriggeredSpawner.EventType eType) {
		if (SpawnUtility.AppIsShuttingDown || this.isDespawning) {
			return;
		}
		
		this.isDespawning = true;

		if (this.listener != null) {
			this.listener.Despawning(eType);
		}
		
		SpawnUtility.Despawn(this.trans);
	}
	
	private Transform CurrentDeathPrefab {	
		get {
			switch (deathPrefabSource) {
				case WaveSpecifics.SpawnOrigin.Specific:
					return deathPrefabSpecific;
				case WaveSpecifics.SpawnOrigin.PrefabPool:			
					return deathPrefabWavePool.GetRandomWeightedTransform();
			}
			
			return null;
		}
	}
	
	private Transform CurrentDamagePrefab {	
		get {
			switch (damagePrefabSource) {
				case SpawnSource.Specific:
					return damagePrefabSpecific;
				case SpawnSource.PrefabPool:			
					return damagePrefabWavePool.GetRandomWeightedTransform();
			}
			
			return null;
		}
	}
	
	private void LogIfEnabled(string msg) {
		if (!enableLogging) {
			return;
		}
		
		Debug.Log("Killable '" + trans.name + "' log: " + msg);
	}
	
	public void AddHitPoints(int pointsToAdd) {
		HitPoints += pointsToAdd;
		if (HitPoints < 0) {
			HitPoints = 0;
		}
		currentHitPoints += pointsToAdd;
		if (currentHitPoints < 0) {
			currentHitPoints = 0;
		}
	}
}
