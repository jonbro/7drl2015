using UnityEngine;
using System.Collections;

public class GameRunner : MonoBehaviour {
	Panel currentPanel;
	public bool jumpToGame = true;
	// Use this for initialization
	void Start () {
		SetupGrid ();
		if (jumpToGame)
			SetupGame ();
		else
			SetupTitle ();
	}
	void SetupGrid(){
		int gridX = 20;
		int gridY = 15;
		GameObject gridHolder = new GameObject ("grid");
		Grid.SetCameraSize (gridX, gridY, 2, 2);
		// setup the grid with sprites for now (to be replaced by cale's screenspace thing at some point)
		for (int x = 0; x < gridX; x++) {
			for (int y = 0; y < gridY; y++) {
				GridSprite.Create (x, y, SpriteLibrary.FindSprite ("gridPoint"), Grid.Offset.UPPER_LEFT).gameObject.transform.SetParent(gridHolder.transform, true);
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
		if (currentPanel != null) {
			// destroy everything on the current panel and get the game displaying
			foreach (DisplayElement de in currentPanel.elements) {
				de.Destroy ();
			}
			Destroy (currentPanel.gameObject);
		}
		currentPanel = Panel.Create ();
		GameObject gameGO = (GameObject)Instantiate(Resources.Load ("Game") as GameObject, Vector3.zero, Quaternion.identity);
		gameGO.name = "game";
		gameGO.GetComponent<Level> ().Build (currentPanel);
	}
	void Update(){
		if (Input.GetKeyDown (KeyCode.Space)) {
			SetupGame ();
		}
	}
}
