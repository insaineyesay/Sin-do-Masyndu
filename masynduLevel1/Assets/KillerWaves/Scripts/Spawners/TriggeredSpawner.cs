using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Dark Tonic/Killer Waves/Spawners/Triggered Spawner")]
public class TriggeredSpawner : MonoBehaviour {
	#region categorizations of event types
	public static List<TriggeredSpawner.EventType> eventsThatCanRepeatWave = new List<TriggeredSpawner.EventType>() {
		EventType.OnEnabled,
		EventType.Visible,
		EventType.OnTriggerEnter,
		EventType.OnTriggerExit,
		EventType.MouseClick,
		EventType.MouseOver,
		EventType.OnCollision,
		EventType.OnSpawned,
		EventType.CodeTriggered1,
		EventType.CodeTriggered2,
		EventType.OnClick,
		EventType.OnCollision2D,
		EventType.OnTriggerEnter2D,
		EventType.OnTriggerExit2D
	};

	public static List<TriggeredSpawner.EventType> eventsWithTagLayerFilters = new List<TriggeredSpawner.EventType>() {
		EventType.OnCollision,
		EventType.OnTriggerEnter,
		EventType.OnTriggerExit,
		EventType.OnCollision2D,
		EventType.OnTriggerEnter2D,
		EventType.OnTriggerExit2D
	};

	public static List<TriggeredSpawner.EventType> eventsWithInflexibleWaveLength = new List<TriggeredSpawner.EventType>() {
		EventType.Invisible,
		EventType.OnDespawned,
		EventType.OnDisabled
	};

	public static List<TriggeredSpawner.EventType> eventsThatCanTriggerDespawn = new List<TriggeredSpawner.EventType>() {
		EventType.MouseClick,
		EventType.MouseOver,
		EventType.OnCollision,
		EventType.OnTriggerEnter,
		EventType.OnTriggerExit,
		EventType.OnCollision2D,
		EventType.OnTriggerEnter2D,
		EventType.OnTriggerExit2D,
		EventType.Visible,
		EventType.OnEnabled,
		EventType.OnSpawned,
		EventType.CodeTriggered1,
		EventType.CodeTriggered2,
		EventType.OnClick
	};
	#endregion
	
	public LevelSettings.ActiveItemMode activeMode = LevelSettings.ActiveItemMode.Always;
	public WorldVariableRangeCollection activeItemCriteria = new WorldVariableRangeCollection();

	public bool hideUnusedEvents = true;
	public GameOverBehavior gameOverBehavior = GameOverBehavior.Disable;
	public WavePauseBehavior wavePauseBehavior = WavePauseBehavior.Disable;
	public SpawnerEventSource eventSourceType = SpawnerEventSource.Self;
	public bool transmitEventsToChildren = true;
	public TriggeredSpawnerListener listener = null;
	
	public TriggeredWaveSpecifics enableWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics disableWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics visibleWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics invisibleWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics mouseOverWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics mouseClickWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics collisionWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics triggerEnterWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics triggerExitWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics spawnedWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics despawnedWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics codeTriggeredWave1 = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics codeTriggeredWave2 = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics clickWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics collision2dWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics triggerEnter2dWave = new TriggeredWaveSpecifics();
	public TriggeredWaveSpecifics triggerExit2dWave = new TriggeredWaveSpecifics();

	private TriggeredWaveMetaData enableWaveMeta = null;
	private TriggeredWaveMetaData disableWaveMeta = null;
	private TriggeredWaveMetaData visibleWaveMeta = null;
	private TriggeredWaveMetaData invisibleWaveMeta = null;
	private TriggeredWaveMetaData mouseOverWaveMeta = null;
	private TriggeredWaveMetaData mouseClickWaveMeta = null;
	private TriggeredWaveMetaData collisionWaveMeta = null;
	private TriggeredWaveMetaData triggerEnterWaveMeta = null;
	private TriggeredWaveMetaData triggerExitWaveMeta = null;
	private TriggeredWaveMetaData spawnedWaveMeta = null;
	private TriggeredWaveMetaData despawnedWaveMeta = null;
	private TriggeredWaveMetaData codeTriggeredWave1Meta = null;
	private TriggeredWaveMetaData codeTriggeredWave2Meta = null;
	private TriggeredWaveMetaData clickWaveMeta = null;
	private TriggeredWaveMetaData collision2dWaveMeta = null;
	private TriggeredWaveMetaData triggerEnter2dWaveMeta = null;
	private TriggeredWaveMetaData triggerExit2dWaveMeta = null;

	private Transform trans;
	private bool isVisible;
	private List<TriggeredSpawner> childSpawners = new List<TriggeredSpawner>();
	
	#region Enums
	public enum SpawnerEventSource {
		ReceiveFromParent,
		Self,
		None
	}
	
	public enum GameOverBehavior {
		BehaveAsNormal,
		Disable
	}

	public enum WavePauseBehavior {
		BehaveAsNormal,
		Disable
	}
	
