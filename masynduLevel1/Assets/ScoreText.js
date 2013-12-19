#pragma strict

var scoreText : GUIText;

private var score : int;

function Start () {
	// set the Gui text for the score to Count: #
	scoreText.text = "Count:  " + score.ToString();
}

function Update () {
	
}