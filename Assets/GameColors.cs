using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class NamedColor {
	public string name;
	public Color color;
}

public class GameColors : MonoBehaviour {
	public List<NamedColor> gameColors;
	Dictionary<string, Color> colorDict;
	static GameColors _instance;
	public static GameColors instance{
		get{ 
			if (_instance == null) {
				GameObject GameColorsGo = (GameObject)Instantiate(Resources.Load ("GameColors"));
				GameObject.DontDestroyOnLoad (_instance);
				_instance = GameColorsGo.GetComponent<GameColors> ();
				_instance.SetupColors ();
			}
			return _instance;
		}
	}
	public static Color GetColor(string colorName){
		if (!instance.colorDict.ContainsKey (colorName))
			return Color.cyan;
		return instance.colorDict [colorName];
	}
	void SetupColors(){
		colorDict = new Dictionary<string, Color> ();
		foreach (NamedColor nc in gameColors) {
			colorDict.Add (nc.name, nc.color);
		}
	}
}
