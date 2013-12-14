using UnityEngine;
using System;
using System.Collections;

#if UNITY_WEBPLAYER
	// can't compile this class
#else
	using PlayerPrefs = PreviewLabs.PlayerPrefs;
#endif

[Serializable]
public class InGameWorldVariable {

    private const string PLAYER_PREF_STAT_TOKEN = "~KWStat_{0}~";
	
	public string statName;
	public WorldVariable sourceStat;

	private string _tokenizedPrefsKey = string.Empty;
	
	private string TokenizedPrefsKey {
		get {
			if (_tokenizedPrefsKey == string.Empty) {
				_tokenizedPrefsKey = GetTokenPrefsKey(statName);
			}
			
			return _tokenizedPrefsKey;
		}
	} 
	
	public int CurrentValue {
		get {
			if (!PlayerPrefs.HasKey(TokenizedPrefsKey)) {
				PlayerPrefs.SetInt(TokenizedPrefsKey, sourceStat.startingValue);
			}
			
			return PlayerPrefs.GetInt(TokenizedPrefsKey);
		}
		set {
			PlayerPrefs.SetInt(TokenizedPrefsKey, value);		
		}
	}
	
	#region static methods
	private static string GetTokenPrefsKey(string myStatName) {
		return string.Format(PLAYER_PREF_STAT_TOKEN, myStatName);
	}
	
	public static void FlushAll() {
		#if UNITY_WEBPLAYER
			// can't compile this class
		#else
			PlayerPrefs.Flush();
		#endif
	}
	
	public static int? GetCurrentWorldVariableValue(string variableName) {
		var tokenKey = GetTokenPrefsKey(variableName);
		
		if (!PlayerPrefs.HasKey(tokenKey)) {
			Debug.LogWarning("No World Variable named '" + variableName + "' was found in Killer Waves configuration.");
			return null;
		}
		
		return PlayerPrefs.GetInt(tokenKey);
	}
	
	public static void SetCurrentWorldVariableValue(string myStatName, int newValue) {
		var tokenKey = GetTokenPrefsKey(myStatName);

		PlayerPrefs.SetInt(tokenKey, newValue);
	}
	#endregion
}
