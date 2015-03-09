using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlayerInput
{
	UP,
	DOWN,
	LEFT,
	RIGHT,
	SPECIAL,
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
	public List<RLCharacter> characters;
	public List<RLCharacter> monsters;
	FsmSystem fsm;
	RL.Pathfinder pf = new RL.Pathfinder ();

	Panel gamePanel;
	RLCharacter currentPlayer;
	public void Build(Panel _gamePanel){
		gamePanel = _gamePanel;
		fsm = new FsmSystem ();
		fsm.AddState (new FsmState (FsmStateId.InitialGen)
			.WithBeforeEnteringAction(GenLevel)
			.WithTransition (FsmTransitionId.Complete, FsmStateId.Player)
		);
		fsm.AddState (new FsmState (FsmStateId.Player)
			.WithUpdateAction(PlayerUpdate)
			.WithTransition (FsmTransitionId.Complete, FsmStateId.Monster)
			.WithTransition(FsmTransitionId.LevelComplete, FsmStateId.LevelGenWait)
		);
		fsm.AddState (new FsmState (FsmStateId.LevelGenWait)
			.WithBeforeEnteringAction(GenLevel)
			.WithTransition (FsmTransitionId.Complete, FsmStateId.Player)
		);
		fsm.AddState (new FsmState (FsmStateId.Monster)
			.WithUpdateAction(MonsterUpdate)
			.WithTransition (FsmTransitionId.Complete, FsmStateId.Player)
		);
		fsm.Start ();
	}
	public void GenLevel(){
		// destroy everything on the existing panel, and build a new level
		foreach (DisplayElement de in gamePanel.elements) {
			de.Destroy ();
		}
		map = new RL.Map (10, 10);
		monsters = new List<RLCharacter> ();
		// add entrance / exit
		map [0, Random.Range (1, 9)] = RL.Objects.ENTRANCE;
		map [9, Random.Range (1, 9)] = RL.Objects.EXIT;
		//		// put in some random walls
		for (int i = 0; i < 10; i++) {
			map [Random.Range (1, 9), Random.Range (1, 9)] = RL.Objects.WALL;
		}
		//		// add in a player character
		Vector2i pposition = GetPositionOfElement (RL.Objects.ENTRANCE) + new Vector2i (1, 0);
		RLCharacter player = RLCharacter.Create (pposition.x, pposition.y, "Player");
		gamePanel.Add (player);
		currentPlayer = player;
		// check to make sure we can walk from the beginning to the end of the level
		Vector2i exit = GetPositionOfElement (RL.Objects.EXIT);
		List<Vector2i> path = pf.FindPath (GetPositionOfElement (RL.Objects.ENTRANCE), exit, (int x, int y) => {
			return 1;
		}, map); 
		if (HasPath (path, exit)) {
			UpdateMapDisplay ();
			fsm.PerformTransition (FsmTransitionId.Complete);
		} else {
			GenLevel ();
		}
	}
	bool HasPath(List<Vector2i> path, Vector2i endPoint){
		return path.Count > 0 && path [path.Count - 1].Equals (endPoint);
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
		if (Input.GetKeyDown (KeyCode.Space)) {
			nextInput = PlayerInput.SPECIAL;
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
		if (!deltaD.Equals (new Vector2i (0, 0))) {
			if (PlayerMovement (deltaD)) {
				fsm.PerformTransition (FsmTransitionId.Complete);
			}
		}
	}
	public bool PlayerMovement(Vector2i delta){
		Vector2i newPosition = currentPlayer.position + delta;
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
	public void MonsterUpdate(){
		foreach (RLCharacter m in monsters) {
		
		}
		fsm.PerformTransition (FsmTransitionId.Complete);
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