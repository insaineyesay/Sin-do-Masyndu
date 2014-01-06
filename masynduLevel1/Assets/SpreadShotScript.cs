using UnityEngine;
using System.Collections;

public class SpreadShotScript : MonoBehaviour {

	private Vector2 shotpath;
	void Bezier2 (Vector2 Start, Vector2 Control, Vector2 End, float t)
	{
		shotpath = (((1-t)*(1-t)) * Start) + (2 * t * (1 - t) * Control) + ((t * t) * End);

	}
}
