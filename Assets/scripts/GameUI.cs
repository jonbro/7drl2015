﻿using System;
using System.Collections.Generic;
using UnityEngine;

// displays all the current stuff for the players
public class GameUI : MonoBehaviour
{
	Level level;
	Panel panel;
	List<string> itemDescription = new List<string>();
	public void Setup(Level _level){
		panel = Panel.Create();
		level = _level;
	}
	Vector2i lastPosition;
	public void Update(){
		float vPosition = 0.35f;
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
			vPosition -= 2;
		}
		// determine if there is a character underneath the cursor, and display the range highlights if so
		Vector2i mp = Grid.WorldToGrid (Camera.main.ScreenToWorldPoint (new Vector3(Input.mousePosition.x, Input.mousePosition.y)));
		bool displayHighlights = false;
		if (
			(!lastPosition.Equals(mp))
			&& mp.x >= 0 && mp.x < level.sx
			&& mp.y >= 0 && mp.y < level.sy
		){
			itemDescription.Clear ();
			lastPosition = mp;
			if (level.playerMap [mp.x, mp.y] != null) {
				level.playerMap [mp.x, mp.y].DisplayFireRadius (level.map, level.monsterMap, level.highlights);
				displayHighlights = true;
			} 
			if(level.monsterMap [mp.x, mp.y] != null) {
				level.monsterMap [mp.x, mp.y].DisplayFireRadius (level.map, level.playerMap, level.highlights);
				itemDescription.Add (level.monsterMap [mp.x, mp.y].info.description);
				displayHighlights = true;
			}
			if(level.itemMap [mp.x, mp.y] != null) {
				itemDescription.Add (level.itemMap [mp.x, mp.y].powerUp.DescriptionText());
			}
			if(!displayHighlights){
				level.HideHighlights ();
			}
		}
		// display the console with the current state of the game
		// display instructions
		VectorGui.SetPosition (new Vector2(-6.35f, 0.35f));
		VectorGui.Label ("ARROW: Move-Atk", 0.1f, Color.white);
		VectorGui.Label ("TAB: Next Unit", 0.1f, Color.white);
		VectorGui.Label ("MOUSE: Get Info", 0.1f, Color.white);
		VectorGui.Label ("E: End Player Turn", 0.1f, Color.white);

		VectorGui.SetPosition (new Vector2(-.35f, -7.65f));
		VectorGui.Label (System.String.Format("Credits: {0} (of {1}) Level: {2} / {3}", level.gameInfo.creditsEarned, level.gameInfo.totalCredits, level.currentLevel, level.contract.rooms), 0.1f, Color.white);
		foreach (String description in itemDescription) {
			VectorGui.Label (description, 0.1f, Color.white);
		}
		if (level.monsters.Count == 0) {
			VectorGui.Label ("-1 Credit Per Player Turn", 0.1f, Color.white);
			VectorGui.Label ("Press Space to go to next level", 0.1f, Color.white);
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