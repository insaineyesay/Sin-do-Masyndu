using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Killer Waves/Utility/Killable Child Collision")]
public class KillableChildCollision : MonoBehaviour {
	public Killable killable = null;
	
	private bool isValid = true;
	
	void Awake() {
		if (killable != null) {
			return;
		}
		
		if (this.transform.parent != null) {
			var parentKill = this.transform.parent.GetComponent<Killable>();
			
			if (parentKill != null) {
				killable = parentKill;
			}
		}
		
		if (killable == null) {
			Debug.Log("Could not locate Killable to alert from KillableChildCollision script on GameObject '" + this.name + "'.");
			isValid = false;
		}
	}
	
	
	void OnCollisionEnter(Collision collision) {
		if (!isValid) {
			return;
		}
		
		killable.CollisionEnter(collision);
	}
	
	void OnTriggerEnter(Collider other) {
		if (!isValid) {
			return;
		}
		
		killable.TriggerEnter(other);
	}

	#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		// not supported
	#else
		void OnCollisionEnter2D(Collision2D coll) {
			if (!isValid) {
				return;
			}
			
			killable.CollisionEnter2D(coll);
		}
	
		void OnTriggerEnter2D(Collider2D other) {
			if (!isValid) {
				return;
			}
			
			killable.TriggerEnter2D(other);
		}
	#endif
}
