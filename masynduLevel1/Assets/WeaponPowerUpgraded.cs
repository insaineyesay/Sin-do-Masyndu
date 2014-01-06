using UnityEngine;
using System.Collections;

public class WeaponPowerUpgraded : WeaponPowerUp {


	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Player"){

			IncreasedDamage = 10;
			Debug.Log("attack points are now set to " + IncreasedDamage);
			upgradeTime = 10;
			Debug.Log("Upgrade time is now set to " + upgradeTime);


			Debug.Log("You collected an item!" + hasUpgrade);
		} 
	}
}
