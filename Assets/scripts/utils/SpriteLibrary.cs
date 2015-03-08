using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SpriteLibrary : MonoBehaviour {
	static SpriteLibrary _instance;
	public List<Sprite> sprites;
	public static SpriteLibrary instance{
		get{
			if (_instance == null) {
				GameObject go = Resources.Load ("SpriteLibrary") as GameObject;
				_instance = go.GetComponent<SpriteLibrary> ();
			}
			return _instance;
		}
	}
	Sprite _FindSprite(string name){
		foreach (Sprite s in sprites) {
			if (name == s.name) {
				return s;
			}
		}
		return null;
	}
	public static Sprite FindSprite(string name){
		return instance._FindSprite (name);
	}
}
