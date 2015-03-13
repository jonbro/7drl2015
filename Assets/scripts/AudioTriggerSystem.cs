using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
			PlayerPrefs.SetFloat ("sfxLevel", 1);
		if (!PlayerPrefs.HasKey ("musicLevel"))
			PlayerPrefs.SetFloat ("musicLevel", 1);

	}
	void Init () {
		loopPlayer = Resources.Load<GameObject> ("loopPlayer");
		main = Camera.main.audio;
		// load in the clips we are going to use later
		LoadLoop ("exc2");

	}
	void LoadClip(string clipName){
		AudioClip c = Resources.Load<AudioClip> ("Audio/" + clipName);
		if (c != null) {
			clips [clipName] = new AudioClipBank();
			clips [clipName].clips.Add (c);
		}
	}
	void LoadClipToBank(string bankName, string clipName){
		AudioClip c = Resources.Load<AudioClip> ("Audio/" + clipName);
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
			audio.PlayOneShot (clips [s].GetRandom(), PlayerPrefs.GetFloat ("sfxLevel"));
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
					thisPlayer.volume = PlayerPrefs.GetFloat ("musicLevel");
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

		foreach (string k in playingLoops.Keys) {
			// this is where you would fade it out
			playingLoops [k].volume = PlayerPrefs.GetFloat ("musicLevel");
		}

	}
}
