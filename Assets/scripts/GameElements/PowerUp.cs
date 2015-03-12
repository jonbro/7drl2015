using UnityEngine;
using System.Collections;

public class PowerUp {
	private string _descriptionText;
	public string descriptionText{
		set{ _descriptionText = value; }
		get{ return _descriptionText; }
	}
	virtual public bool OnPickup (RLCharacter c, Level level){ return false; }
	virtual public bool OnUse (RLCharacter c, Level level){ return false; }
	virtual public string SvgIcon(){
		return "";
	}
	virtual public string DisplayText(){
		return "";
	}
	public static PowerUp GetPowerup(){
		switch (Random.Range (0, 4)) {
		case 0:
			return new PUDelayedHeal ();
		case 1:
			return new PUHealthUp ();
		case 2:
			return new PUFastMove ();
		case 3:
			return new PUScoreUp ();
		default:
			return new PUOverwatch ();
		}
	}
}

public class PUOverwatch : PowerUp {
	override public string DisplayText(){ return "OVRWTCH"; }
	public string descriptionText = "END TURN AND FIRE IN RANGE ON ENEMY TURN";
	override public string SvgIcon(){
		return "Overwatch";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		c.canUsePowerup = false;
		c.SetState("overwatch",true);
		return true;
	}
}
public class PUEndTurn : PowerUp {
	public string descriptionText = "END TURN";
	override public string SvgIcon(){ return "EndTurn"; }
	override public string DisplayText(){ return"END"; }
	override public bool OnPickup (RLCharacter c, Level level){
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		return true;
	}
}
public class PUHealthUp : PowerUp {
	override public string DisplayText(){ return "ADD HP"; }
	public string descriptionText = "IMMEDIATELY ADD 2 TO HEALTH";
	override public string SvgIcon(){
		return "HealthUp";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		// should destroy self and immediately add 2 to the players health
		c.healthPoints = Mathf.Max(c.healthPoints+2, 3);
		return false;
	}
}
public class PUScoreUp : PowerUp {
	override public string DisplayText(){ return "SCORE UP"; }
	public string descriptionText = "ADD 5 TO SCORE";
	override public string SvgIcon(){
		return "scoreToken";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		// adds 5 to score
		level.score += 5;
		return false;
	}
}
public class PUFastMove : PowerUp {
	override public string DisplayText(){ return "MOVE FAST"; }
	public string descriptionText = "MOVE REALLY FAST IN DIRECTION";
	override public string SvgIcon(){
		return "fastMove";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		// set fast move on the character, and allow them to take another turn
		c.canUsePowerup = false;
		c.SetState("fastMove",true);
		return false;
	}
}
public class PUDelayedHeal : PowerUp {
	int charges = 3;
	override public string DisplayText(){ return "HEAL NEIGHBORS ("+charges+")"; }
	public string descriptionText = "HEAL NEIGHBORS";

	override public string SvgIcon(){
		return "delayedHeal";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		// check to see if there are any neighbors, and heal them if so
		level.UpdateMaps ();
		for (int i = 0; i < 8; i++) {
			Vector2i nDir = c.position + new Vector2i (RL.Map.nDir [i, 0], RL.Map.nDir [i, 1]);
			if (level.playerMap [nDir.x, nDir.y] != null) {
				level.playerMap [nDir.x, nDir.y].healthPoints++;
				charges--;
				if (charges <= 0)
					break;
			}
		}
		if (charges <= 0) {
			c.powerups.Remove (this);
		}
		return true;
	}
}