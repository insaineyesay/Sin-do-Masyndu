using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable] 
public class LevelSpecifics {
	public List<LevelWave> WaveSettings = new List<LevelWave>();
	public bool isExpanded = true;
}
