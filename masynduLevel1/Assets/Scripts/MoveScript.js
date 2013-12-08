#pragma strict

var speed : Vector2 = new Vector2(10,10);
var direction : Vector2 = new Vector2(-1,0);

function Update () {
	var movement : Vector3 = new Vector3(
		speed.x * direction.x,
		speed.y * direction.y,
		0);
		
		movement *= Time.deltaTime;
		transform.Translate(movement);
}