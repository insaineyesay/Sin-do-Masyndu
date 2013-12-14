#pragma strict

var fMagnitude : float = 2;
var v3Axis : Vector3 = new Vector3(1,1,1);

function Start () {
	v3Axis.Normalize();

}

function Update () {
	transform.localPosition = v3Axis * Mathf.Sin(Time.time) * fMagnitude;
}