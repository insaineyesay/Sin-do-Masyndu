using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class WavePrefabPoolItem {
	public Transform prefabToSpawn;
	public LevelSettings.ActiveItemMode activeMode = LevelSettings.ActiveItemMode.Always;
	public WorldVariableRangeCollection activeItemCriteria = new WorldVariableRangeCollection();
	public int weight = 1;
	public bool isExpanded = true;
}
