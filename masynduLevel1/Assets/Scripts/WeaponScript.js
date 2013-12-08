#pragma strict

var shotPrefab : Transform;
var shootingRate : float = .25;

private var shootCooldown : float;

function Start () {
	shootCooldown = 0;
}

function Update () {
	if (shootCooldown > 0) 
		{
		shootCooldown -= Time.deltaTime;
		}
	}
		
var Attack = function (isEnemy: boolean) {
	if (CanAttack) {
		shootCooldown = shootingRate;
		
		// Create a new shot
		var shotTransform = Instantiate(shotPrefab) as Transform;
		
		// Assign Position
		shotTransform.position = transform.position;
		
		// The isEnemy Property
		var shot = shotTransform.gameObject.GetComponent(ShotScript);
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