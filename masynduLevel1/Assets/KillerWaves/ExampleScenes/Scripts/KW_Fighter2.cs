using UnityEngine;
using System.Collections;

public class KW_Fighter2 : MonoBehaviour {
	private Transform trans;
	private float zMovement;
	
	void Awake() {
		this.useGUILayout = false;
		this.trans = this.transform;
		this.zMovement = 30.5f;
	}
	
	// Update is called once per frame
	void Update () {
		var pos = this.trans.position;
		
		pos.z -= zMovement * Time.deltaTime;
		
		this.trans.position = pos; 
		
	}
}
