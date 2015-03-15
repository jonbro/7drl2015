using System;
using System.Collections.Generic;
using UnityEngine;

public class HoverDescription {
	public Rect targetRect;
	public string displayText;
}
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
	List<HoverDescription> hovers = new List<HoverDescription>();
	int currentHoverDesc = 0;
	void ResetHovers(){
		foreach (HoverDescription hd in hovers) {
			hd.targetRect = new Rect (0,0,0,0);
		}
		currentHoverDesc = -1;
	}
	HoverDescription GetNextHover(){
		currentHoverDesc++;
		if (hovers.Count <= currentHoverDesc) {
			hovers.Add (new HoverDescription ());
		}
		return hovers [currentHoverDesc];
	}
	public void Update(){
		float vPosition = 0.35f;

		VectorGui.SetPosition (new Vector2(-6.35f, 1.35f));
		if (level.fsm.CurrentStateID == FsmStateId.Monster) {
			VectorGui.Label ("Enemy Turn", 0.28f, GameColors.GetColor("enemy"));
		} else {
			VectorGui.Label ("AP:" + level.playerActionPoints, 0.28f, Color.white);
		}
		// clear out all the hover descriptions
		ResetHovers ();
		foreach (RLCharacter c in level.players) {
			VectorGui.SetPosition (new Vector2(10.65f, vPosition));
			vPosition -= 1;
			Color color = Color.white;
			if (c == level.currentPlayer)
				color = GameColors.GetColor("player");
			VectorGui.Label("HP:"+c.healthPoints, 0.1f, color);
//			VectorGui.Label("AP:"+c.actionPoints, 0.1f, color);

			VectorGui.SetPosition (new Vector2 (10.65f, vPosition));
			// display the powers the player can use
			for (int i = 0; i < c.powerups.Count; i++) {
				Vector3 hoverStart = VectorGui.Pen ().position;
				VectorGui.Label ((i+1)+":"+c.powerups[i].InventoryText(), 0.1f, color);
				Vector3 hoverEnd = VectorGui.Pen ().position;
				HoverDescription hover = GetNextHover ();
				hover.targetRect = new Rect(hoverStart.x, hoverStart.y-0.45f, 8, 0.45f);
				hover.displayText = c.powerups [i].DescriptionText () + " sale value " +c.powerups[i].saleValue;
			}
			vPosition -= 2;
		}
		// determine if there is a character underneath the cursor, and display the range highlights if so
		Vector3 mpWorld = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y));
		Vector2i mp = Grid.WorldToGrid (mpWorld);
		bool displayHighlights = false;
		bool foundItem = true;
		if (
			(!lastPosition.Equals (mp))
			&& mp.x >= 0 && mp.x < level.sx
			&& mp.y >= 0 && mp.y < level.sy) {
			foundItem = false;
			itemDescription.Clear ();
			lastPosition = mp;
			if (level.playerMap [mp.x, mp.y] != null) {
				level.playerMap [mp.x, mp.y].DisplayFireRadius (level.map, level.monsterMap, level.highlights);
				displayHighlights = true;
			} 
			if (level.monsterMap [mp.x, mp.y] != null) {
				level.monsterMap [mp.x, mp.y].DisplayFireRadius (level.map, level.playerMap, level.highlights);
				itemDescription.Add (level.monsterMap [mp.x, mp.y].info.description);
				displayHighlights = true;
				foundItem = true;
			}
			if (level.itemMap [mp.x, mp.y] != null) {
				itemDescription.Add (level.itemMap [mp.x, mp.y].powerUp.DescriptionText ());
				foundItem = true;
			}
			if (!displayHighlights) {
				level.HideHighlights ();
			}
		} else if(!lastPosition.Equals (mp)) {
			foundItem = false;
		}

		// check to see if we are in any of the hoverrects
		foreach (HoverDescription hd in hovers) {
			if (hd.targetRect.Contains (new Vector2 (mpWorld.x, mpWorld.y))) {
				itemDescription.Clear ();
				itemDescription.Add (hd.displayText);
				foundItem = true;
			}
		}
		if (!foundItem) {
			itemDescription.Clear ();
		}
		// display the console with the current state of the game
		// display instructions
		VectorGui.SetPosition (new Vector2(-6.35f, 0.35f));
		VectorGui.Label ("NEXT SPAWN: "+(level.apsPerSpawn-level.spawnTimer), 0.1f, Color.white);
		VectorGui.Label ("ARROW: Move-Atk", 0.1f, Color.white);
		VectorGui.Label ("TAB: Next Unit", 0.1f, Color.white);
		VectorGui.Label ("MOUSE: Get Info", 0.1f, Color.white);
		if (PlayerPrefs.GetFloat ("musicLevel") == 1) {
			VectorGui.Label ("A: Mute Audio", 0.1f, Color.white);
			if (Input.GetKeyDown (KeyCode.A)) {
				PlayerPrefs.SetFloat ("musicLevel", 0);
				PlayerPrefs.SetFloat ("sfxLevel", 0);
				AudioTriggerSystem.instance ().MuteAudio ();
			}
		} else {
			VectorGui.Label ("A: Unmute Audio", 0.1f, Color.white);
			if (Input.GetKeyDown (KeyCode.A)) {
				PlayerPrefs.SetFloat ("musicLevel", 1);
				PlayerPrefs.SetFloat ("sfxLevel", 1);
				AudioTriggerSystem.instance ().UnmuteAudio ();
			}
		}
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