	public enum RetriggerLimitMode {
		None,
		FrameBased,
		TimeBased
	}
	
	public enum EventType {
		OnEnabled,
		OnDisabled,
		Visible,
		Invisible,
		MouseOver,
		MouseClick,
		OnCollision,
		OnTriggerEnter,
		OnSpawned,
		OnDespawned,
		OnClick,
		CodeTriggered1,
		CodeTriggered2,
		LostHitPoints,
		OnTriggerExit,
		OnCollision2D,
		OnTriggerEnter2D,
		OnTriggerExit2D
	}
	#endregion
		
	void Awake() {
		this.trans = this.transform;
		SpawnedOrAwake();
	}
	
	#region Propogate Events
	private void PropagateEventToChildSpawners(EventType eType) {
		if (!transmitEventsToChildren) {
			return;
		}
		
		if (childSpawners.Count > 0) {
			if (listener != null) {
				listener.EventPropagating(eType, this.trans, childSpawners.Count);
			}
			
			for (var i = 0; i < childSpawners.Count; i++) {
				childSpawners[i].PropagateEventTrigger(eType, this.trans);
			}
		}
	}
	
	public void PropagateEventTrigger(EventType eType, Transform transmitterTrans) {
		if (listener != null) {
			listener.PropagatedEventReceived(eType, transmitterTrans);
		}
		
		switch (eType) {
			case EventType.CodeTriggered1:
				ActivateCodeTriggeredEvent1();
				break;
			case EventType.CodeTriggered2:
				ActivateCodeTriggeredEvent2();
				break;
			case EventType.Invisible:
				_OnBecameInvisible(false);
				break;
			case EventType.MouseClick:
				_OnMouseDown(false);
				break;
			case EventType.MouseOver:
				_OnMouseEnter(false);
				break;
			case EventType.OnClick:
				_OnClick(false);
				break;
			case EventType.OnCollision:
				_OnCollisionEnter(false);
				break;
			case EventType.OnDespawned:
				_OnDespawned(false);
				break;
			case EventType.OnDisabled:
				_DisableEvent(false);
				break;
			case EventType.OnEnabled:
				_EnableEvent(false);
				break;
			case EventType.OnSpawned:
				_OnSpawned(false);
				break;
			case EventType.OnTriggerEnter:
				_OnTriggerEnter(false);
				break;
			case EventType.OnTriggerExit:
				_OnTriggerExit(false);
				break;
			case EventType.Visible:
				_OnBecameVisible(false);
				break;
			#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				// not supported
			#else
				case EventType.OnCollision2D:
					_OnCollision2dEnter(false);
					break;
				case EventType.OnTriggerEnter2D:
					_OnTriggerEnter2D(false);
					break;
				case EventType.OnTriggerExit2D:
					_OnTriggerExit2D(false);
					break;
			#endif
		}
	}
	#endregion
	
	#region OnSpawned
	void OnSpawned() {
		SpawnedOrAwake();

		_OnSpawned(true);
	}
	
