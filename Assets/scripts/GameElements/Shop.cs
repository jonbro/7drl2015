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
		int count = 0;
		foreach (RLPlayerCharacterData cd in gameInfo.crew) {
			foreach (PowerUp p in cd.powerups) {
				GridSVG powerSvg = GridSVG.CreateFromSvg (-5+count%2*8, 2+count/2, p.SvgIcon());
				characterPowers.Add (new CharacterPowerup{character=cd, powerup=p, svg = powerSvg});
				panel.Add (powerSvg);
				count++;
			}
		}
	}
	void RebuildInfoStrings(){
		string introductionText = "A successful misson. But was it enough?\n" +
			"you now have {0} days to pay back the {1} credits you owe,\nbefore you are repossesed.";
		introductionText = System.String.Format (introductionText, gameInfo.daysRemaining, gameInfo.totalCredits-gameInfo.creditsEarned);
		introStrings = introductionText.Split ('\n');
	}
	void FixPowerPositions(){
		int count = 0;
		foreach (CharacterPowerup cp in characterPowers) {
			cp.svg.transform.position = Grid.GridToWorld (-5+count%2*8, 2+count/2);
			count++;
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
		int itemX = ((gp.x + 5) / 8);
		int itemY = ((gp.y - 2))*2;
		if (itemX >= 0 && itemX < 2) {
			selectedItem = itemX+itemY;
		}
		CharacterPowerup toSell = new CharacterPowerup();
		bool sold = false;
		int count = 0;
		foreach (CharacterPowerup cp in characterPowers) {
			Color c = Color.white;
			if (count == selectedItem) {
				c = GameColors.GetColor ("player");
				if (Input.GetMouseButtonDown (0)) {
					toSell = cp;
					sold = true;
				}
			}
			VectorGui.SetPosition (new Vector2(-4.35f+count%2*8, -1.65f-count/2));
			VectorGui.Label ("scrap value: "+cp.powerup.saleValue, 0.1f, c);
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
