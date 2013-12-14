using UnityEngine;
using System.Collections;

public class KW_Shot3 : MonoBehaviour {
	private Transform trans;
	private float sideMovement;
	
	void Awake() {
		this.useGUILayout = false;
		this.trans = this.transform;
		this.sideMovement = Random.Range(-6, 6) * Time.deltaTime;
	}
	
	// Update is called once per frame
	void Update () {
		this.trans.Rotate(Vector3.down * 300 * Time.deltaTime);
		
		var pos = this.trans.position;
		pos.x += this.sideMovement;
		pos.z -= 120 * Time.deltaTime;
		this.trans.position = pos;
	}
}
