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
				color = GameColors.GetColor("player");
			VectorGui.Label("HP:"+c.healthPoints, 0.1f, color);
			VectorGui.Label("AP:"+c.actionPoints, 0.1f, color);

			VectorGui.SetPosition (new Vector2 (10.65f, vPosition));
			// display the powers the player can use
			for (int i = 0; i < c.powerups.Count; i++) {
				VectorGui.Label ((i+1)+":"+c.powerups[i].DisplayText(), 0.1f, color);
			}
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