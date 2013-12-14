using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Killer Waves/Utility/Killable Child Visibility")]
public class KillableChildVisibility : MonoBehaviour {
	public Killable killableWithRenderer = null;
	
	private bool isValid = true;
	
	void Awake() {
		if (killableWithRenderer != null) {
			return;
		}
		
		if (this.transform.parent != null) {
			var parentKill = this.transform.parent.GetComponent<Killable>();
			
			if (parentKill != null) {
				killableWithRenderer = parentKill;
			}
		}
		
		if (killableWithRenderer == null) {
			Debug.Log("Could not locate Killable to alert from KillableChildVisibility script on GameObject '" + this.name + "'.");
			isValid = false;
		}
	}
	
	void OnBecameVisible() {
		if (!isValid) {
			return;
		}
		
		killableWithRenderer.BecameVisible();
	}
	
	void OnBecameInvisible() {
		if (!isValid) {
			return;
		}

		killableWithRenderer.BecameInvisible();
	}
}
