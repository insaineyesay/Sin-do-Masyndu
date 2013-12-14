#pragma strict

var health : int; 			// reference to the health of the player
var player : GameObject; 	// reference to the player
var points : int; 			// reference to the players points for the current level
private var totalPoints: int;		// reference to the players points overall
private var lives : int;			// reference to the amount of lives the player has

function Awake () {
	// set the points for the level to zero
	points = 0;
	
	// set the players health to 100%
	health = 100;
	
	// how many points does the player have so far
	totalPoints = totalPoints += points;
	
	
}

function Update () {

}