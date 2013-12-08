#pragma strict

// Designer Variables 
var shotPrefab : Transform; // Projectile Prefab for shooting
var shootingRate : float = .25; // Cooldown in seconds between 2 shots

private var shootCooldown : float;

// CoolDown

function Start () {
	shootCooldown = 0;
}

function Update () {
	if (shootCooldown > 0) 
		{
		shootCooldown -= Time.deltaTime;
		}
	}
	
// Shooting from other scripts (Enemies etc)
		
function Attack (isEnemy: boolean) {
	if (CanAttack ) {
		shootCooldown = shootingRate;
		
		// Create a new shot
		var shotTransform = Instantiate(shotPrefab) as Transform;
		
		// Assign Position
		shotTransform.position = transform.position;
		
		// The isEnemy Property
		var shot : ShotScript = shotTransform.gameObject.GetComponent(ShotScript);
			if (shot != null) 
			{
				shot.isEnemyShot = isEnemy;
			}
			
			// Make the weapon shot always towards it
			var move : MoveScript = shotTransform.gameObject.GetComponent(MoveScript);
				if (move != null)
				{
				move.direction = this.transform.right;
				}
			}
		};
		
var CanAttack = function () {
	return shootCooldown <= 0;
};