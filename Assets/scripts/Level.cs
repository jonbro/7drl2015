using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlayerInput
{
	UP,
	DOWN,
	LEFT,
	RIGHT,
	OVERWATCH,
	NONE
}

public class Level : MonoBehaviour {
	public enum RLObject {
		OPEN = 0,
		WALL,
		ENTRANCE,
		EXIT
	}
	public RL.Map map;
	public RL.CharacterMap monsterMap, playerMap;
	public List<RLCharacter> players;
	public int currentPlayerCounter = 0;
	public List<RLCharacter> monsters;
	FsmSystem fsm;
	GameUI ui;
	RL.Pathfinder pf = new RL.Pathfinder ();
	public System.Action OnGameOver;
	Panel gamePanel;
	RLCharacter currentPlayer;
	public void Build(Panel _gamePanel){
		gamePanel = _gamePanel;
		ui = ((GameObject)Instantiate((Resources.Load ("UI") as GameObject))).GetComponent<GameUI>();
		ui.Setup (this);
		fsm = new FsmSystem ();
		fsm.AddState (new FsmState (FsmStateId.InitialGen)
			.WithBeforeEnteringAction(GenLevel)
			.WithTransition (FsmTransitionId.Complete, FsmStateId.Player)
		);
		fsm.AddState (new FsmState (FsmStateId.Player)
			.WithBeforeEnteringAction(PlayerSetup)
			.WithUpdateAction(PlayerUpdate)
			.WithTransition (FsmTransitionId.Complete, FsmStateId.Monster)
			.WithTransition(FsmTransitionId.LevelComplete, FsmStateId.LevelGenWait)
		);
		fsm.AddState (new FsmState (FsmStateId.LevelGenWait)
			.WithBeforeEnteringAction(GenLevel)
			.WithTransition (FsmTransitionId.Complete, FsmStateId.Player)
		);
		fsm.AddState (new FsmState (FsmStateId.GameOver)
			.WithBeforeEnteringAction(GameOver)
		);
		fsm.AddState (new FsmState (FsmStateId.Monster)
			.WithBeforeEnteringAction(MonsterUpdate)
			.WithTransition (FsmTransitionId.Complete, FsmStateId.Player)
			.WithTransition (FsmTransitionId.GameOver, FsmStateId.GameOver)
		);
		fsm.Start ();
	}
	public void GenLevel(){
		// destroy everything on the existing panel, and build a new level
		foreach (DisplayElement de in gamePanel.elements) {
			de.Destroy ();
		}
		map = new RL.Map (10, 10);
		monsterMap = new RL.CharacterMap (10, 10);
		playerMap = new RL.CharacterMap (10, 10);
		monsters = new List<RLCharacter> ();
		players = new List<RLCharacter> ();
		// add entrance / exit
		map [0, Random.Range (1, 9)] = RL.Objects.ENTRANCE;
		map [9, Random.Range (1, 9)] = RL.Objects.EXIT;
		//		// put in some random walls
		for (int i = 0; i < 10; i++) {
			map [Random.Range (1, 9), Random.Range (1, 9)] = RL.Objects.WALL;
		}

		// add in a few player characters
		Vector2i pposition = GetPositionOfElement (RL.Objects.ENTRANCE);
		for (int i = 0; i < 3; i++) {
			pposition = FindOpenPositionAdjacent (pposition);
			RLCharacter player = RLCharacter.Create (pposition.x, pposition.y, "Player");
			player.gameObject.name = "player"+i;
			gamePanel.Add (player);
			players.Add (player);
			playerMap [pposition.x, pposition.y] = player;
			pposition = player.position;
		}
		currentPlayer = players[currentPlayerCounter];
		for (int i = 0; i < 3; i++) {
			Vector2i monsterPosition = FindOpenPosition ();
			RLCharacter monster = RLCharacter.Create (monsterPosition.x, monsterPosition.y, "Enemy");
			gamePanel.Add (monster);
			monsters.Add (monster);
		}
		// check to make sure we can walk from the beginning to the end of the level
		Vector2i exit = GetPositionOfElement (RL.Objects.EXIT);
		List<Vector2i> path = pf.FindPath (GetPositionOfElement (RL.Objects.ENTRANCE), exit, OneCostFunction, map); 
		if (HasPath (path, exit)) {
			UpdateMapDisplay ();
			fsm.PerformTransition (FsmTransitionId.Complete);
		} else {
			GenLevel ();
		}
	}
	Vector2i FindOpenPosition(){
		Vector2i position = new Vector2i (0, 0);
		while (map [position.x, position.y] != RL.Objects.OPEN) {
			position = new Vector2i(Random.Range (0, map.sx), Random.Range(0, map.sy));
		}
		return position;
	}
	Vector2i FindOpenPositionAdjacent(Vector2i center){
		for (int i = 0; i < 8; i++) {
			Vector2i testPosition = new Vector2i (center.x + RL.Map.nDir [i, 0], center.y + RL.Map.nDir [i, 1]);
			if (map.IsOpenTile (testPosition.x, testPosition.y)) {
				return testPosition;
			}
		}
		return new Vector2i (-1000, -1000);
	}
	int OneCostFunction(int x, int y){
		return 1;
	}
	bool HasPath(List<Vector2i> path, Vector2i endPoint){
		return path.Count > 0 && path [path.Count - 1].Equals (endPoint);
	}
	public void PlayerSetup(){
		foreach (RLCharacter p in players) {
			p.actionPoints = 2;
			p.overwatch = false;
		}
		currentPlayerCounter = 0;
		SetCurrentPlayer ();
	}
	void SetCurrentPlayer(){
		currentPlayer = players [currentPlayerCounter];
		foreach (RLCharacter p in players) {
			p.current = false;
		}
		currentPlayer.current = true;
	}
	public void PlayerUpdate (PlayerInput externalInput = PlayerInput.NONE)
	{
		bool performedAction = false;
		bool performedMove = false;
		PlayerInput nextInput = externalInput;
		// dealing with a bug in the tutorial of always sending the up key on the first run through the tutorial
		#if UNITY_STANDALONE
		nextInput = PlayerInput.NONE;
		#endif
		// get the keycode needed for moving things around the map
		if (Input.GetKeyDown (KeyCode.DownArrow)) {
			nextInput = PlayerInput.DOWN;
		}
		if (Input.GetKeyDown (KeyCode.UpArrow)) {
			nextInput = PlayerInput.UP;
		}
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			nextInput = PlayerInput.RIGHT;
		}
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			nextInput = PlayerInput.LEFT;
		}
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			nextInput = PlayerInput.OVERWATCH;
		}
		// convert input into delta positions for easier map movement
		Vector2i deltaD = new Vector2i (0, 0);
		if (nextInput == PlayerInput.DOWN) {
			deltaD = new Vector2i (0, 1);
		}
		if (nextInput == PlayerInput.UP) {
			deltaD = new Vector2i (0, -1);
		}
		if (nextInput == PlayerInput.RIGHT) {
			deltaD = new Vector2i (1, 0);
		}
		if (nextInput == PlayerInput.LEFT) {
			deltaD = new Vector2i (-1, 0);
		}
		if (nextInput == PlayerInput.OVERWATCH) {
			currentPlayer.overwatch = true;
			currentPlayer.actionPoints = 1;
			CompletePlayerActions ();
		}
		if (!deltaD.Equals (new Vector2i (0, 0))) {
			UpdateMonsterMap ();
			if (Attack(deltaD, currentPlayer, monsterMap) || PlayerMovement (deltaD)) {
				CompletePlayerActions ();
			}
		}
	}
	void CompletePlayerActions(){
		ui.UpdateDisplay ();
		UpdatePlayerMap ();
		currentPlayer.actionPoints--;
		if (currentPlayer.actionPoints == 0) {
			currentPlayerCounter = currentPlayerCounter + 1;
			if (currentPlayerCounter > players.Count - 1) {
				fsm.PerformTransition (FsmTransitionId.Complete);
			} else {
				SetCurrentPlayer ();
			}
		}
	}
	public void UpdatePlayerMap(){
		playerMap.Clear ();
		foreach (RLCharacter p in players) {
			playerMap [p.position.x, p.position.y] = p;
		}
	}
	public bool Attack(Vector2i delta, RLCharacter firingCharacter, RL.CharacterMap cMap){
		// check to see if one of the neighboring cells has a character in it, and attack if so
		Vector2i currentCell = firingCharacter.position + delta;

		while (map [currentCell.x, currentCell.y] == RL.Objects.OPEN) {

			RLCharacter enemy = cMap [currentCell.x, currentCell.y];
			if (enemy != null) {
				LeanTween.move (enemy.gameObject, new Vector3[] {
					enemy.transform.position + new Vector3 (Random.value, Random.value, 0f)*0.25f,
					enemy.transform.position + new Vector3 (Random.value, Random.value, 0f)*0.25f,
					enemy.transform.position + new Vector3 (Random.value, Random.value, 0f)*0.25f,
					enemy.transform.position + new Vector3 (Random.value, Random.value, 0f)*0.25f
				}, 0.1f).setOnComplete(() => {
					enemy.transform.position = Grid.GridToWorld(enemy.x, enemy.y);
				});
				enemy.healthPoints--;
				if (enemy.healthPoints == 0) {
					gamePanel.elements.Remove (enemy);
					if(monsters.Contains(enemy))
						monsters.Remove (enemy);
					else
						players.Remove (enemy);
					enemy.Destroy ();
				}
				return true;
			}
			currentCell += delta;
		}
		return false;
	}

	public bool PlayerMovement(Vector2i delta){
		Vector2i newPosition = currentPlayer.position + delta;
		if (playerMap [newPosition.x, newPosition.y] != null) {
			return false;
		}
		switch (map [newPosition.x, newPosition.y]) {
		case RL.Objects.OPEN:
			currentPlayer.position = newPosition;
			return true;
		case RL.Objects.EXIT:
			fsm.PerformTransition (FsmTransitionId.LevelComplete);
			return false;
		}
		return false;
	}
	void UpdateMonsterMap(){
		monsterMap.Clear ();
		foreach (RLCharacter m in monsters) {
			monsterMap [m.position.x, m.position.y] = m;
		}
	}
	bool MonsterAttack(RLCharacter m){
		bool attacked = false;
		for (int i = 0; i < 4; i++) {
			Vector2i AttackDirection = new Vector2i (RL.Map.nDir [i, 0], RL.Map.nDir [i, 1]);
			if (Attack (AttackDirection, m, playerMap)) {
				// if the attack happens on the first turn, should remove action points
				return true;
			}
		}
		return false;
	}
	bool MonsterMove(RLCharacter m){
		if (players.Count == 0)
			return true;
		List<Vector2i> path = pf.FindPath (m.position, players[0].position, OneCostFunction, map);
		for (int i = 1; i < players.Count; i++) {
			List<Vector2i> nextPath = pf.FindPath (m.position, players[i].position, (int x, int y)=>{
				if(monsterMap[x,y]!=null)
					return 1000;
				return 1;
			}, map);
			if(nextPath.Count < path.Count){
				path = nextPath;
			}
		}
		m.position = path [1];
		// check to see if we just stepped into a players overwatch
		return true;
	}
	public void CheckOverwatch(){
		UpdateMonsterMap ();
		foreach (RLCharacter p in players) {
			if (p.overwatch) {
				for (int i = 0; i < 4; i++) {
					Vector2i AttackDirection = new Vector2i (RL.Map.nDir [i, 0], RL.Map.nDir [i, 1]);
					if (Attack (AttackDirection, p, monsterMap)) {
						// if the attack happens on the first turn, should remove action points
						p.overwatch = false;
						break;
					}
				}
			}
		}
	}
	public void MonsterUpdate(){
		UpdatePlayerMap ();
		StartCoroutine (MonsterUpdateCoro ());
	}
	public IEnumerator MonsterUpdateCoro(){
		CheckOverwatch ();
		foreach (RLCharacter m in monsters) {
			while (m.actionPoints > 0) {
				// path towards the player or attack. Should probably add overwatch support at some point for asshole monsters
				if (MonsterAttack (m) || MonsterMove (m)) {
					CheckOverwatch ();
					m.actionPoints--;
					yield return new WaitForSeconds (0.1f);
					// should do a little delay here to visually process everything
				}
			}
		}
		foreach (RLCharacter m in monsters) {
			m.actionPoints = 2;
		}
		StartCoroutine (WaitAndCall(.25f, () => {
			if(players.Count == 0)
				fsm.PerformTransition (FsmTransitionId.GameOver);
			else
				fsm.PerformTransition (FsmTransitionId.Complete);
		}));
	}
	public void GameOver(){
		foreach (DisplayElement de in gamePanel.elements) {
			de.Destroy ();
		}
		Destroy (ui.gameObject);
		if (OnGameOver != null) {
			OnGameOver ();
		}
	}
	public IEnumerator WaitAndCall(float time, System.Action toCall){
		yield return new WaitForSeconds (time);
		toCall ();
	}
	public Vector2i GetPositionOfElement(RL.Objects element){
		for (int x = 0; x < 10; x++) {
			for (int y = 0; y < 10; y++) {
				if (map [x, y] == element) {
					return new Vector2i (x, y);
				}
			}
		}
		return new Vector2i(-1000, -1000);
	}
	public void UpdateMapDisplay(){
		for (int x = 0; x < 10; x++) {
			for (int y = 0; y < 10; y++) {
				switch (map [x, y]) {
				case RL.Objects.WALL:
					gamePanel.Add(GridSprite.Create (x, y, SpriteLibrary.FindSprite ("circ5_1")));
					break;
				case RL.Objects.ENTRANCE:
					gamePanel.Add(GridSprite.Create (x, y, SpriteLibrary.FindSprite ("dot2_0")));
					break;
				case RL.Objects.EXIT:
					gamePanel.Add(GridSprite.Create (x, y, SpriteLibrary.FindSprite ("dot0_0")));
					break;
				}
			}
		}
	}
	void Update(){
		fsm.CurrentState.Update ();
	}
}