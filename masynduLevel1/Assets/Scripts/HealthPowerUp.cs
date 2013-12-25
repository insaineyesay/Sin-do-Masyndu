using UnityEngine;
using System.Collections;

public class HealthPowerUp : MonoBehaviour
{
	public int healthBonus;				// How much health the crate gives the player.
	public AudioClip collect;				// The sound of the crate being collected.


	void Awake ()
	{

	}


	void OnTriggerEnter2D (Collider2D other)
	{
		// If the player enters the trigger zone...
		if (other.tag == "Player") {
			// Get a reference to the player health script.
			Killable playerHealth = other.GetComponent<Killable> ();

			// Increasse the player's health by the health bonus but clamp it at 100.
			playerHealth.currentHitPoints += healthBonus;
			playerHealth.currentHitPoints = Mathf.Clamp (playerHealth.currentHitPoints, 0, playerHealth.HitPoints);
			Destroy(gameObject);
		}
	}

}
