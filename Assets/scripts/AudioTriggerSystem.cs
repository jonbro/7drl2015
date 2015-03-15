using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public static class MusicSystem{
	public static void Title(){
		AudioTriggerSystem.instance ().QueueClip ("bass");
		AudioTriggerSystem.instance ().PlayQueue (true);
	}
	public static void Contract(){
		AudioTriggerSystem.instance ().QueueClip ("bass");
		AudioTriggerSystem.instance ().QueueClip ("lowbass");
		AudioTriggerSystem.instance ().PlayQueue (true, 1);
	}
	public static void GameNextEnemy(){
		AudioTriggerSystem.instance ().QueueClip ("bass");
		if (Random.Range (0, 4) > 0) {
			switch (Random.Range (0, 4)) {
			case 0:
				AudioTriggerSystem.instance ().QueueClip("drumcutup1-drive_fixed");
				break;
			case 1:
				AudioTriggerSystem.instance ().QueueClip("drumcutup2");
				break;
			case 2:
				AudioTriggerSystem.instance ().QueueClip("drumcutup3");
				break;
			default:
				AudioTriggerSystem.instance ().QueueClip("drums");
				break;
			}
		}
		if (Random.Range (0, 6) >= 2) {
			AudioTriggerSystem.instance ().QueueClip("jazz");
		}
		if (Random.Range (0, 3) == 0) {
			AudioTriggerSystem.instance ().QueueClip("tapdelay");
		}
		if (Random.Range (0, 3) == 0) {
			AudioTriggerSystem.instance ().QueueClip("haunt_partial");
		}
		AudioTriggerSystem.instance ().PlayQueue (true, 1);
	}
	public static void GameOver(){
		AudioTriggerSystem.instance ().QueueClip("halfdrums");
		AudioTriggerSystem.instance ().QueueClip ("halfglitter_good");
		AudioTriggerSystem.instance ().PlayQueue (true, 1);
	}

	public static void Shop(){
		AudioTriggerSystem.instance ().QueueClip ("bass");
		AudioTriggerSystem.instance ().QueueClip ("tapdelay");
		AudioTriggerSystem.instance ().PlayQueue (true, 1);
	}
}
public class AudioLoop {
	public AudioClip 	clip;
	public float	 	fadeInTime, fadeOutTime;
	public bool 		playing;
	public AudioLoop(AudioClip c, float fadeTime){
		clip = c;
		fadeInTime = fadeOutTime = fadeTime;
	}
	public IEnumerator fader;
}
public class AudioClipBank {
	public List<AudioClip> clips = new List<AudioClip>();
	public AudioClip GetRandom(){
		return clips [Random.Range (0, clips.Count)];
	}
}

public class AudioTriggerSystem : MonoBehaviour {
	static AudioTriggerSystem _instance;
	public static AudioTriggerSystem instance(){
		if (_instance == null) {
			GameObject instanceGO = new GameObject ("AudioSystem");
			DontDestroyOnLoad (instanceGO);
			instanceGO.transform.position = Camera.main.transform.position;
			instanceGO.transform.parent = Camera.main.transform;
			instanceGO.AddComponent<AudioSource> ();
			_instance = instanceGO.AddComponent<AudioTriggerSystem> ();

			_instance.Init ();
		}
		return _instance;
	}
	Dictionary<string, AudioClipBank> clips = new Dictionary<string, AudioClipBank>();
	Dictionary<string, AudioLoop> loops = new Dictionary<string, AudioLoop>();

	List<string> queuedClips = new List<string> ();
	Dictionary<string, AudioSource> playingLoops = new Dictionary<string, AudioSource> ();
	List<AudioSource> loopPlayers = new List<AudioSource> ();

