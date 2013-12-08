#pragma strict

var hp : int = 2;

var isEnemy = true;

function OnTriggerEnter2D(collider: Collider2D) {
	// Is this a shot?
	var shot : ShotScript = collider.gameObject.GetComponent(ShotScript);
		if(shot != null)
			{
			// Avoid Friendly fire
			if(shot.isEnemyShot != isEnemy){
				hp -= shot.damage;
				
				// Destroy the shot
				// Remember to always target the same object, otherwise the script will be removed
				Destroy(shot.gameObject);
				
				if (hp <= 0) {
					// Dead!
					Destroy(gameObject);
					}
				}
			}
		}