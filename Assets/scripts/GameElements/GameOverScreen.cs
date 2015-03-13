using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameOverScreen : MonoBehaviour {
	public System.Action ExitScreen;
	string[] introStrings;
	GameInfo gameInfo;
	public bool setup;
	Panel panel;
	public Vector2 offset;
	string bigText = "";
	public void Init (GameInfo _info) {
		panel = Panel.Create ();
		gameInfo = _info;
		setup = true;
		RebuildInfoStrings ();
		// for each of the powerups that was being carried by the crew, add a display
	}
	void RebuildInfoStrings(){
		string descriptionText = "";
		if (gameInfo.creditsEarned >= gameInfo.totalCredits) {
			if (gameInfo.daysRemaining < 0) {
				// earned the correct number of credits, but ran out of days
				descriptionText = "You paid your debts, but were {0} days late.\n" +
					"A few body parts got taken from you as interest.";
				descriptionText = System.String.Format (descriptionText, -gameInfo.daysRemaining);
				bigText = "PARTIAL WIN";
			} else {
				// earned the correct number of credits in the alloted time
				descriptionText = "There was a party on the {0} that night.\n" +
				"They paid off their bodies and had {1} left over for food.\n";
				descriptionText = System.String.Format (descriptionText, gameInfo.shipName, gameInfo.creditsEarned - gameInfo.totalCredits);
				bigText = "SALVAGE SUCCESSFUL";
			}
		} else {
			descriptionText = "Not everyone is cut out for salvage work.\n" +
				"At least you earned {0} Credits, but without a body\n" +
				"what does it matter.";
			descriptionText = System.String.Format (descriptionText, gameInfo.creditsEarned);
			bigText = "NO BODIES, NO WIN";
		}
		introStrings = descriptionText.Split ('\n');
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
		VectorGui.SetPosition (new Vector2 (-5.35f, -6.65f));
		VectorGui.Label (bigText, 0.3f, Color.white);
	
		VectorGui.SetPosition (new Vector2 (-5.35f, -8.65f));
		VectorGui.Label ("Press space to return to title", 0.1f, Color.white);
		if (Input.GetKeyDown (KeyCode.Space)) {
			ExitScreen ();
			Destroy (gameObject);
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
