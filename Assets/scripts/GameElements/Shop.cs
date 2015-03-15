using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shop : MonoBehaviour {
	public class CharacterPowerup {
		public RLPlayerCharacterData character;
		public PowerUp powerup;
		public GridSVG svg;
	}
	public System.Action PickContract;
	public System.Action EndGame;
	string[] introStrings;
	GameInfo gameInfo;
	public bool setup;
	List<CharacterPowerup> characterPowers = new List<CharacterPowerup>();
	Panel panel;
	public Vector2 offset;
	public void Init (GameInfo _info) {
		panel = Panel.Create ();
		gameInfo = _info;
		setup = true;
		RebuildInfoStrings ();
		// for each of the powerups that was being carried by the crew, add a display
		int playerCount = 0;
		foreach (RLPlayerCharacterData cd in gameInfo.crew) {
			int count = 0;
			foreach (PowerUp p in cd.powerups) {
				GridSVG powerSvg = GridSVG.CreateFromSvg (-5+playerCount%2*5, 2+count, p.SvgIcon());
				characterPowers.Add (new CharacterPowerup{character=cd, powerup=p, svg = powerSvg});
				panel.Add (powerSvg);
				count++;
			}
			playerCount++;
		}
		FixPowerPositions ();
	}
	void RebuildInfoStrings(){
		string introductionText = "A successful misson. But was it enough?\n" +
			"you now have {0} days to pay back the {1} credits you owe,\nbefore you are repossesed.";
		introductionText = System.String.Format (introductionText, gameInfo.daysRemaining, gameInfo.totalCredits-gameInfo.creditsEarned);
		introStrings = introductionText.Split ('\n');
	}
	void FixPowerPositions(){
		int count = 0;
		int playerCount = 0;
		if (characterPowers.Count > 0) {
			RLPlayerCharacterData c = characterPowers [0].character;
			foreach (CharacterPowerup cp in characterPowers) {
				if (c != cp.character) {
					c = cp.character;
					playerCount++;
					count = 0;
				}
				Debug.Log (count);
				cp.svg.transform.position = Grid.GridToWorld (-5 + playerCount * 6, 2 + count);
				count++;
			}
		}
	}
	// Update is called once per frame
	void Update () {
		if(setup){
			DisplayUpdate ();
		}
	}
	void DisplayUpdate(){
		if (introStrings != null) {
			VectorGui.SetPosition (new Vector2(-5.35f, 0.35f));
			foreach (string s in introStrings) {
				VectorGui.Label (s, 0.1f, Color.white);
			}
		}
		Vector2i gp = Grid.WorldToGrid (Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y)));
		int selectedItem = -1;
		int itemX = ((gp.x + 5) / 6);
		int itemY = ((gp.y - 2));
		if (itemX >= 0 && itemX < 2) {
			selectedItem = itemX+itemY;
		}
		CharacterPowerup toSell = new CharacterPowerup();
		bool sold = false;
		if (characterPowers.Count > 0) {
			RLPlayerCharacterData c = characterPowers [0].character;
			int playerCount = 0;	
			int count = 0;
			foreach (CharacterPowerup cp in characterPowers) {
				if (c != cp.character) {
					c = cp.character;
					playerCount++;
					count = 0;
				}
				Color color = Color.white;
				if (playerCount == itemX && count == itemY) {
					color = GameColors.GetColor ("player");
					if (Input.GetMouseButtonDown (0)) {
						toSell = cp;
						sold = true;
						AudioTriggerSystem.instance ().PlayClipImmediate ("sellitem");
					}
				}
				VectorGui.SetPosition (new Vector2 (-4.35f + playerCount * 6, -1.65f - count));
				VectorGui.Label (cp.powerup.InventoryText (), 0.1f, color);
				count++;
			}
			if (sold) {
				gameInfo.creditsEarned += toSell.powerup.saleValue;
				RebuildInfoStrings ();
				toSell.svg.Destroy ();
				toSell.character.powerups.Remove (toSell.powerup);
				characterPowers.Remove (toSell);
				FixPowerPositions ();
			}
		}
		VectorGui.SetPosition (new Vector2(-5.35f, -7.65f));
		VectorGui.Label ("Click to sell items, unsold items 2x value", 0.1f, Color.white);
		VectorGui.SetPosition (new Vector2 (-5.35f, -8.65f));
		if (gameInfo.totalCredits - gameInfo.creditsEarned > 0) {
			VectorGui.Label ("Press space to take new contract", 0.1f, Color.white);
			if (Input.GetKeyDown (KeyCode.Space)) {
				foreach (RLPlayerCharacterData cd in gameInfo.crew) {
					foreach (PowerUp p in cd.powerups) {
						p.saleValue *= 2;
					}
				}
				PickContract ();
				Destroy (gameObject);
			}			
		} else {
			VectorGui.Label ("Press space to end game", 0.1f, Color.white);
			if (Input.GetKeyDown (KeyCode.Space)) {
				EndGame ();
				Destroy (gameObject);
			}			
		}
	}
	void OnDestroy(){
		if (panel != null) {
			// destroy everything on the current panel and get the game displaying
			foreach (DisplayElement de in panel.elements) {
				de.Destroy ();
			}
			Destroy (panel.gameObject);
		}
	}
}