	private void _OnSpawned(bool calledFromSelf) {
		var eType = EventType.OnSpawned;
		
		if (!IsWaveValid(spawnedWave, eType, calledFromSelf)) {
			return;
		}

		invisibleWaveMeta = null; // stop "visible" wave.
		
		if (SetupNextWave(spawnedWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, spawnedWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(spawnedWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion
	
	#region CodeTriggeredEvents
	public void ActivateCodeTriggeredEvent1() {
		var eType = EventType.CodeTriggered1;

		if (!IsWaveValid(codeTriggeredWave1, eType, false)) {
			return;
		}
		
		if (SetupNextWave(codeTriggeredWave1, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, codeTriggeredWave1Meta.waveSpec);
			}
		}
		SpawnFromWaveMeta(codeTriggeredWave1Meta, eType);
		
		PropagateEventToChildSpawners(eType);
	}

	public void ActivateCodeTriggeredEvent2() {
		var eType = EventType.CodeTriggered2;

		if (!IsWaveValid(codeTriggeredWave2, eType, false)) {
			return;
		}
		
		if (SetupNextWave(codeTriggeredWave2, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, codeTriggeredWave2Meta.waveSpec);
			}
		}
		SpawnFromWaveMeta(codeTriggeredWave2Meta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion
	
	#region OnEnable
	void OnEnable() { 
		_EnableEvent(true);
	}

	private void _EnableEvent(bool calledFromSelf) {
		var eType = EventType.OnEnabled;

		if (!IsWaveValid(enableWave, eType, calledFromSelf)) {
			return;
		}
		
		disableWaveMeta = null; // stop "disable" wave.
		
		if (SetupNextWave(enableWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, enableWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(enableWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion
	
	#region OnDisable
	void OnDisable() {
		_DisableEvent(true);
	}
	
	private void _DisableEvent(bool calledFromSelf) {
		var eType = EventType.OnDisabled;

		if (!IsWaveValid(disableWave, eType, calledFromSelf)) {
			return;
		}

		enableWaveMeta = null; // stop "enable" wave.
		
		if (SetupNextWave(disableWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, disableWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(disableWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion
	
	#region Pool Manager events (OnSpawned, OnDespawned)
	void OnDespawned() {
		_OnDespawned(true);
	}
	
	private void _OnDespawned(bool calledFromSelf) {
		var eType = EventType.OnDespawned;

		if (!IsWaveValid(despawnedWave, eType, calledFromSelf)) {
			return;
		}

		visibleWaveMeta = null; // stop "visible" wave.
		
		if (SetupNextWave(despawnedWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, despawnedWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(despawnedWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion
	
	#region OnBecameVisible
	void OnBecameVisible() {
		if (this.isVisible) {
			return; // to fix Unity bug.
		}

		_OnBecameVisible(true);
	}
	
	private void _OnBecameVisible(bool calledFromSelf) {
		var eType = EventType.Visible;

		if (!IsWaveValid(visibleWave, eType, calledFromSelf)) {
			return;
		}

		invisibleWaveMeta = null; // stop "visible" wave.
		
		this.isVisible = true;
		
		if (SetupNextWave(visibleWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, visibleWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(visibleWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion
	
	#region OnBecameInvisible
	void OnBecameInvisible() {
		_OnBecameInvisible(true);
	}
	
	private void _OnBecameInvisible(bool calledFromSelf) {
		var eType = EventType.Invisible;

		if (!IsWaveValid(invisibleWave, eType, calledFromSelf)) {
			return;
		}
		
		visibleWaveMeta = null; // stop "visible" wave.
		
		this.isVisible = false;
		
		if (SetupNextWave(invisibleWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, invisibleWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(invisibleWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion
	
	#region OnMouseEnter
	void OnMouseEnter() {
		_OnMouseEnter(true);
	}
	
	private void _OnMouseEnter(bool calledFromSelf) {
		var eType = EventType.MouseOver;

		if (!IsWaveValid(mouseOverWave, eType, calledFromSelf)) {
			return;
		}

		if (SetupNextWave(mouseOverWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, mouseOverWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(mouseOverWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion
	
	#region OnMouseDown
	void OnMouseDown() {
		_OnMouseDown(true);
	}
	
	private void _OnMouseDown(bool calledFromSelf) {
		var eType = EventType.MouseClick;

		if (!IsWaveValid(mouseClickWave, eType, calledFromSelf)) {
			return;
		}

		if (SetupNextWave(mouseClickWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, mouseClickWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(mouseClickWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion
	
	#region NGUI events (onClick)
	void OnClick() {
		_OnClick(true);
	}
	
	private void _OnClick(bool calledFromSelf) {
		var eType = EventType.OnClick;

		if (!IsWaveValid(clickWave, eType, calledFromSelf)) {
			return;
		}
			
		if (SetupNextWave(clickWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, clickWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(clickWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion
	
	#region OnCollisionEnter
	void OnCollisionEnter(Collision collision) {
		// check filters for matches if turned on
		if (collisionWave.useLayerFilter && !collisionWave.matchingLayers.Contains(collision.gameObject.layer)) {
			return;
		}
		
		if (collisionWave.useTagFilter && !collisionWave.matchingTags.Contains(collision.gameObject.tag)) {
			return;
		}
		
		_OnCollisionEnter(true);
	}
	
	private void _OnCollisionEnter(bool calledFromSelf) {
		var eType = EventType.OnCollision;

		if (!IsWaveValid(collisionWave, eType, calledFromSelf)) {
			return;
		}
		
		if (SetupNextWave(collisionWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, collisionWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(collisionWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion
	
	#region OnTriggerEnter
	void OnTriggerEnter(Collider other) {
		// check filters for matches if turned on
		if (triggerEnterWave.useLayerFilter && !triggerEnterWave.matchingLayers.Contains(other.gameObject.layer)) {
			return;
		}
		
		if (triggerEnterWave.useTagFilter && !triggerEnterWave.matchingTags.Contains(other.gameObject.tag)) {
			return;
		}
		
		_OnTriggerEnter(true);
	}
	 
	private void _OnTriggerEnter(bool calledFromSelf) {
		var eType = EventType.OnTriggerEnter;

		if (!IsWaveValid(triggerEnterWave, eType, calledFromSelf)) {
			return;
		}
		
		if (SetupNextWave(triggerEnterWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, triggerEnterWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(triggerEnterWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion

	#region OnTriggerExit
	void OnTriggerExit(Collider other) {
		// check filters for matches if turned on
		if (triggerExitWave.useLayerFilter && !triggerExitWave.matchingLayers.Contains(other.gameObject.layer)) {
			return;
		}
		
		if (triggerExitWave.useTagFilter && !triggerExitWave.matchingTags.Contains(other.gameObject.tag)) {
			return;
		}
		
		_OnTriggerExit(true);
	}
	 
	private void _OnTriggerExit(bool calledFromSelf) {
		var eType = EventType.OnTriggerExit;

		if (!IsWaveValid(triggerExitWave, eType, calledFromSelf)) {
			return;
		}
		
		if (SetupNextWave(triggerExitWave, eType)) {
			if (listener != null) {
				listener.WaveStart(eType, triggerExitWaveMeta.waveSpec);
			}
		}
		SpawnFromWaveMeta(triggerExitWaveMeta, eType);
		
		PropagateEventToChildSpawners(eType);
	}
	#endregion

	#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		// not supported
	#else
		void OnCollisionEnter2D(Collision2D collision) {
			// check filters for matches if turned on
			if (collision2dWave.useLayerFilter && !collision2dWave.matchingLayers.Contains(collision.gameObject.layer)) {
				return;
			}
			
			if (collision2dWave.useTagFilter && !collision2dWave.matchingTags.Contains(collision.gameObject.tag)) {
				return;
			}
			
			_OnCollision2dEnter(true);
		}
		
		private void _OnCollision2dEnter(bool calledFromSelf) {
			var eType = EventType.OnCollision2D;
			
			if (!IsWaveValid(collision2dWave, eType, calledFromSelf)) {
				return;
			}
			
			if (SetupNextWave(collision2dWave, eType)) {
				if (listener != null) {
					listener.WaveStart(eType, collision2dWaveMeta.waveSpec);
				}
			}
			SpawnFromWaveMeta(collision2dWaveMeta, eType);
			
			PropagateEventToChildSpawners(eType);
		}


		void OnTriggerEnter2D(Collider2D other) {
			// check filters for matches if turned on
			if (triggerEnter2dWave.useLayerFilter && !triggerEnter2dWave.matchingLayers.Contains(other.gameObject.layer)) {
				return;
			}
			
			if (triggerEnter2dWave.useTagFilter && !triggerEnter2dWave.matchingTags.Contains(other.gameObject.tag)) {
				return;
			}
			
			_OnTriggerEnter2D(true);
		}
		
		private void _OnTriggerEnter2D(bool calledFromSelf) {
			var eType = EventType.OnTriggerEnter2D;
			
			if (!IsWaveValid(triggerEnter2dWave, eType, calledFromSelf)) {
				return;
			}
			
			if (SetupNextWave(triggerEnter2dWave, eType)) {
				if (listener != null) {
					listener.WaveStart(eType, triggerEnter2dWaveMeta.waveSpec);
				}
			}
			SpawnFromWaveMeta(triggerEnter2dWaveMeta, eType);
			
			PropagateEventToChildSpawners(eType);
		}


		void OnTriggerExit2D(Collider2D other) {
			// check filters for matches if turned on
			if (triggerExit2dWave.useLayerFilter && !triggerExit2dWave.matchingLayers.Contains(other.gameObject.layer)) {
				return;
			}
			
			if (triggerExit2dWave.useTagFilter && !triggerExit2dWave.matchingTags.Contains(other.gameObject.tag)) {
				return;
			}
			
			_OnTriggerExit2D(true);
		}
		
		private void _OnTriggerExit2D(bool calledFromSelf) {
			var eType = EventType.OnTriggerExit2D;
			
			if (!IsWaveValid(triggerExit2dWave, eType, calledFromSelf)) {
				return;
			}
			
			if (SetupNextWave(triggerExit2dWave, eType)) {
				if (listener != null) {
					listener.WaveStart(eType, triggerExit2dWaveMeta.waveSpec);
				}
			}
			SpawnFromWaveMeta(triggerExit2dWaveMeta, eType);
			
			PropagateEventToChildSpawners(eType);
		}
	#endif

	protected virtual void SpawnedOrAwake() {
		this.isVisible = false;
		
		// reset any in-progress waves that were despawned.
		enableWaveMeta = null;
		disableWaveMeta = null;
		visibleWaveMeta = null;
		invisibleWaveMeta = null;
		mouseOverWaveMeta = null;
		mouseClickWaveMeta = null;
		collisionWaveMeta = null;
		triggerEnterWaveMeta = null;
		triggerExitWaveMeta = null;
		spawnedWaveMeta = null;
		despawnedWaveMeta = null;
		codeTriggeredWave1Meta = null;
		codeTriggeredWave2Meta = null;
		clickWaveMeta = null;
		collision2dWaveMeta = null;
		triggerEnter2dWaveMeta = null;
		triggerExit2dWaveMeta = null;

		// scan for and cache child spawners
		childSpawners = GetChildSpawners(trans);
	}
	
	public static List<TriggeredSpawner> GetChildSpawners(Transform _trans) {
		var childSpawn = new List<TriggeredSpawner>();
		if (_trans.childCount > 0) {
			for (var i = 0; i < _trans.childCount; i++) {
				var spawner = _trans.GetChild(i).GetComponent<TriggeredSpawner>();
				
				if (spawner == null ||spawner.eventSourceType != SpawnerEventSource.ReceiveFromParent) {
					continue;
				}
				
				childSpawn.Add(spawner);
			}
		}
		
		return childSpawn;
	}
	
	private bool SpawnerIsPaused {
		get {
			return LevelSettings.WavesArePaused && this.wavePauseBehavior == WavePauseBehavior.Disable;
		}
	}
	
	private bool GameIsOverForSpawner {
		get {
			return LevelSettings.IsGameOver && this.gameOverBehavior == GameOverBehavior.Disable;
		}
	}
	
	private bool IsWaveValid(TriggeredWaveSpecifics wave, EventType eType, bool calledFromSelf) {
		if (GameIsOverForSpawner || !wave.enableWave || !SpawnerIsActive) {
			return false;
		}
		
		switch (eventSourceType) {
			case SpawnerEventSource.Self:
				// just fine in all scenarios.
				break;
			case SpawnerEventSource.ReceiveFromParent:
				if (calledFromSelf) {
					return false;
				}
				break;
			case SpawnerEventSource.None:
				return false; // disabled!
		}
		
		// check for limiting restraints
		switch (wave.retriggerLimitMode) {
			case RetriggerLimitMode.FrameBased:
				if (Time.frameCount - wave.triggeredLastFrame < wave.limitPerXFrames) {
					if (LevelSettings.IsLoggingOn) {
						Debug.LogError(string.Format("{0} Wave of transform: '{1}' was limited by retrigger frame count setting.",
							eType.ToString(),
							this.trans.name
						));
					}
					return false;	
				}
				break;
			case RetriggerLimitMode.TimeBased:
				if (Time.time - wave.triggeredLastTime < wave.limitPerXSeconds) {
					if (LevelSettings.IsLoggingOn) {
						Debug.LogError(string.Format("{0} Wave of transform: '{1}' was limited by retrigger time setting.",
							eType.ToString(),
							this.trans.name
						));
					}
					return false;	
				}
				break;
		}
		
		return true;
	}
	
	private bool HasActiveSpawningWave {
		get {
			return enableWave != null
				|| disableWave != null
				|| visibleWaveMeta != null 
				|| invisibleWaveMeta != null
				|| mouseOverWaveMeta != null
				|| mouseClickWaveMeta != null
				|| collisionWaveMeta != null
				|| triggerEnterWaveMeta != null
				|| triggerExitWaveMeta != null
				|| spawnedWaveMeta != null
				|| despawnedWaveMeta != null
				|| codeTriggeredWave1Meta != null
				|| codeTriggeredWave2Meta != null
				|| clickWaveMeta != null
				|| collision2dWaveMeta != null
				|| triggerEnter2dWaveMeta != null
				|| triggerExit2dWaveMeta != null;
		}
	}
	
	void FixedUpdate() {
		if (GameIsOverForSpawner || SpawnerIsPaused || !HasActiveSpawningWave || !SpawnerIsActive) {
			return;
		}
		
		SpawnFromWaveMeta(enableWaveMeta, EventType.OnEnabled);
		SpawnFromWaveMeta(disableWaveMeta, EventType.OnDisabled);
		SpawnFromWaveMeta(visibleWaveMeta, EventType.Visible);
		SpawnFromWaveMeta(invisibleWaveMeta, EventType.Invisible);
		SpawnFromWaveMeta(mouseOverWaveMeta, EventType.MouseOver);
		SpawnFromWaveMeta(mouseClickWaveMeta, EventType.MouseClick);
		SpawnFromWaveMeta(collisionWaveMeta, EventType.OnCollision);
		SpawnFromWaveMeta(triggerEnterWaveMeta, EventType.OnTriggerEnter);
		SpawnFromWaveMeta(triggerExitWaveMeta, EventType.OnTriggerExit);
		SpawnFromWaveMeta(spawnedWaveMeta, EventType.OnSpawned);
		SpawnFromWaveMeta(despawnedWaveMeta, EventType.OnDespawned);
		SpawnFromWaveMeta(codeTriggeredWave1Meta, EventType.CodeTriggered1);
		SpawnFromWaveMeta(codeTriggeredWave2Meta, EventType.CodeTriggered2);
		SpawnFromWaveMeta(clickWaveMeta, EventType.OnClick);
		SpawnFromWaveMeta(collision2dWaveMeta, EventType.OnCollision2D);
		SpawnFromWaveMeta(triggerExit2dWaveMeta, EventType.OnTriggerEnter2D);
		SpawnFromWaveMeta(triggerExit2dWaveMeta, EventType.OnTriggerExit2D);
	}
	
	private bool CanRepeatWave(TriggeredWaveMetaData wave) {
		switch (wave.waveSpec.curWaveRepeatMode) {
			case WaveSpecifics.RepeatWaveMode.NumberOfRepetitions:
				return wave.waveRepetitionNumber < wave.waveSpec.maxRepeats;
			case WaveSpecifics.RepeatWaveMode.Endless:
				return true;
			case WaveSpecifics.RepeatWaveMode.UntilWorldVariableAbove:
				for (var i = 0; i < wave.waveSpec.repeatPassCriteria.statMods.Count; i++) {
					var stat = wave.waveSpec.repeatPassCriteria.statMods[i];
					var varVal = InGameWorldVariable.GetCurrentWorldVariableValue(stat.statName);
					
					if (varVal < stat.modValue) {
						return true;
					}
				}	
			
				return false;
			case WaveSpecifics.RepeatWaveMode.UntilWorldVariableBelow:
				for (var i = 0; i < wave.waveSpec.repeatPassCriteria.statMods.Count; i++) {
					var stat = wave.waveSpec.repeatPassCriteria.statMods[i];
					var varVal = InGameWorldVariable.GetCurrentWorldVariableValue(stat.statName);
					
					if (varVal > stat.modValue) {
						return true;
					}
				}	
			
				return false;
			default:
				Debug.LogError("Handle new wave repetition type: " + wave.waveSpec.curWaveRepeatMode);
				return false;
		}
	}
	
	private void SpawnFromWaveMeta(TriggeredWaveMetaData wave, EventType eType) {
		if (wave == null || SpawnerIsPaused) {
			return;
		}
		
		if (wave.waveFinishedSpawning 
			|| (Time.time - wave.waveStartTime < wave.waveSpec.WaveDelaySeconds)
			|| (Time.time - wave.lastSpawnTime <= wave.singleSpawnTime && wave.singleSpawnTime > Time.deltaTime)) {
			
			if (wave.waveFinishedSpawning 
				&& wave.waveSpec.enableRepeatWave
				&& CanRepeatWave(wave)
				&& Time.time - wave.previousWaveEndTime > wave.waveSpec.repeatWavePauseTime) {
				
				if (SetupNextWave(wave.waveSpec, eType, wave.waveRepetitionNumber)) {
					if (listener != null) {
						listener.WaveRepeat(eType, wave.waveSpec);
					}
				}
			}
			
			return;
		}
		
		int numberToSpawn = 1;
		
		if (wave.waveSpec.NumberToSpawn > 0) {
			if (wave.singleSpawnTime < Time.deltaTime) {
				if (wave.singleSpawnTime == 0) {
					numberToSpawn = wave.currentWaveSize;
				} else {
					numberToSpawn = (int) Math.Ceiling(Time.deltaTime / wave.singleSpawnTime);
				}
			}
		} else {
			numberToSpawn = 0;
		}
		
		for (var i = 0; i < numberToSpawn; i++) {
			if (this.CanSpawnOne()) {
				Transform prefabToSpawn = this.GetSpawnable(wave);		
				if (wave.waveSpec.spawnSource == WaveSpecifics.SpawnOrigin.PrefabPool && prefabToSpawn == null) {
					// "no item"
					continue;
				}
				if (prefabToSpawn == null) {
					Debug.LogError(string.Format("Triggered Spawner '{0}' has no prefab to spawn for event: {1}", 
						this.name, 
						eType.ToString()));
		
					switch (eType) {
						case EventType.OnEnabled:
							enableWaveMeta = null;
							break;
						case EventType.OnDisabled:
							disableWaveMeta = null;
							break;
						case EventType.Visible:
							visibleWaveMeta = null;
							break;
						case EventType.Invisible:
							invisibleWaveMeta = null;
							break;
						case EventType.MouseOver:
							mouseOverWaveMeta = null;
							break;
						case EventType.MouseClick:
							mouseClickWaveMeta = null;
							break;
						case EventType.OnCollision:
							collisionWaveMeta = null;
							break;
						case EventType.OnTriggerEnter:
							triggerEnterWaveMeta = null;
							break;
						case EventType.OnTriggerExit:
							triggerExitWaveMeta = null;
							break;
						case EventType.OnSpawned:
							spawnedWaveMeta = null;
							break;
						case EventType.OnDespawned:
							despawnedWaveMeta = null;
							break;
						case EventType.CodeTriggered1:
							codeTriggeredWave1Meta = null;
							break;
						case EventType.CodeTriggered2:
							codeTriggeredWave2Meta = null;
							break;
						case EventType.OnClick:
							clickWaveMeta = null;
							break;
						case EventType.OnCollision2D:
							collision2dWaveMeta = null;
							break;
						case EventType.OnTriggerEnter2D:
							triggerEnter2dWaveMeta = null;
							break;
						case EventType.OnTriggerExit2D:
							triggerExit2dWaveMeta = null;
							break;
						default:
							Debug.LogError("need event stop code for event: " + eType.ToString());
							break;
					}
					
					return;
				}

				var spawnPosition = this.GetSpawnPosition(this.trans.position, wave.countSpawned, wave);
				
				var spawnedPrefab = SpawnUtility.Spawn(prefabToSpawn, 
					spawnPosition, this.GetSpawnRotation(prefabToSpawn, wave.countSpawned, wave));
			
				if (spawnedPrefab == null) {
					if (!SpawnUtility.AppIsShuttingDown) {
						if (listener != null) {
							listener.ItemFailedToSpawn(eType, prefabToSpawn);
						}
						
						Debug.Log("Could not spawn: " + prefabToSpawn + " : " + Time.time);
					}
					return;
				}
				
				this.AfterSpawn(spawnedPrefab, wave, eType);
			}
				
			wave.countSpawned++;
			
			if (wave.countSpawned >= wave.currentWaveSize) {
				if (LevelSettings.IsLoggingOn) {
					Debug.Log(string.Format("Triggered Spawner '{0}' finished spawning wave from event: {1}.",
						this.name,
						eType.ToString()));
				}
				wave.waveFinishedSpawning = true;
				if (listener != null) {
					listener.WaveFinishedSpawning(eType, wave.waveSpec);
				}
				
				if (wave.waveSpec.enableRepeatWave) {
					wave.previousWaveEndTime = Time.time;
					wave.waveRepetitionNumber++;
				}
			}
			
			wave.lastSpawnTime = Time.time;
		}
		
		AfterSpawnWave(wave);
	}
	
	private void AfterSpawnWave(TriggeredWaveMetaData newWave) {
		if (newWave.waveSpec.willDespawnOnEvent) {
			if (listener != null) {
				listener.SpawnerDespawning(this.trans);
			}
			SpawnUtility.Despawn(this.trans);
		}
	}
	
	private bool SetupNextWave(TriggeredWaveSpecifics newWave, EventType eventType, int repetionNumber = 0) {
		if (!newWave.enableWave) { // even in repeating waves we need to check.
			return false;
		}
		
		if (LevelSettings.IsLoggingOn) {
			Debug.Log(string.Format("Starting wave from triggered spawner: {0}, event: {1}.",
				this.name,
				eventType.ToString()));
		}
		
		WavePrefabPool myWavePool = null;
		
		if (newWave.spawnSource == WaveSpecifics.SpawnOrigin.PrefabPool) {
			var poolTrans = LevelSettings.GetFirstMatchingPrefabPool(newWave.prefabPoolName);
			if (poolTrans == null) {
				Debug.LogError(string.Format("Spawner '{0}' event: {1} is trying to use a Prefab Pool that can't be found.", 
					this.name,
					eventType.ToString()));
				return false;
			}
			
			myWavePool = poolTrans;
		} else {
			myWavePool = null;
		}
		
		var myCurrentWaveSize = newWave.NumberToSpawn;
		myCurrentWaveSize += (repetionNumber * newWave.repeatItemIncrease);
		myCurrentWaveSize = Math.Min(myCurrentWaveSize, newWave.repeatItemLimit); // cannot exceed limits
		myCurrentWaveSize = Math.Max(0, myCurrentWaveSize);
		
		var timeToSpawnWave = (float) newWave.TimeToSpawnWholeWave;
		timeToSpawnWave += repetionNumber * newWave.repeatTimeIncrease;
		timeToSpawnWave = Math.Min(timeToSpawnWave, newWave.repeatTimeLimit); // cannot exceed limits
		timeToSpawnWave = Math.Max(0f, timeToSpawnWave);
		
		var mySingleSpawnTime = timeToSpawnWave  / (float) myCurrentWaveSize;
		
		var newMetaWave = new TriggeredWaveMetaData() {
			wavePool = myWavePool,
			currentWaveSize = myCurrentWaveSize,
			waveStartTime = Time.time,
			singleSpawnTime = mySingleSpawnTime,
			waveSpec = newWave,
			waveRepetitionNumber = repetionNumber
		};
		
		switch (eventType) {
			case EventType.OnEnabled:
				enableWaveMeta = newMetaWave;
				break;
			case EventType.OnDisabled:
				disableWaveMeta = newMetaWave;
				break;
			case EventType.Visible:
				visibleWaveMeta = newMetaWave;
				break;
			case EventType.Invisible:
				invisibleWaveMeta = newMetaWave;
				break;
			case EventType.MouseOver:
				mouseOverWaveMeta = newMetaWave;
				break;
			case EventType.MouseClick:
				mouseClickWaveMeta = newMetaWave;
				break;
			case EventType.OnCollision:
				collisionWaveMeta = newMetaWave;
				break;
			case EventType.OnTriggerEnter:
				triggerEnterWaveMeta = newMetaWave;
				break;
			case EventType.OnTriggerExit:
				triggerExitWaveMeta = newMetaWave;
				break;
			case EventType.OnSpawned:
				spawnedWaveMeta = newMetaWave;
				break;
			case EventType.OnDespawned:
				despawnedWaveMeta = newMetaWave;
				break;
			case EventType.CodeTriggered1:
				codeTriggeredWave1Meta = newMetaWave;
				break;
			case EventType.CodeTriggered2:
				codeTriggeredWave2Meta = newMetaWave;
				break;
			case EventType.OnClick:
				clickWaveMeta = newMetaWave;
				break;
			case EventType.OnCollision2D:
				collision2dWaveMeta = newMetaWave;
				break;
			case EventType.OnTriggerEnter2D:
				triggerEnter2dWaveMeta = newMetaWave;
				break;
			case EventType.OnTriggerExit2D:
				triggerExit2dWaveMeta = newMetaWave;
				break;
			default:
				Debug.LogError("No matching event type: " + eventType.ToString());
				return false;
		}
		
		switch (newMetaWave.waveSpec.retriggerLimitMode) {
			case RetriggerLimitMode.FrameBased:
				newMetaWave.waveSpec.triggeredLastFrame = Time.frameCount;
				break;
			case RetriggerLimitMode.TimeBased:
				newMetaWave.waveSpec.triggeredLastTime = Time.time;
				break;
		}
		
		return true;
	}
	
	protected virtual Transform GetSpawnable(TriggeredWaveMetaData wave) {	
		switch (wave.waveSpec.spawnSource) {
			case WaveSpecifics.SpawnOrigin.Specific:
				return wave.waveSpec.prefabToSpawn;
			case WaveSpecifics.SpawnOrigin.PrefabPool:			
				return wave.wavePool.GetRandomWeightedTransform();
		}

		return null;
	}
		
	protected virtual bool CanSpawnOne() {
		return true; // this is for later subclasses to override (or ones you make!)
	}
	
	protected virtual Vector3 GetSpawnPosition(Vector3 pos, int itemSpawnedIndex, TriggeredWaveMetaData wave) {
		var addVector = Vector3.zero;
		
		var currentWave = wave.waveSpec;
		
		if (currentWave.enableRandomizations) {
			addVector.x = UnityEngine.Random.Range(-currentWave.randomDistanceX, currentWave.randomDistanceX);
			addVector.y = UnityEngine.Random.Range(-currentWave.randomDistanceY, currentWave.randomDistanceY);
			addVector.z = UnityEngine.Random.Range(-currentWave.randomDistanceZ, currentWave.randomDistanceZ);
		}
		
		if (currentWave.enableIncrements && itemSpawnedIndex > 0) {
			addVector.x += (currentWave.incrementPosX * itemSpawnedIndex);
			addVector.y += (currentWave.incrementPosY * itemSpawnedIndex);
			addVector.z += (currentWave.incrementPosZ * itemSpawnedIndex);
		}
		
		return pos + addVector;
	}
	
	protected virtual Quaternion GetSpawnRotation(Transform prefabToSpawn, int itemSpawnedIndex, TriggeredWaveMetaData wave) {
		var currentWave = wave.waveSpec;
		
		Vector3 euler = prefabToSpawn.rotation.eulerAngles;

		if (currentWave.enableRandomizations && currentWave.randomXRotation) {
			euler.x = UnityEngine.Random.Range(wave.waveSpec.randomXRotationMin, wave.waveSpec.randomXRotationMax);
		} else if (currentWave.enableIncrements && itemSpawnedIndex > 0) {
			euler.x += (itemSpawnedIndex * currentWave.incrementRotationX);
		}
		
		if (currentWave.enableRandomizations && currentWave.randomYRotation) {
			euler.y = UnityEngine.Random.Range(wave.waveSpec.randomYRotationMin, wave.waveSpec.randomYRotationMax);
		} else if (currentWave.enableIncrements && itemSpawnedIndex > 0) {
			euler.y += (itemSpawnedIndex * currentWave.incrementRotationY);
		}
		
		if (currentWave.enableRandomizations &&  currentWave.randomZRotation) {
			euler.z = UnityEngine.Random.Range(wave.waveSpec.randomZRotationMin, wave.waveSpec.randomZRotationMax);
		} else if (currentWave.enableIncrements && itemSpawnedIndex > 0) {
			euler.z += (itemSpawnedIndex * currentWave.incrementRotationZ);
		}

		return Quaternion.Euler(euler);
	}
	
	protected virtual void AfterSpawn(Transform spawnedTrans, TriggeredWaveMetaData wave, EventType eType) {
		var currentWave = wave.waveSpec;
		
		if (currentWave.enablePostSpawnNudge) {
			spawnedTrans.Translate(Vector3.forward * currentWave.postSpawnNudgeForward);
			spawnedTrans.Translate(Vector3.right * currentWave.postSpawnNudgeRight);
			spawnedTrans.Translate(Vector3.down * currentWave.postSpawnNudgeDown);
		}
		
		if (listener != null) {
			listener.ItemSpawned(eType, spawnedTrans);
		}
	}
	
	public bool SpawnerIsActive {
		get {
			switch (activeMode) {
				case LevelSettings.ActiveItemMode.Always:
					return true;
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
							Debug.LogError("The Min cannot be greater than the Max for Active Item Limit in Triggered Spawner '" + this.transform.name + "'.");
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
							Debug.LogError("The Min cannot be greater than the Max for Active Item Limit in Triggered Spawner '" + this.transform.name + "'.");
							return false;
						}
					
						if (varVal >= stat.modValueMin && varVal <= stat.modValueMax) {
							return false;
						}
					}	

					break;
			}
			
			return true;
		}
	}
	
	public bool IsVisible {
		get {
			return this.isVisible;
		}
	}
}