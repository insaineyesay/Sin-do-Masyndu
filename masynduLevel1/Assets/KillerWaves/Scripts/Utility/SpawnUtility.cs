using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class SpawnUtility {
	private static bool appIsShuttingDown = false;

	public static Transform Spawn(Transform spawn, Vector3 spawnPos, Quaternion spawnRotation) {
		if (appIsShuttingDown) { // VERY IMPORTANT! Do not delete this part.
			return null;
		}
		
		/* Non Pool Manager Code */
		var gameObj = GameObject.Instantiate(spawn.gameObject, spawnPos, spawnRotation) as GameObject;
		return gameObj.transform;
		
		/* TO USE POOL MANAGER, USE CODE LIKE THIS INSTEAD OF THE ABOVE */
		//return PoolManager.Pools["YourPoolName"].Spawn(spawn, spawnPos, spawnRotation);
	}
	
	public static void Despawn(Transform spawned) {
		GameObject.Destroy(spawned.gameObject);
		
		/* TO USE POOL MANAGER, USE CODE LIKE THIS INSTEAD OF THE ABOVE */
		//PoolManager.Pools["YourPoolName"].Despawn(spawned);
	}
	
	public static bool SpawnedMembersAreAllBeyondDistance(Transform spawnerTrans, List<Transform> members, float minDist) {
		bool allMembersBeyondDistance = true;
		
		var spawnerPos = spawnerTrans.position;		
		var sqrDist = minDist * minDist;
		
		foreach (var t in members) {
			if (t == null || !IsActive(t.gameObject)) { // .active will work with Pool Manager.
				continue;
			}
			
			if (Vector3.SqrMagnitude(spawnerPos - t.transform.position) < sqrDist) {
				allMembersBeyondDistance = false;
			}
		}
		
		return allMembersBeyondDistance;
	}
	
	public static bool IsActive(GameObject go) {
		#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
			return go.active;
		#else
			return go.activeInHierarchy;
		#endif
	}
	
	public static bool AppIsShuttingDown {
		get {
			return appIsShuttingDown;
		}
		set {
			appIsShuttingDown = value;
		}
	}
	
	public static void SetActive(GameObject go, bool isActive) {
		#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
			go.SetActiveRecursively(isActive);
		#else
			go.SetActive(isActive);
		#endif
	}
}
