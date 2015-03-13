using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RLCharacterInfo {
	public int maxHealth = 3;
	public int maxActionPoints = 2;
	public string svgName = "circ0_0";
	public int fireRadius = 3;
	public bool endTurnOnAttack = false;
	public string description = "Player";
	public Color coreColor;
	public RLCharacterInfo(){
		coreColor = GameColors.GetColor ("player");
	}
	public static RLCharacterInfo baseMonster = new RLCharacterInfo {
		description = "SMPL: RANGE 3 : END ON ATK",
		svgName = "enemy",
		coreColor = GameColors.GetColor("enemy")
	};
	public static RLCharacterInfo monsterMultifire = new RLCharacterInfo {
		description = "ASLT: RANGE 2 : MULTIFIRE",
		svgName = "enemy_range2",
		fireRadius = 2,
		coreColor = GameColors.GetColor("enemy")
	};
	public static RLCharacterInfo GetRandomMonster(){
		switch (UnityEngine.Random.Range (0, 2)) {
		case 0:
			return baseMonster;
		default:
			return monsterMultifire;
		}
	}
}
public class RLPlayerCharacterData{
	public List<PowerUp> powerups = new List<PowerUp>();
}
public class RLCharacter : DisplayElement
{
	public RLPlayerCharacterData characterData;
	Dictionary<string, bool> states = new Dictionary<string, bool>();
	SpriteRenderer actionPointDisplay, overwatchDisplay;
	SvgRenderer healthPointDisplay, stateDisplay;
	public bool canUsePowerup;
	public RLCharacterInfo info;
	public int x {
		get{ return position.x; }
	}
	public int y {
		get { return position.y; }
	}
	public List<PowerUp> powerups{
		get{ 
			return characterData.powerups;
		}
		set{
			characterData.powerups = value;
		}
	}
	// overlay gfx, could potentially abstract?
	int _maxActionPoints = 2;
	public int maxActionPoints {
		get{ 
			int tempMax = _maxActionPoints;
			foreach (PowerUp p in powerups) {
				tempMax += p.actionPointModifier;
			}
			return tempMax;
		}
		set{
			_maxActionPoints = value;
		}
	}
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
			if (value < _healthPoints) {
				Compression.PopBlur (transform, 0.9f, 1.25f, .8f);
			}
			_healthPoints = Mathf.Min(3, value);
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
	public static RLCharacter Create(int x, int y, string ResourceName, RLCharacterInfo _info){
		GameObject characterGO = (GameObject)Instantiate (Resources.Load ("Player") as GameObject, Grid.GridToWorld (x, y), Quaternion.identity);
		RLCharacter character = characterGO.GetComponent<RLCharacter> ();
		characterGO.GetComponent<SvgRenderer> ().LoadSvgFromResources (_info.svgName);
		character.info = _info;
		character.GetComponent<SvgRenderer>().colorProperty = character.originalColor = _info.coreColor;
		character.position = new Vector2i (x, y);
		character.actionPoints = _info.maxActionPoints;
		character._maxActionPoints = _info.maxActionPoints;
		character.healthPoints = _info.maxHealth;
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
			while (map.IsValidTile (currentCell.x, currentCell.y) && map [currentCell.x, currentCell.y] == RL.Objects.OPEN && count < info.fireRadius) {
				count++;
				RLCharacter enemy = enemyMap [currentCell.x, currentCell.y];
				highlights [currentCell.x, currentCell.y].color = Color.red;
				currentCell += delta;
			}
		}
	}
	public bool CustomMovement(Vector2i delta, RL.Map map, RL.CharacterMap<RLCharacter> monsterMap, RL.CharacterMap<RLCharacter> playerMap){
		if (GetState ("fastMove")) {
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
			canUsePowerup = true;
			Compression.PopBlur (transform, 1f, 2.5f);
			return true;
		}
		return false;
	}
}

