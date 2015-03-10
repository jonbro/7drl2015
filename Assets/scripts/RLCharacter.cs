using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RLCharacter : DisplayElement
{
	SpriteRenderer actionPointDisplay, healthPointDisplay, overwatchDisplay;
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
			if (!_current && spriteR) {
				spriteR.color = originalColor;
			}
		}
	}

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
//			if (healthPointDisplay == null) {
//				GameObject healthPointDisplayGO = new GameObject ("healthPoints");
//				healthPointDisplay = healthPointDisplayGO.AddComponent<SpriteRenderer> ();
//				healthPointDisplayGO.transform.SetParent (transform, false);
//				healthPointDisplayGO.transform.localPosition = Vector3.zero;
//			}
//			healthPointDisplay.sprite = SpriteLibrary.FindSprite ("HP_" + _healthPoints);
		}
	}


	Vector2i _position;
	public Vector2i position{
		get{ return _position; }
		set{
			_position = value;
			transform.position = Grid.GridToWorld (_position.x, _position.y);
		}
	}
	Color originalColor;
	SpriteRenderer spriteR;
	public static RLCharacter Create(int x, int y, string ResourceName){
		GameObject characterGO = (GameObject)Instantiate (Resources.Load (ResourceName) as GameObject, Grid.GridToWorld (x, y), Quaternion.identity);
		RLCharacter character = characterGO.GetComponent<RLCharacter> ();
		character.position = new Vector2i (x, y);
		character.actionPoints = 2;
		character.healthPoints = 3;
		character.spriteR = character.GetComponent<SpriteRenderer> ();
		if(character.spriteR)
			character.originalColor = character.spriteR.color;
		return character;
	}
	public Sprite GetSprite(){
		if (!spriteR)
			return null;
		return spriteR.sprite;
	}
	void Update(){
		if (current && spriteR) {
			spriteR.color = Color.Lerp (Color.clear, originalColor, ((Mathf.Sin (Time.time*5f) + 1)*0.5f) * 0.5f + 0.5f);
		}
	}
}

