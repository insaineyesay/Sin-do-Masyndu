#pragma strict

var speed : Vector2 = new Vector2(50,50);

function Update () {

	var shoot = Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2");
	
	if (shoot)
		{
			var WeaponScript: WeaponScript = GetComponent(WeaponScript);
			if (WeaponScript != null)
				{
				// false becasue the player is not an enemy
					WeaponScript.Attack(false);
				}
		}
		
	var inputX : float = Input.GetAxis("Horizontal");
	var inputY : float = Input.GetAxis("Vertical");
	
	var movement = new Vector3(
		speed.x * inputX,
		speed.y * inputY,
		0);
		
	movement *= Time.deltaTime;
	
	transform.Translate(movement);
}