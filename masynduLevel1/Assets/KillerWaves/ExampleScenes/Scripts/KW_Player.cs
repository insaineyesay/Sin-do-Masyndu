using UnityEngine;
using System.Collections;

public class KW_Player : MonoBehaviour {
	public Texture stableShip;
	public Texture leftShip;
	public Texture rightShip;
	
	public GameObject ProjectilePrefab;
	
	private const float MOVE_SPEED = 100f;
	private Transform trans;
	
	// Use this for initialization
	void Awake() {
		this.useGUILayout = false;
		this.trans = this.transform;
	}
	
	// Update is called once per frame
	void Update () {
		var moveAmt = Input.GetAxis("Horizontal") * MOVE_SPEED * Time.deltaTime;
		
		if (moveAmt == 0) {
			this.renderer.materials[0].mainTexture = stableShip;
		} else if (moveAmt > 0) {
			this.renderer.materials[0].mainTexture = rightShip;
		} else {
			this.renderer.materials[0].mainTexture = leftShip;
		}

		var pos = this.trans.position;
		pos.x += moveAmt;
		this.trans.position = pos;
		
		if (Input.GetMouseButtonDown(0)) {
			var spawnPos = this.trans.position;
			spawnPos.z += 15;
			
			SpawnUtility.Spawn(ProjectilePrefab.transform, spawnPos, ProjectilePrefab.transform.rotation); 
		}
	}
}
