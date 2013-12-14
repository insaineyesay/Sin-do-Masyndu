using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Dark Tonic/Killer Waves/Listeners/Killable Listener")]
public class KillableListener : MonoBehaviour {
	public string sourceKillableName;
	
	public virtual void Despawning(TriggeredSpawner.EventType eType) {
		// your code here.
	}
	
	public virtual void TakingDamage(int pointsDamage, Killable enemyHitBy) {
		// your code here.
	}

	public virtual void DamagePrefabSpawned(Transform damagePrefab) {
		// your code here.
	}
	
	public virtual void DamagePrefabFailedToSpawn(Transform damagePrefab) {
		// your code here.  
	}
	
	public virtual void DeathPrefabSpawned(Transform deathPrefab) {
		// your code here.
	}
	
	public virtual void DeathPrefabFailedToSpawn(Transform deathPrefab) {
		// your code here.  
	}

	public virtual void ModifyingWorldVariables(List<WorldVariableModifier> variableModifiers) {
		// your code here. You can change the variable modifiers before they get used if you want.
	}
	
	public virtual void DestroyingKillable(Killable deadKillable) {
		// your code here.
	}
	
	public virtual string DeterminingScenario(Killable deadKillable, string scenario) {
		// if you wish to use logic to change the Scenario, do it here. Example below.
		
		/// if (yourLogicHere == true) {
		///   scenario = "ReachedTower";
		/// }
		
		return scenario; 
	}
}
