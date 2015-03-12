using UnityEngine;
using System.Collections;

public class GameRunner : MonoBehaviour {
	Panel currentPanel;
	public bool jumpToGame = true;
	public GameObject grid;
	// Use this for initialization
	void Awake () {
		SetupGrid ();
	}
	void Start(){
		if (jumpToGame)
			SetupGame ();
		else
			SetupTitle ();
	}
	void SetupGrid(){
		int gridX = 20;
		int gridY = 12;
		GameObject gridHolder = new GameObject ("grid");
		Grid.SetCameraSize (gridX, gridY, 2, 2);
		//TODO make this the correct size
		grid.transform.localScale = new Vector3(gridX*2,gridY*2,1);
	
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
		gameGO.GetComponent<Level> ().OnGameOver = OnGameOver;
	}
	void OnGameOver(){
		SetupTitle ();
	}
	void Update(){
		if (Input.GetKeyDown (KeyCode.Space)) {
			SetupGame ();
		}
	}
}
