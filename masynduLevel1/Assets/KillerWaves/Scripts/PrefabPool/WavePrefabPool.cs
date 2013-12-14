using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WavePrefabPool : MonoBehaviour {
	public Texture logoTexture;
	public bool isExpanded = true;
	public bool exhaustiveList = true;
	public PoolDispersalMode dispersalMode = PoolDispersalMode.Randomized;
	public WavePrefabPoolListener listener;
	
	private bool isValid = false;
	public List<WavePrefabPoolItem> poolItems;
	private List<int> poolItemIndexes = new List<int>();
	
	public enum PoolDispersalMode {
		Randomized,
		OriginalPoolOrder
	}
	
	void Awake() {
		this.useGUILayout = false;
		FillPool();
	}
	
	void FillPool() {
		// fill weighted pool
		for (var item = 0; item < poolItems.Count; item++) {
			var poolItem = poolItems[item];
	
			bool includeItem = true;
			
			switch (poolItem.activeMode) {
				case LevelSettings.ActiveItemMode.Always:
					break;
				case LevelSettings.ActiveItemMode.Never:
					continue;
				case LevelSettings.ActiveItemMode.IfWorldVariableInRange:
					for (var i = 0; i < poolItem.activeItemCriteria.statMods.Count; i++) {
						var stat = poolItem.activeItemCriteria.statMods[i];
						var varVal = InGameWorldVariable.GetCurrentWorldVariableValue(stat.statName);
					
						if (stat.modValueMin > stat.modValueMax) {
							Debug.LogError("The Min cannot be greater than the Max for Active Item Limit in Prefab Pool '" + this.transform.name + "'. Skipping item '" + poolItem.prefabToSpawn.name + "'.");
							includeItem = false;
							break;
						}
					
						if (varVal < stat.modValueMin || varVal > stat.modValueMax) {
							includeItem = false;
							break;
						}
					}	
					
					break;
				case LevelSettings.ActiveItemMode.IfWorldVariableOutsideRange:
					for (var i = 0; i < poolItem.activeItemCriteria.statMods.Count; i++) {
						var stat = poolItem.activeItemCriteria.statMods[i];
						var varVal = InGameWorldVariable.GetCurrentWorldVariableValue(stat.statName);
						
						if (stat.modValueMin > stat.modValueMax) {
							Debug.LogError("The Min cannot be greater than the Max for Active Item Limit in Prefab Pool '" + this.transform.name + "'. Skipping item '" + poolItem.prefabToSpawn.name + "'.");
							includeItem = false;
							break;
						}
					
						if (varVal >= stat.modValueMin && varVal <= stat.modValueMax) {
							includeItem = false;
							break;
						}
					}	

					break;
			}
			
			if (!includeItem) {
				continue;
			}
			
			for (int i = 0; i < poolItem.weight; i++) {
				poolItemIndexes.Add(item);
			}
		}
		
		if (poolItemIndexes.Count == 0) {
			Debug.LogError("The Prefab Pool '" + this.name + "' has no active Prefab Pool items. Please add some or delete the Prefab pool before continuing. Disabling Killer Waves.");
			LevelSettings.IsGameOver = true;
			return;
		}
	
		isValid = true;
	}
	
	public Transform GetRandomWeightedTransform() {
		if (!isValid) {
			return null;
		}
		
		var index = 0; // for non-random
		if (dispersalMode == PoolDispersalMode.Randomized) {
			index = UnityEngine.Random.Range(0, poolItemIndexes.Count);
		} 
		
		var prefabIndex = poolItemIndexes[index];
		
		if (exhaustiveList || dispersalMode == PoolDispersalMode.OriginalPoolOrder) {
			poolItemIndexes.RemoveAt(index);
			
			if (poolItemIndexes.Count == 0) {
				// refill
				if (LevelSettings.IsLoggingOn) {
					Debug.Log(string.Format("Prefab Pool '{0}' refilling exhaustion list.", 
						this.name));
				}
				
				if (this.listener != null) {
					this.listener.PoolRefilling();
				}
				
				FillPool();
			}
		}

		var spawnable = poolItems[prefabIndex].prefabToSpawn;

		if (LevelSettings.IsLoggingOn) {
			Debug.Log(string.Format("Prefab Pool '{0}' spawning random item '{1}'.", 
				this.name,
				spawnable.name));
		}
	
		if (this.listener != null) {
			this.listener.PrefabGrabbedFromPool(spawnable);
		}
		
		return spawnable;
	}
}
