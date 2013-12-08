#pragma strict

private var weapon : WeaponScript;

function Awake() {
	var weapon : WeaponScript = GetComponent(WeaponScript);
}

function Update () {
	
		// Auto Fire
	if (weapon != null && weapon.CanAttack) {
		weapon.Attack(true);
		}
	
}