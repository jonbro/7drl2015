using UnityEngine;
using System.Collections;

public class GameRunner : MonoBehaviour {
	Panel currentPanel;
	// Use this for initialization
	void Start () {
		SetupGrid ();
		SetupTitle ();
	}
	void SetupGrid(){
		int gridX = 20;
		int gridY = 15;
		Grid.SetCameraSize (gridX, gridY, 2, 2);
		// setup the grid with sprites for now (to be replaced by cale's screenspace thing at some point)
		for (int x = 0; x < gridX; x++) {
			for (int y = 0; y < gridY; y++) {
				GridSprite.Create (x, y, SpriteLibrary.FindSprite ("gridPoint"), Grid.Offset.UPPER_LEFT);
			}
		}
	}
	void SetupTitle(){
		currentPanel = Panel.Create ();
		// place title at grid position
		currentPanel.Add(GridText.Create (0, 0, "SALVAGE CREW"));
		currentPanel.Add(GridText.Create (0, 1, "TOP SCORE " + PlayerPrefs.GetInt("topScore")));
		currentPanel.Add(GridText.Create (0, 3, "Run Op"));
	}
	void SetupGame(){
		// destroy everything on the current panel and get the game displaying
		foreach (DisplayElement de in currentPanel.elements) {
			de.Destroy ();
		}
		Destroy(currentPanel.gameObject);
		currentPanel = Panel.Create ();
	}
	void Update(){
		if (Input.GetKeyDown (KeyCode.Space)) {
			SetupGame ();
		}
	}
}
