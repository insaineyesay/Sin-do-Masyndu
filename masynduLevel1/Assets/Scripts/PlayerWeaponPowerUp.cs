using UnityEngine;
using System.Collections;

public class PlayerWeaponPowerUp : WeaponPowerUp
{
		void Awake ()
		{
					
		}

		void OnTriggerEnter2D (Collider2D other)
		{

				// If the player enters the trigger zone...
				if (other.tag == "WeaponPowerUp") {
						Destroy (other);

						
				}

				if (!hasUpgrade) {
						Destroy (GameObject.FindGameObjectWithTag ("TwinCannons1"));
				}
		}
	
}
