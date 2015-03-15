using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// contains information about the current game
public class GameInfo{
	public List<RLPlayerCharacterData> crew = new List<RLPlayerCharacterData>();
	public int totalDays;
	public int daysRemaining;
	public int totalCredits;
	public int creditsEarned;
	public string shipName;
	public GameInfo(){
		daysRemaining = totalDays = 3; //Random.Range (7, 15);
		totalCredits = 90; //Random.Range (70, 130);
		creditsEarned = 0;
		shipName = NameGen.GetShipName ();
	}
}

public class GameRunner : MonoBehaviour {
	Camera cam;
	public Transform blurTarget;
	Panel currentPanel;
	public bool jumpToGame = true;
	public GameObject grid;
	enum ScreenName {
		GAME,
		CONTRACT,
		TITLE,
		SHOP,
		GAMEOVER
	}
	bool runningGame = false;
	bool titleScreen = true;
	GameInfo gameInfo;
	ScreenName currentScreen;
	// Use this for initialization
	void Awake () {
		PlayerPrefs.SetFloat ("musicLevel", 1.0f);
		PlayerPrefs.GetFloat ("sfxLevel", 1.0f);
		cam = FindObjectOfType<Camera>();
		SetupGrid ();
	}
	void Start(){
		SetupGameInfo ();
//		AddRandomScrap ();
//		SetupShop ();
		if (jumpToGame)
			OnGameOver ();
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
		ClearCurrentPanel ();
		SetupGameInfo ();
		currentPanel = Panel.Create ();
		// place title at grid position
		runningGame = false;
		currentScreen = ScreenName.TITLE;
		MusicSystem.Title ();
	}
	void ClearCurrentPanel(){
		if (currentPanel != null) {
			// destroy everything on the current panel and get the game displaying
			foreach (DisplayElement de in currentPanel.elements) {
				de.Destroy ();
			}
			Destroy (currentPanel.gameObject);
		}
	}
	void SetupContract(){
		// check to see if we hit our credit limit
		// or we ran out of days
		if (gameInfo.creditsEarned >= gameInfo.totalCredits || gameInfo.daysRemaining <= 0) {
			OnGameOver ();
		} else {
			ClearCurrentPanel ();
			currentPanel = Panel.Create ();
			GameObject contractPicker = new GameObject ("ContractPicker");
			ContractPicker cp = contractPicker.AddComponent<ContractPicker> ();
			cp.Init (gameInfo);
			cp.StartGame = SetupGame;
			currentScreen = ScreenName.CONTRACT;
			MusicSystem.Contract ();
		}
	}
	void SetupGame(ContractInfo contract){
		ClearCurrentPanel ();
		gameInfo.daysRemaining -= contract.days;
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
		ClearCurrentPanel ();
		currentPanel = Panel.Create ();
		GameObject gameOver = new GameObject ("GameOver");
		GameOverScreen gameoverScreen = gameOver.AddComponent<GameOverScreen> ();
		gameoverScreen.Init (gameInfo);
		gameoverScreen.ExitScreen = SetupTitle;
		currentScreen = ScreenName.GAMEOVER;
		MusicSystem.GameOver ();
	}
	void SetupShop(){
		ClearCurrentPanel ();
		currentPanel = Panel.Create ();
		GameObject contractPicker = new GameObject ("Shop");
		Shop shop = contractPicker.AddComponent<Shop> ();
		shop.Init (gameInfo);
		shop.PickContract = SetupContract;
		shop.EndGame = OnGameOver;
		currentScreen = ScreenName.SHOP;
		MusicSystem.Shop ();
	}
	void Update(){
		if (currentScreen == ScreenName.TITLE) {
			VectorGui.SetPosition (new Vector2(-6.35f, 0.35f));
			VectorGui.Label ("SALVAGE CREW", 0.6f, Color.white);
			VectorGui.Label ("", 0.35f, Color.white);
			VectorGui.Label ("PRESS SPACE TO PAY YOUR DEBTS", 0.2f, Color.white);
			if (PlayerPrefs.HasKey ("topScore")) {
				VectorGui.Label ("TOP CREDITS: "+PlayerPrefs.GetInt("topScore"), 0.1f, Color.white);
			}
			if (Input.GetKeyDown (KeyCode.Space)) {
				SetupContract ();
			}
			VectorGui.Label ("", 0.4f, Color.white);
			VectorGui.Label ("BY: ", 0.1f, Color.white);
			VectorGui.Label ("Jonathan Brodsky (at:jonbro)", 0.1f, Color.white);
			VectorGui.Label ("Additional Graphics: Cale Bradbury (at:netgrind)", 0.1f, Color.white);
			VectorGui.Label ("Music by: Sal Farina & Austin Redwood", 0.1f, Color.white);
		}
		if(currentScreen!=ScreenName.GAME){
			blurTarget.transform.position = Vector3.Lerp(cam.ScreenToWorldPoint(Input.mousePosition),blurTarget.transform.position,.5f);
		}
	}
}
