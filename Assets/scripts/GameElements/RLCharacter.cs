using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
public class RLCharacter : DisplayElement
{
	Dictionary<string, bool> states = new Dictionary<string, bool>();
	public List<PowerUp> powerups = new List<PowerUp>();
	SpriteRenderer actionPointDisplay, overwatchDisplay;
	SvgRenderer healthPointDisplay, stateDisplay;
	public bool canUsePowerup;
	public int x {
		get{ return position.x; }
	}
	public int y {
		get { return position.y; }
	}
	// overlay gfx, could potentially abstract?
	int _actionPoints = 2;
	public int actionPoints{
		get { return _actionPoints; }
		set {
			// attach the action point display if required,
			_actionPoints = value;
//			if (actionPointDisplay == null) {
//				GameObject actionPointDisplayGO = new GameObject ("actionPoints");
//				actionPointDisplay = actionPointDisplayGO.AddComponent<SpriteRenderer> ();
//				actionPointDisplayGO.transform.SetParent (transform, false);
//				actionPointDisplayGO.transform.localPosition = Vector3.zero;
//			}
//			actionPointDisplay.sprite = SpriteLibrary.FindSprite ("AP_" + _actionPoints);
		}
	}
	bool _current = false;
	public bool current{
		get{ return _current; }
		set{ 
			_current = value;
			if (!_current) {
				GetComponent<SvgRenderer>().colorProperty = originalColor;
			}
		}
	}
	public Color color{
		set { 
			originalColor = value;
			GetComponent<SvgRenderer>().colorProperty = originalColor;
		}
	}
	public int fireRange = 3;
	bool _overwatch = false;
	public bool overwatch{
		get { return _overwatch; }
		set {
			// attach the action point display if required,
			_overwatch = value;
			if (overwatchDisplay == null) {
				GameObject overwatchDisplayGO = new GameObject ("overwatch");
				overwatchDisplay = overwatchDisplayGO.AddComponent<SpriteRenderer> ();
				overwatchDisplayGO.transform.SetParent (transform, false);
				overwatchDisplayGO.transform.localPosition = Vector3.zero;
			}
			if(_overwatch)
				overwatchDisplay.sprite = SpriteLibrary.FindSprite ("overwatch");
			else
				overwatchDisplay.sprite = null;
		}
	}
	int _healthPoints = 3;
	public int healthPoints{
		get { return _healthPoints; }
		set {
			// attach the action point display if required,
			_healthPoints = value;
			if (healthPointDisplay == null) {
				healthPointDisplay = GridSVG.CreateFromSvg (x, y, "HP_3").GetComponent<SvgRenderer>();
				GameObject healthPointDisplayGO = healthPointDisplay.gameObject;
				healthPointDisplayGO.transform.SetParent (transform, false);
				healthPointDisplayGO.transform.localPosition = Vector3.zero;
			}
			healthPointDisplay.LoadSvgFromResources("HP_" + _healthPoints);
			healthPointDisplay.colorProperty = originalColor;
		}
	}


	Vector2i _position;
	public Vector2i position{
		get{ return _position; }
		set{
			_position = value;
			LeanTween.cancel (gameObject);
			Compression.PopBlur (transform, 0.9f, 0.25f, 0);
			LeanTween.move (gameObject, Grid.GridToWorld (_position.x, _position.y), 0.125f);
		}
	}
	string currentDisplayState = "";
	public void SetState(string stateName, bool value){
		if (value) {
			if (stateDisplay == null) {
				stateDisplay = GridSVG.CreateFromSvg (x, y, stateName + "State").GetComponent<SvgRenderer> ();
				GameObject stateDisplayGO = stateDisplay.gameObject;
				stateDisplayGO.transform.SetParent (transform, false);
				stateDisplayGO.transform.localPosition = Vector3.zero;
			}
			currentDisplayState = stateName;
			stateDisplay.LoadSvgFromResources (stateName + "State");
			stateDisplay.colorProperty = originalColor;
		} else {
			if (currentDisplayState == stateName) {
				currentDisplayState = "";
				stateDisplay.colorProperty = Color.clear;
			}
		}
		states [stateName] = value;
	}
	public bool GetState(string stateName){
		if (!states.ContainsKey (stateName))
			return false;
		else
			return states [stateName];
	}
	Color originalColor;
	SpriteRenderer spriteR;
	public static RLCharacter Create(int x, int y, string ResourceName){
		GameObject characterGO = (GameObject)Instantiate (Resources.Load (ResourceName) as GameObject, Grid.GridToWorld (x, y), Quaternion.identity);
		RLCharacter character = characterGO.GetComponent<RLCharacter> ();
		character.originalColor = character.GetComponent<SvgRenderer>().colorProperty;
		character.position = new Vector2i (x, y);
		character.actionPoints = 2;
		character.healthPoints = 3;
		character.spriteR = character.GetComponent<SpriteRenderer> ();
		return character;
	}
	public Sprite GetSprite(){
		if (!spriteR)
			return null;
		return spriteR.sprite;
	}
	void Update(){
		if (current) {
			GetComponent<SvgRenderer>().colorProperty = Color.Lerp (Color.clear, originalColor, ((Mathf.Sin (Time.time*5f) + 1)*0.5f) * 0.5f + 0.5f);
		}
	}
	public void DisplayFireRadius(RL.Map map, RL.CharacterMap<RLCharacter> enemyMap, RL.CharacterMap<RLHighlight> highlights){
		for (int x = 0; x < highlights.sizeX; x++) {
			for (int y = 0; y < highlights.sizeY; y++) {
				highlights [x, y].color = Color.clear;
			}
		}
		// check to see if one of the neighboring cells has a character in it, and attack if so
		for (int i = 0; i < 4; i++) {
			Vector2i delta = new Vector2i (RL.Map.nDir [i, 0], RL.Map.nDir [i, 1]);
			Vector2i currentCell = position + delta;
			int count = 0;
			while (map.IsValidTile (currentCell.x, currentCell.y) && map [currentCell.x, currentCell.y] == RL.Objects.OPEN && count < fireRange) {
				count++;
				RLCharacter enemy = enemyMap [currentCell.x, currentCell.y];
				highlights [currentCell.x, currentCell.y].color = Color.red;
				currentCell += delta;
			}
		}
	}
	public bool CustomMovement(Vector2i delta, RL.Map map, RL.CharacterMap<RLCharacter> monsterMap, RL.CharacterMap<RLCharacter> playerMap){
		Debug.Log ("checking custom movement");
		if (GetState ("fastMove")) {
			Debug.Log ("checking fast move");
			// attempt to move to the end of the line
			Vector2i lastPosition = position;
			Vector2i np = position+delta;
			while (
				map.IsOpenTile (np.x, np.y)
				&& (playerMap[np.x, np.y] == null ||playerMap[np.x, np.y] == this)
				&& monsterMap[np.x, np.y] == null
			) {
				lastPosition = np;
				np += delta;
			}
			position = lastPosition;
			SetState ("fastMove", false);

			Compression.PopBlur (transform, 1f, 2.5f);
			return true;
		}
		return false;
	}
}

