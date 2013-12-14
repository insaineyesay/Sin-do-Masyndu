using UnityEngine;
using System.Collections;


[AddComponentMenu("Dark Tonic/Killer Waves/Despawners/Particle Despawner")]
[RequireComponent(typeof(ParticleSystem))]
public class ParticleDespawner : MonoBehaviour {
	private ParticleSystem particles;
	private Transform trans;
	
	// Update is called once per frame
	void Awake() {
		this.trans = this.transform;
		this.particles = this.GetComponent<ParticleSystem>();
	}
	
	void Update () {
		if (!this.particles.IsAlive()) {
			SpawnUtility.Despawn(this.trans);
		}
	}
}
