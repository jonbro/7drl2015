using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlayerInput
{
	UP,
	DOWN,
	LEFT,
	RIGHT,
	POWER1,
	POWER2,
	END_TURN,
	NEXT_CHARACTER,
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
	public RL.CharacterMap<RLCharacter> monsterMap, playerMap;
	public RL.CharacterMap<RLItem> itemMap;
	public RL.CharacterMap<RLHighlight> highlights;
	public List<RLCharacter> players, exitPlayers;
	public List<RLItem> items;
	public int currentPlayerCounter = 0;
	public List<RLCharacter> monsters;
	public FsmSystem fsm;
	GameUI ui;
	RL.Pathfinder pf = new RL.Pathfinder ();
	public System.Action OnGameOver, OnContractComplete;
	Panel gamePanel, highlightPanel;
	public RLCharacter currentPlayer;
	public int currentLevel = 0;
	public int sx = 8;
	public int sy = 8;
	public int score = 5;
	public ContractInfo contract;
	public GameInfo gameInfo;
	public int playerActionPoints;
	public int basePlayerActionPoints = 4;
	public int spawnTimer = 0;
	public int apsPerSpawn = 13;
	public void Build(Panel _gamePanel, GameInfo _gameInfo){
		gameInfo = _gameInfo;
		gamePanel = _gamePanel;
		highlightPanel = Panel.Create();
		ui = ((GameObject)Instantiate((Resources.Load ("UI") as GameObject))).GetComponent<GameUI>();
		ui.Setup (this);
		fsm = new FsmSystem ();
		fsm.AddState (new FsmState (FsmStateId.InitialGen)
			.WithBeforeEnteringAction(InitialGen)
			.WithTransition (FsmTransitionId.Complete, FsmStateId.LevelGenWait)
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
	public void InitialGen(){
		while (gameInfo.crew.Count < 3) {
			gameInfo.crew.Add (new RLPlayerCharacterData ());
		}
		for (int i = 0; i < gameInfo.crew.Count; i++) {
			RLCharacter player = RLCharacter.Create (0, 0, "Player", new RLCharacterInfo());
			player.characterData = gameInfo.crew [i];
			player.maxActionPoints = 4;
			exitPlayers.Add (player);
			player.color = GameColors.GetColor ("player");
			player.gameObject.name = "player"+i;
		}
		highlights = new RL.CharacterMap<RLHighlight>(sx, sy);
		// setup the highlights
		for (int x = 0; x < sx; x++) {
			for (int y = 0; y < sy; y++) {
				RLHighlight highlight = RLHighlight.CreateFromSvg (x,y, "highlight");
				highlight.color = Color.clear;
				highlightPanel.Add (highlight);
				highlights [x, y] = highlight;
			}
		}
		currentLevel = 0;
		fsm.PerformTransition (FsmTransitionId.Complete);
	}
	public void GenLevel(){
		StartCoroutine (GenLevelSlow ());
	}
	public IEnumerator GenLevelSlow(){
		while (!GenLevelInternal ()) {
			UpdateMapDisplay ();
			yield return new WaitForSeconds (0.25f);
		}
		currentLevel++;
		exitPlayers.Clear ();
		UpdateMapDisplay ();
		fsm.PerformTransition (FsmTransitionId.Complete);
	}
	public bool GenLevelInternal(){
		spawnTimer = 0;
		// incase this runs a few times, need to copy the players into the exit players so it doesn't fail
		// destroy everything on the existing panel, and build a new level
		foreach (DisplayElement de in gamePanel.elements) {
			if (typeof(RLCharacter)!=de.GetType() || !exitPlayers.Contains ((RLCharacter)de) && !players.Contains ((RLCharacter)de)) {
				de.Destroy ();
			}
		}
		map = new RL.Map (sx, sy);
		monsterMap = new RL.CharacterMap<RLCharacter> (sx, sy);
		playerMap = new RL.CharacterMap<RLCharacter> (sx, sy);
		itemMap = new RL.CharacterMap<RLItem> (sx, sy);
		items = new List<RLItem> ();
		monsterMap.Clear ();
		playerMap.Clear ();
		itemMap.Clear ();

		monsters = new List<RLCharacter> ();
		players = new List<RLCharacter> ();
		// add entrance / exit
		map [0, Random.Range (1, sy-1)] = RL.Objects.ENTRANCE;
		//		// put in some random walls
		for (int i = 0; i < 4; i++) {
			map [Random.Range (1, sx-1), Random.Range (1, sy-1)] = RL.Objects.WALL;
		}

		// add in a few player characters
		Vector2i pposition = GetPositionOfElement (RL.Objects.ENTRANCE);

		foreach (RLCharacter player in exitPlayers) {
			pposition = FindOpenPositionAdjacent (pposition);
			if (!map.IsValidTile (pposition.x, pposition.y)) {
				return false;
			}
			player.Show ();
			player.position = pposition;
			gamePanel.Add (player);
			players.Add (player);
			playerMap [pposition.x, pposition.y] = player;
			pposition = player.position;
			UpdatePlayerMap ();
		}
		currentPlayerCounter = 0;
		currentPlayer = players[currentPlayerCounter];
		SelectNextPlayer ();
		for (int i = 0; i < (currentLevel+1); i++) {
			AddMonster ();
		}
		// add items to the map
		for (int i = 0; i < 2; i++) {
			AddItem (PowerUp.GetPowerup ());
		}
		// guarantee at least one score token
		AddItem (new PUScoreUp ());

		// check to make sure every monster is accessible by every player
		foreach (RLCharacter p in players) {
			Vector2i exit = GetPositionOfElement (RL.Objects.EXIT);
			foreach (RLCharacter m in monsters) {
				List<Vector2i> path = pf.FindPath (p.position, m.position, OneCostFunction, map); 
				if (!HasPath (path, p.position, m.position))
					return false;
			}
		}
		MusicSystem.GameNextEnemy ();
		return true;
	}
	void AddMonster(){
		Vector2i monsterPosition = FindOpenPosition ();
		// confirm that this position can access the players
		bool hasPath = false;
		while (hasPath == false) {	
			bool allHavePath = true;
			foreach (RLCharacter p in players) {
				List<Vector2i> path = pf.FindPath (p.position, monsterPosition, OneCostFunction, map); 
				if (!HasPath (path, p.position, monsterPosition)) {
					allHavePath = false;
					monsterPosition = FindOpenPosition ();
					break;
				}
			}
			if(allHavePath)
				hasPath = true;
		}
		RLCharacter monster = RLCharacter.Create (monsterPosition.x, monsterPosition.y, "Enemy", RLCharacterInfo.GetRandomMonster());
		monster.color = GameColors.GetColor ("enemy");
		gamePanel.Add (monster);
		monsters.Add (monster);
		UpdateMaps ();
	}
	void AddItem(PowerUp itemToAdd){
		Vector2i itemPosition = FindOpenPosition ();
		RLItem item = RLItem.CreateFromSvg (itemPosition.x, itemPosition.y, itemToAdd.SvgIcon());
		item.powerUp = itemToAdd;
		item.color = GameColors.GetColor ("powerup");
		gamePanel.Add (item);
		items.Add (item);
		UpdateMaps ();
	}
	Vector2i FindOpenPosition(){
		Vector2i position = new Vector2i (0, 0);
		while (
			map [position.x, position.y] != RL.Objects.OPEN
			|| playerMap[position.x, position.y] != null
			|| monsterMap[position.x, position.y] != null
			|| itemMap[position.x, position.y] != null
		) {
			position = new Vector2i(Random.Range (0, map.sx), Random.Range(0, map.sy));
		}
		return position;
	}
	Vector2i FindOpenPositionAdjacent(Vector2i center){
		List<int> directions = new List<int> ();
		for (int i = 0; i < 8; i++) {
			directions.Add (i);
		}
		directions = directions.Shuffle ();
		for (int i = 0; i < 8; i++) {
			Vector2i tp = new Vector2i (center.x + RL.Map.nDir [directions[i], 0], center.y + RL.Map.nDir [directions[i], 1]);
			if (
				playerMap[tp.x, tp.y] == null
				&& map.IsValidTile(tp.x, tp.y)
				&& map.IsOpenTile (tp.x, tp.y)
				&& (map[tp.x, tp.y]&(RL.Objects.ENTRANCE&RL.Objects.EXIT))==0
			) {
				return tp;
			}
		}
		return new Vector2i (-1000, -1000);
	}
	int OneCostFunction(int x, int y){
		return 1;
	}
	bool HasPath(List<Vector2i> path, Vector2i startPoint, Vector2i endPoint){
		return path.Count > 0 && path [path.Count - 1].Equals (endPoint) && path[0].Equals(startPoint);
	}
	public void PlayerSetupNewLevel(){
		foreach (RLCharacter p in players) {
			p.healthPoints = Mathf.Min(p.healthPoints+1, 3);
		}
		PlayerSetup ();
	}
	public void PlayerSetup(){
		playerActionPoints = basePlayerActionPoints;
		foreach (RLCharacter p in players) {
			p.actionPoints = p.maxActionPoints;
			p.SetState("overwatch", false);
			p.canUsePowerup = true;
			// check to see if there is any powerups that mod APs
		}
	}
	void SetCurrentPlayer(){
		currentPlayer = players [currentPlayerCounter];
		foreach (RLCharacter p in players) {
			p.current = false;
		}
		currentPlayer.current = true;
	}
	public void PlayerUpdate ()
	{
		PlayerUpdate (PlayerInput.NONE);
	}
	public void PlayerUpdate (PlayerInput externalInput)
	{
		bool performedAction = false;
		bool performedMove = false;
		PlayerInput nextInput = externalInput;
		// dealing with a bug in the tutorial of always sending the up key on the first run through the tutorial
		#if UNITY_STANDALONE
		nextInput = PlayerInput.NONE;
		#endif
		int powerupUsed = -1;
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
			nextInput = PlayerInput.POWER1;
			powerupUsed = 0;
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			nextInput = PlayerInput.POWER2;
			powerupUsed = 1;
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			nextInput = PlayerInput.POWER1;
			powerupUsed = 2;
		}
		if (Input.GetKeyDown (KeyCode.Alpha4)) {
			nextInput = PlayerInput.POWER2;
			powerupUsed = 3;
		}
		if (Input.GetKeyDown (KeyCode.Tab)) {
			nextInput = PlayerInput.NEXT_CHARACTER;
			SelectNextPlayer ();
		}
		if (Input.GetKeyDown (KeyCode.Space) && monsters.Count == 0) {
			MoveToNextLevel ();
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
		if (currentPlayer == null) {
			SelectNextPlayer ();
		}
		if (currentPlayer.canUsePowerup && powerupUsed >= 0) {
			int powerSlot = powerupUsed;
			// check to see if the player has a power in the current slot, and use it if so
			if (currentPlayer.powerups.Count > powerSlot 
				&& currentPlayer.powerups [powerSlot] != null 
				&& currentPlayer.powerups [powerSlot].OnUse (currentPlayer, this))
			{
				AudioTriggerSystem.instance ().PlayClipImmediate ("use_powerup");
				CompletePlayerActions ();
			}
		}
		if (!deltaD.Equals (new Vector2i (0, 0))) {
			UpdateMonsterMap ();
			if (
				currentPlayer.CustomMovement(deltaD, map, monsterMap, playerMap)
				|| Attack(deltaD, currentPlayer, monsterMap, playerMap)
				|| PlayerMovement (deltaD)
			) {
				// check to see if the player is on a powerup, and apply it if they are
				if (itemMap [currentPlayer.x, currentPlayer.y] != null) {
					RLItem item = itemMap [currentPlayer.x, currentPlayer.y];
					// is this an item that takes up inventory slots
					if (item.powerUp.OnPickup (currentPlayer, this)) {
						if (currentPlayer.powerups.Count < 4) {
							AudioTriggerSystem.instance ().PlayClipImmediate ("getitem");
							currentPlayer.powerups.Add (item.powerUp);
							item.Destroy ();
							items.Remove (item);
							UpdateMaps ();
						}
					} else {
						item.Destroy ();
						items.Remove (item);
						UpdateMaps ();
					}
				}
				CompletePlayerActions ();
			}
		}
	}
	void MoveToNextLevel(){
		if (currentLevel == contract.rooms) {
			OnContractComplete ();
			Destroy (ui.gameObject);
			Destroy (gameObject);
		}else{
			for (int i = monsters.Count - 1; i >= 0; i--) {
				gamePanel.elements.Remove (monsters[i]);
				monsters[i].Destroy ();
				monsters.Remove (monsters[i]);
			}

			PlayerSetupNewLevel ();
			foreach (RLCharacter p in players) {
				exitPlayers.Add (p);
			}
			players.Clear ();
			fsm.PerformTransition (FsmTransitionId.LevelComplete);
		}
	}
	bool SelectNextPlayer(){
		int apRemain = 0;
		foreach (RLCharacter p in players) {
			apRemain += p.actionPoints;
		}
		if (apRemain > 0) {
			currentPlayerCounter = (currentPlayerCounter + 1) % players.Count;
			SetCurrentPlayer ();
			while (currentPlayer.actionPoints <= 0) {
				SelectNextPlayer ();
			}
			return true;
		}
		return false;
	}
	void CompletePlayerActions(){
		UpdatePlayerMap ();
		AudioTriggerSystem.instance ().PlayClipImmediate (playerActionPoints+"moveleft");
		playerActionPoints--;
		spawnTimer++;
		if (spawnTimer - apsPerSpawn >= 0) {
			spawnTimer = 0;
			MusicSystem.GameNextEnemy ();
			AddMonster ();
		}
		int nonGhostCount = 0;
		foreach (RLCharacter m in monsters) {
			if (!m.ghost && !m.stun)
				nonGhostCount++;
		}
		if (nonGhostCount == 0) {
			MoveToNextLevel ();
		} else {
			if (playerActionPoints <= 0) {
				if (monsters.Count <= 0) {
					gameInfo.creditsEarned--;
				}
				fsm.PerformTransition (FsmTransitionId.Complete);
			}
		}
	}
	public void UpdateMaps(){
		UpdatePlayerMap ();
		UpdateMonsterMap ();
		UpdateItemMap ();
	}
	public void UpdateItemMap(){
		itemMap.Clear ();
		foreach (RLItem p in items) {
			itemMap [p.position.x, p.position.y] = p;
		}

	}
	public void UpdatePlayerMap(){
		playerMap.Clear ();
		foreach (RLCharacter p in players) {
			playerMap [p.position.x, p.position.y] = p;
		}
	}
	public bool Attack(Vector2i delta, RLCharacter firingCharacter, RL.CharacterMap<RLCharacter> cMap, RL.CharacterMap<RLCharacter> blockerMap){
		// check to see if one of the neighboring cells has a character in it, and attack if so
		Vector2i currentCell = firingCharacter.position + delta;
		int count = 0;
		while (
			map.IsValidTile(currentCell.x, currentCell.y) 
			&& map [currentCell.x, currentCell.y] == RL.Objects.OPEN
			&& blockerMap[currentCell.x, currentCell.y] == null
			&& count < firingCharacter.fireRange
		){
			count++;
			RLCharacter enemy = cMap [currentCell.x, currentCell.y];
			if (enemy != null && !enemy.stun) {
				LeanTween.move (enemy.gameObject, new Vector3[] {
					enemy.transform.position + new Vector3 (Random.value, Random.value, 0f)*0.25f,
					enemy.transform.position + new Vector3 (Random.value, Random.value, 0f)*0.25f,
					enemy.transform.position + new Vector3 (Random.value, Random.value, 0f)*0.25f,
					enemy.transform.position + new Vector3 (Random.value, Random.value, 0f)*0.25f
				}, 0.1f).setOnComplete(() => {
					enemy.transform.position = Grid.GridToWorld(enemy.x, enemy.y);
				});
				enemy.healthPoints--;
				if (monsters.Contains (enemy)) {
					AudioTriggerSystem.instance ().PlayClipImmediate ("player_attack");
				} else {
//					AudioTriggerSystem.instance ().PlayClipImmediate ("player_attack");
				}
				if (enemy.healthPoints <= 0) {
					if (monsters.Contains (enemy)) {
						// killing enemy
						monsters.Remove (enemy);
						gamePanel.elements.Remove (enemy);
						enemy.Destroy ();
						AudioTriggerSystem.instance ().PlayClipImmediate ("enemy_defeated");
//						enemy.stun = true;
					} else {
						// killing player
						gameInfo.crew.Remove (enemy.characterData);
						players.Remove (enemy);
						gamePanel.elements.Remove (enemy);
						enemy.Destroy ();
					}
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
			exitPlayers.Add (currentPlayer);
			players.Remove (currentPlayer);
			currentPlayer.Hide ();
			if (players.Count == 0) {
				fsm.PerformTransition (FsmTransitionId.LevelComplete);
			} else {
				CompletePlayerActions ();
			}
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
			if (Attack (AttackDirection, m, playerMap, monsterMap)) {
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
		if (monsterMap [path [1].x, path [1].y] == null && playerMap [path [1].x, path [1].y] == null) {
			m.position = path [1];
		}
		return true;
	}
	public void CheckOverwatch(){
		UpdateMonsterMap ();
		foreach (RLCharacter p in players) {
			if (p.GetState("overwatch")) {
				for (int i = 0; i < 4; i++) {
					Vector2i AttackDirection = new Vector2i (RL.Map.nDir [i, 0], RL.Map.nDir [i, 1]);
					if (Attack (AttackDirection, p, monsterMap, playerMap)) {
						// if the attack happens on the first turn, should remove action points
						p.SetState("overwatch", false);
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
		yield return new WaitForSeconds (0.55f);
		CheckOverwatch ();
		for (int i = monsters.Count - 1; i >= 0; i--) {
			RLCharacter m = monsters [i];
			while (m.actionPoints > 0) {
				// path towards the player or attack. Should probably add overwatch support at some point for asshole monsters
				if (m.stun) {
					m.actionPoints = 0;
					m.stun = false;
					m.ghost = true;
				}else if (MonsterAttack (m)) {
					m.actionPoints = 0;
					AudioTriggerSystem.instance ().PlayClipImmediate ("enemyattack");
					yield return new WaitForSeconds (0.15f);
					continue;
				}else if(MonsterMove (m)){
					AudioTriggerSystem.instance ().PlayClipImmediate ("enemymove");

					CheckOverwatch ();
					if(m != null)
						m.actionPoints--;
					yield return new WaitForSeconds (0.15f);
					// should do a little delay here to visually process everything
				}
			}
		}
		foreach (RLCharacter m in monsters) {
			m.actionPoints = 2;
		}
		StartCoroutine (WaitAndCall(.55f, () => {
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
		for (int x = 0; x < sx; x++) {
			for (int y = 0; y < sy; y++) {
				if (map [x, y] == element) {
					return new Vector2i (x, y);
				}
			}
		}
		return new Vector2i(-1000, -1000);
	}
	public void UpdateMapDisplay(){
		for (int x = 0; x < sx; x++) {
			for (int y = 0; y < sy; y++) {
				GridSVG svg;
				switch (map [x, y]) {
				case RL.Objects.WALL:
					svg = (GridSVG)gamePanel.Add (GridSVG.CreateFromSvg (x, y, contract.svgName+"wall"));
					svg.color = contract.contractColor;
					break;
				case RL.Objects.ENTRANCE:
					svg = (GridSVG)gamePanel.Add(GridSVG.Create (x, y, "airlock"));
					svg.color = GameColors.GetColor ("wall");
					break;
				case RL.Objects.EXIT:
					svg = (GridSVG)gamePanel.Add(GridSVG.Create (x, y, "airlock"));
					svg.color = GameColors.GetColor ("wall");
					break;
				}
			}
		}
	}
	public void HideHighlights(){
		for (int x = 0; x < sx; x++) {
			for (int y = 0; y < sy; y++) {
				highlights [x, y].color = Color.clear;
			}
		}
	}
	void OnDestroy(){
		foreach (DisplayElement highlight in highlightPanel.elements) {
			highlight.Destroy ();
		}
		if(highlightPanel != null)
			Destroy (highlightPanel.gameObject);
	}
	void Update(){
		if(fsm != null)
			fsm.CurrentState.Update ();
	}
}