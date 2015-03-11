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
		float vPosition = -0.65f;
		foreach (RLCharacter c in level.players) {
			VectorGui.SetPosition (new Vector2(10.65f, vPosition));
			vPosition -= 1;
			Color color = Color.white;
			if (c == level.currentPlayer)
				color = Color.green;
			VectorGui.Label("HP:"+c.healthPoints, 0.1f, color);
			VectorGui.Label("AP:"+c.actionPoints, 0.1f, color);

			// display the powers the player can use
			VectorGui.SetPosition (new Vector2(10.65f, vPosition));
			VectorGui.Label("1:OVRWTCH", 0.1f, color);
			vPosition -= 1;
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