	AudioSource main;
	GameObject loopPlayer;
	// Use this for initialization 
	void Awake(){
		if (!PlayerPrefs.HasKey ("sfxLevel"))
			PlayerPrefs.SetFloat ("sfxLevel", 1f);
		if (!PlayerPrefs.HasKey ("musicLevel"))
			PlayerPrefs.SetFloat ("musicLevel", 1);

	}
	void Init () {
		loopPlayer = Resources.Load<GameObject> ("loopPlayer");
		main = Camera.main.audio;
		// load in the clips we are going to use later
		LoadLoop ("bass", 0.5f);
		LoadLoop ("lowbass", 0.5f);
		LoadLoop ("tapdelay", 0.5f);
		LoadLoop ("halfglitter_good", 0.5f);
		LoadLoop ("haunt_partial", 0.5f);

		LoadLoop("drumcutup1-drive_fixed", 0.5f);
		LoadLoop("drumcutup2", 0.5f);
		LoadLoop("drumcutup3", 0.5f);
		LoadLoop("drums", 0.5f);
		LoadLoop("halfdrums", 0.5f);
		LoadLoop("jazz", 0.5f);
	
		LoadClip("1moveleft");
		LoadClip("2moveleft");
		LoadClip("3moveleft");
		LoadClip("4moveleft");
		LoadClip("enemyattack");
		LoadClip("enemymove");
		LoadClip("getitem");
		LoadClip("sellitem");
		LoadClip("playerdied");
		LoadClip("enemyspawn");
		LoadClip("use_powerup");

		LoadClip("all_clear");
		LoadClip("enemy_defeated");
		LoadClip("player_attack");
	}
	void LoadClip(string clipName){
		AudioClip c = Resources.Load<AudioClip> ("sounds/" + clipName);
		if (c != null) {
			clips [clipName] = new AudioClipBank();
			clips [clipName].clips.Add (c);
		}
	}
	void LoadClipToBank(string bankName, string clipName){
		AudioClip c = Resources.Load<AudioClip> ("sounds/" + clipName);
		if (c != null) {
			if(!clips.ContainsKey(bankName))
				clips [bankName] = new AudioClipBank();
			clips [bankName].clips.Add (c);
		}

	}
	void LoadLoop(string loopName, float fadeTime = 0.25f){
		AudioClip c = Resources.Load<AudioClip> ("sounds/" + loopName);
		if (c != null) {
			loops[loopName] = new AudioLoop(c, fadeTime);
		}
	}
	public void QueueClip(string name){
		if (!queuedClips.Contains (name))
			queuedClips.Add (name);
	}
	public void PlayClipImmediate(string s){
		if (clips.ContainsKey (s)) {
			transform.position = Camera.main.transform.position;
			audio.PlayOneShot (clips [s].GetRandom(), PlayerPrefs.GetFloat ("sfxLevel")*0.6f);
		}
	}
	public void MuteAudio(){
		foreach (string k in playingLoops.Keys) {
			playingLoops [k].volume = 0;
		}
	}
	public void UnmuteAudio(){
		foreach (string k in playingLoops.Keys) {
			playingLoops [k].volume = 1;
		}
	}
	public void PlayQueue(bool fadeLoops = false, float fadeSpeed = 1.0f){
		foreach (string s in queuedClips) {
			if (fadeLoops && loops.ContainsKey (s)) {
				// if the clip isn't already faded in, then get a new audio object to contain it and fade it in
				if (!playingLoops.ContainsKey (s)) {
					AudioSource thisPlayer = GetLoopPlayer ();
					thisPlayer.clip = loops [s].clip;
					// this is where you would fade it in
					thisPlayer.Play ();
					thisPlayer.gameObject.name = s;
					if (loops[s].fader != null) {
						StopCoroutine (loops[s].fader);
					}
					thisPlayer.volume = 0;
					loops [s].fader = FadeIn (thisPlayer, PlayerPrefs.GetFloat ("musicLevel"), fadeSpeed);
					StartCoroutine (loops [s].fader);
					playingLoops [s] = thisPlayer;
				}
			} else if (clips.ContainsKey (s)) {
				transform.position = Camera.main.transform.position;
				audio.PlayOneShot (clips [s].GetRandom(), PlayerPrefs.GetFloat ("sfxLevel"));
			}
		}
		if (fadeLoops) {
			List<string> toRemove = new List<string> ();
			foreach (string k in playingLoops.Keys) {
				if (!queuedClips.Contains (k)) {
					// this is where you would fade it out
					if (loops [k].fader != null) {
						StopCoroutine (loops [k].fader);
					}
					loops[k].fader = FadeAndRemove (playingLoops [k], fadeSpeed);
					StartCoroutine (loops[k].fader);
					toRemove.Add (k);
				}
			}
			foreach (string k in toRemove) {
				playingLoops.Remove (k);
			}
		}

		queuedClips.Clear ();
	}
	AudioSource GetLoopPlayer(){
		foreach (AudioSource g in loopPlayers) {
			if (!g.isPlaying)
				return g;
		}
		GameObject source = (GameObject)Instantiate (loopPlayer, Camera.main.transform.position, Quaternion.identity);
		source.transform.parent = transform;
		source.transform.position = Camera.main.transform.position;
		loopPlayers.Add (source.audio);
		return source.audio;
	}
	IEnumerator FadeIn(AudioSource s, float target, float time){
		if (time == 0)
			time = 0.01f;
		float startTime = Time.time;
		while (s.volume < target) {
			s.volume = Mathf.Lerp (0, target, (Time.time - startTime) / time);
			yield return new WaitForEndOfFrame ();
		}
		s.volume = target;
	}
	IEnumerator FadeAndRemove(AudioSource s, float time){
		if (time == 0)
			time = 0.01f;
		float startTime = Time.time;
		while (s.volume > 0) {
			s.volume = Mathf.Lerp (1, 0, (Time.time - startTime) / time);
			yield return new WaitForEndOfFrame ();
		}
		s.Stop ();
	}
	// Update is called once per frame
	void Update () {

	}
}
