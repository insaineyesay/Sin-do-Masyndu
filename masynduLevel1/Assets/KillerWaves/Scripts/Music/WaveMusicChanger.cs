using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Dark Tonic/Killer Waves/Music/Wave Music Changer")]
[RequireComponent(typeof(AudioSource))]
public class WaveMusicChanger : MonoBehaviour {
	public WaveMusicChangerListener listener;

	private static WaveMusicChangerListener statListener;
	private static AudioSource statAudio;
	private static bool isValid;
	private static bool isFading;
	private static float fadeStartTime;
	private static float fadeStartVolume; 
	private static float fadeTotalTime;
	private static float lastVolume;
	
	void Awake() {
		statAudio = this.audio;
		statListener = this.listener;
		isFading = false;
		 
		if (statAudio != null) {
			isValid = true;
		}
	}

	void Start() {
		if (isValid) {
			StartCoroutine(this.CoStart());
		}
	}
	 
	IEnumerator CoStart() {
    	while (true) {
        	yield return StartCoroutine(this.CoUpdate());
		} 
	}
	
	IEnumerator CoUpdate() {
		yield return new WaitForSeconds(.1f); // fading interval
		
		if (!isFading || !statAudio.isPlaying) {
            yield break; // nothing to do.
		}
		  
		statAudio.volume = fadeStartVolume * (fadeTotalTime - (Time.time - fadeStartTime)) / fadeTotalTime;
		
		var volDelta = lastVolume - statAudio.volume;
		
		if (statAudio.volume <= volDelta) {
			isFading = false;
			statAudio.Stop();
		}
		
		lastVolume = statAudio.volume;
	}
	
	public static void WaveUp(LevelWaveMusicSettings newWave) {
		PlayMusic(newWave);
	}
	
	private static void PlayMusic(LevelWaveMusicSettings musicSettings) {
		if (!isValid) {
			Debug.LogError("WaveMusicChanger is not attached to any prefab with an AudioSource component. Music in Killer Waves LevelSettings will not play.");
			return;
		}
		
		if (statListener != null) {
			statListener.MusicChanging(musicSettings);
		}
		
		isFading = false;
		
		switch (musicSettings.WaveMusicMode) {
			case LevelSettings.WaveMusicMode.PlayNew:
				statAudio.Stop();
				statAudio.clip = musicSettings.WaveMusic;
				statAudio.volume = musicSettings.WaveMusicVolume;
				statAudio.Play();
				break;
			case LevelSettings.WaveMusicMode.Silence:
				isFading = true;
				fadeStartTime = Time.time;
				fadeStartVolume = statAudio.volume;
				fadeTotalTime = musicSettings.FadeTime;
				break;
			case LevelSettings.WaveMusicMode.KeepPreviousMusic:
				statAudio.volume = musicSettings.WaveMusicVolume;
				break;
		}
	}
	
	public static void PlayGameOverMusic(LevelWaveMusicSettings musicSettings) {
		PlayMusic(musicSettings);
	}
}
