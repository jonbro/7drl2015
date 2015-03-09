using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RLCharacter : DisplayElement
{
	SpriteRenderer actionPointDisplay, healthPointDisplay;

	// overlay gfx, could potentially abstract?
	int _actionPoints = 2;
	public int actionPoints{
		get { return _actionPoints; }
		set {
			// attach the action point display if required,
			_actionPoints = value;
			if (actionPointDisplay == null) {
				GameObject actionPointDisplayGO = new GameObject ("actionPoints");
				actionPointDisplay = actionPointDisplayGO.AddComponent<SpriteRenderer> ();
				actionPointDisplayGO.transform.SetParent (transform, false);
				actionPointDisplayGO.transform.localPosition = Vector3.zero;
			}
			actionPointDisplay.sprite = SpriteLibrary.FindSprite ("AP_" + _actionPoints);
		}
	}

	int _healthPoints = 3;
	public int healthPoints{
		get { return _healthPoints; }
		set {
			// attach the action point display if required,
			_healthPoints = value;
			if (healthPointDisplay == null) {
				GameObject healthPointDisplayGO = new GameObject ("healthPoints");
				healthPointDisplay = healthPointDisplayGO.AddComponent<SpriteRenderer> ();
				healthPointDisplayGO.transform.SetParent (transform, false);
				healthPointDisplayGO.transform.localPosition = Vector3.zero;
			}
			healthPointDisplay.sprite = SpriteLibrary.FindSprite ("HP_" + _healthPoints);
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
	public static RLCharacter Create(int x, int y, string ResourceName){
		GameObject characterGO = (GameObject)Instantiate (Resources.Load (ResourceName) as GameObject, Grid.GridToWorld (x, y), Quaternion.identity);
		RLCharacter character = characterGO.GetComponent<RLCharacter> ();
		character.position = new Vector2i (x, y);
		character.actionPoints = 2;
		character.healthPoints = 3;
		return character;
	}
	public Sprite GetSprite(){
		return GetComponent<SpriteRenderer> ().sprite;
	}
}

