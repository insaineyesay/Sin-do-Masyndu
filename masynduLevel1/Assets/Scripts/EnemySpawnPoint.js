#pragma strict

var spawnTime : float = 5; 			// The amount of time between each spawn
var spawnDelay : float = 3;			// The amount of time before spawning starts
var enemies : GameObject[];			// The array of enemy prefabs
private var hasAppeared : boolean; 	// check to see if the object has appeared on screen

function Start () {
	// Start calling the spawn function repeatedly after a delay
	InvokeRepeating("Spawn", spawnDelay, spawnTime);	
}

function Spawn () {
	// Instantiate a random enemy.
	var enemyIndex : int = Random.Range(0, enemies.length);
	Instantiate(enemies[enemyIndex], transform.position, transform.rotation);
	
}