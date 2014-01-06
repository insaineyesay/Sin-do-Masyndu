using UnityEngine;
using System.Collections;

public class WeaponPowerUp : MonoBehaviour
{
	public int IncreasedDamage;					// How much health the crate gives the player.
	public AudioClip collectSound;				// The sound of the crate being collected.
	public int MAX_ATTACK_POINTS;				// Setting the max the attack points can reach
	public float upgradeTime;					// Time limit for weapon upgrade to last
	public Transform WeaponPosition;			// Weapon object to set active when upgrade is obtained
	public Rigidbody2D Weapon;					// Refernce to the Weapon that will be added to Player
	private GameObject Player; 					// reference to the player
	public static bool hasUpgrade;					// Check to see if the player has upgrade already


	void Awake ()
	{	
			
			Debug.Log (hasUpgrade + "Awake function call");
			Player = GameObject.Find ("Player");
	}

	void OnTriggerEnter2D (Collider2D other)
	{

			// If the player enters the trigger zone...
			if (other.tag == "WeaponPowerUp1") {
					
					// and if the players has upgrade status is set to false
//					if (!hasUpgrade) {
							// Start the coroutine
							StartCoroutine ("PlayerUpgraded", other);
							// otherwise, ignore the game object and destroy it
//					} 
//			else if (hasUpgrade) {
//							CollectionIgnored ();
//					}
				
			}
	}

	IEnumerator PlayerUpgraded (Collider2D other)
	{
			
			// if player hasUpgrade status false, set it to true
		hasUpgrade = true;
			Debug.Log ("Player just collided with upgrade. Subroutine started");
			// Destroy the power up object

			
				Debug.Log (hasUpgrade + " You picked up the upgrade");

				// Get a reference to the player shot script for damage increase.
				Killable shotDamage = this.GetComponent<Killable> ();
		
				// Increase the player's health by the health bonus but clamp it at 20.
				shotDamage.AttackPoints += IncreasedDamage;
		Debug.Log ("attack points are now " + IncreasedDamage);

				// Make sure the players attack points do not exceed 20. Otherwise it will keep increasing
				shotDamage.AttackPoints = Mathf.Clamp (shotDamage.AttackPoints, 0, MAX_ATTACK_POINTS); 
				

				// Instatiate a clone of the weapon upgrade
				Weapon = Instantiate (Weapon, WeaponPosition.position, WeaponPosition.rotation) as Rigidbody2D;
				
		Debug.Log("weapon instantiated");

				// Attach the instance of the weapon upgrade to the Player so it moves with the player
				Weapon.transform.parent = transform;

				Debug.Log ("Player upgraded" + Time.time);
				Debug.Log ("waiting for " + upgradeTime + " seconds" + Time.time);
				
				// Pause the playerUpgraded script for the amount of time in the upgrade
				yield return new WaitForSeconds (upgradeTime);

				Debug.Log ("ready to remove upgrade" + Time.time);
				
				GameObject [] cannons = GameObject.FindGameObjectsWithTag ("TwinCannons1");
				foreach (GameObject cannon in cannons) {
//						Destroy (cannon);
						cannon.SetActive (false);
				}

				// Set has upgrade to false and delete the upgrade 
				hasUpgrade = false;
				Debug.Log (hasUpgrade + "hasUpgrade status after upgrade removal");
		}

//		void CollectionIgnored ()
//		{
//			
//				Physics.IgnoreCollision (gameObject.collider, Player.collider, true);
//				Debug.Log ("player has upgrade already " + Time.time);
//				
//		}

}
