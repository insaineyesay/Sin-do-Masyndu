﻿#pragma strict

var speed : Vector2 = new Vector2(50,50);

function Update () {
	
	var shoot = Input.GetButton("Fire1") || Input.GetButton("Fire2");
	
	if (shoot)
		{
			var WeaponScript: WeaponScript = GetComponent(WeaponScript);
			if (WeaponScript != null)
				{
				// false becasue the player is not an enemy
					WeaponScript.Attack(false);
				}
		}
		
	
	
	// Make sure we are not outside of the camera bounds
	var dist = (transform.position - Camera.main.transform.position).z;
	
	var leftBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).x;
	
	var rightBorder = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, dist)).x;
	
	var topBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).y;
	
	var bottomBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, dist)).y;
	
	transform.position = new Vector3(
		Mathf.Clamp(transform.position.x, leftBorder, rightBorder),
      	Mathf.Clamp(transform.position.y, topBorder, bottomBorder),
      	transform.position.z);
      	
      	// End of the update method
}

