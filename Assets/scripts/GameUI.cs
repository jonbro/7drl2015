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
	public void Update(){
		foreach (RLCharacter c in level.players) {
			Color color = Color.white;
			if (c == level.currentPlayer)
				color = Color.green;
			VectorGui.Label("HP: "+c.healthPoints, 0.25f, color);
			VectorGui.Label("AP: "+c.actionPoints, 0.25f, color);
		}
	}
	public void AddToPanelAndTransform(DisplayElement de){
		panel.Add(de);
		de.transform.SetParent (transform);
	}
	void OnDestroy(){
		foreach (DisplayElement de in panel.elements) {
			if(de.enabled)
				de.Destroy ();
		}
	}
}