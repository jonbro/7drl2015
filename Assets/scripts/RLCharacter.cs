using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RLCharacter : DisplayElement
{
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
		return character;
	}
}

