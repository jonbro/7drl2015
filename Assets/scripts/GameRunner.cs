using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// contains information about the current game
public class GameInfo{
	public List<RLPlayerCharacterData> crew = new List<RLPlayerCharacterData>();
	public int totalDays;
	public int daysRemaining;
	public int totalCredits;
	public int creditsPaid;
	public GameInfo(){
		daysRemaining = totalDays = Random.Range (7, 15);
		totalCredits = Random.Range (70, 130);
	}
}

public class GameRunner : MonoBehaviour {
	Panel currentPanel;
	public bool jumpToGame = true;
	public GameObject grid;
	enum ScreenName {
		GAME,
		CONTRACT,
		TITLE,
		SHOP,
		GAMEWIN
	}
	bool runningGame = false;
	bool titleScreen = true;
	GameInfo gameInfo;
	ScreenName currentScreen;
	// Use this for initialization
	void Awake () {
		SetupGrid ();
	}
	void Start(){
		SetupGameInfo ();
		AddRandomScrap ();
		SetupShop ();
	}
	void SetupGrid(){
		int gridX = 20;
		int gridY = 12;
		GameObject gridHolder = new GameObject ("grid");
		Grid.SetCameraSize (gridX, gridY, 2, 2);
		//TODO make this the correct size
		grid.transform.localScale = new Vector3(gridX*2,gridY*2,1);
		
	}
	void SetupGameInfo(){
		gameInfo = new GameInfo ();
		SetupCrew ();
	}
	void SetupCrew(){
		for (int i = 0; i < 3; i++) {
			gameInfo.crew.Add (new RLPlayerCharacterData ());
		}
	}
	void AddRandomScrap(){
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 2; j++) {
				gameInfo.crew [i].powerups.Add (PowerUp.GetPowerup ());
			}
		}
	}
	void SetupTitle(){
		SetupGameInfo ();
		currentPanel = Panel.Create ();
		// place title at grid position
		runningGame = false;
		currentScreen = ScreenName.TITLE;
	}
	void SetupContract(){
		if (currentPanel != null) {
			// destroy everything on the current panel and get the game displaying
			foreach (DisplayElement de in currentPanel.elements) {
				de.Destroy ();
			}
			Destroy (currentPanel.gameObject);
		}
		currentPanel = Panel.Create ();
		GameObject contractPicker = new GameObject ("ContractPicker");
		ContractPicker cp = contractPicker.AddComponent<ContractPicker> ();
		cp.Init (gameInfo);
		cp.StartGame = SetupGame;
		currentScreen = ScreenName.CONTRACT;
	}
	void SetupGame(ContractInfo contract){
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
		gameGO.GetComponent<Level> ().contract = contract;
		gameGO.GetComponent<Level> ().Build (currentPanel, gameInfo);
		gameGO.GetComponent<Level> ().OnGameOver = OnGameOver;
		gameGO.GetComponent<Level> ().OnContractComplete = SetupShop;
		currentScreen = ScreenName.GAME;
	}
	void OnGameOver(){
		SetupTitle ();
	}
	void SetupShop(){
		if (currentPanel != null) {
			// destroy everything on the current panel and get the game displaying
			foreach (DisplayElement de in currentPanel.elements) {
				de.Destroy ();
			}
			Destroy (currentPanel.gameObject);
		}
		currentPanel = Panel.Create ();
		GameObject contractPicker = new GameObject ("Shop");
		Shop shop = contractPicker.AddComponent<Shop> ();
		shop.Init (gameInfo);
		shop.PickContract = SetupContract;
		currentScreen = ScreenName.SHOP;
	}
	void Update(){
		if (currentScreen == ScreenName.TITLE) {
			VectorGui.SetPosition (new Vector2(-6.35f, 0.35f));
			VectorGui.Label ("SALVAGE CREW", 0.6f, Color.white);
			VectorGui.Label ("", 0.35f, Color.white);
			VectorGui.Label ("PRESS SPACE TO PAY YOUR DEBTS", 0.2f, Color.white);
			VectorGui.Label ("TOP CREDITS: "+PlayerPrefs.GetInt("topScore"), 0.1f, Color.white);

			if (Input.GetKeyDown (KeyCode.Space)) {
				SetupContract ();
			}
		}
	}
}
