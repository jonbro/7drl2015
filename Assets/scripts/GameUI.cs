using System;
using System.Collections.Generic;
using UnityEngine;

// displays all the current stuff for the players
public class GameUI : MonoBehaviour
{
	Level level;
	Panel panel;
	public void Setup(Level _level){
		panel = Panel.Create();
		level = _level;
	}
	public void UpdateDisplay(){
		foreach (DisplayElement de in panel.elements) {
			de.Destroy ();
		}
		// display the current hp / ap / spells / resources for each character
		int count = 0;
		foreach (RLCharacter c in level.players) {
			// show the sprite for the character so that we can match it
			AddToPanelAndTransform (GridSprite.Create (12, count, c.GetSprite ()));
			AddToPanelAndTransform (GridText.Create (13, count, "HP: "+c.healthPoints));
			AddToPanelAndTransform (GridText.Create (15, count, "AP: "+c.actionPoints));
			count++;
		}
	}
	public void AddToPanelAndTransform(DisplayElement de){
		panel.Add(de);
		de.transform.SetParent (transform);
	}
}