#pragma strict

private var weapons = [];
private var hasSpawn;
private var moveScript : MoveScript;

function Awake() {
	weapons = GetComponentsInChildren(WeaponScript);
	
	// Retrieve scripts to disable when not spawn
	var moveScript : MoveScript = GetComponent(MoveScript);
}

function Start () {
	hasSpawn = false;
	
	// Disable everything
		// -- collider
		collider2D.enabled = false;
		
		// -- moving
		// moveScript.enabled = false;
		
		// -- Shooting
		for (var weapon : WeaponScript in weapons) {
			weapon.enabled = false;
			}
	}

function Update () {

	// Check if the enemy has spawned
	if(hasSpawn == false)
		{
			if(renderer.isVisible)
				{
					Spawn();
				}
				
		}
}

function Spawn() {
	hasSpawn = true;
	
	// Enable Everything
	collider2D.enabled = true;
	
	// moveScript.enabled = true;
	
	for (var weapon : WeaponScript in weapons)
	{	
		weapon.enabled = true;
	}

}