using UnityEngine;
using System.Collections;

public class PowerUp {
	virtual public string DescriptionText(){
		return "";
	}
	virtual public bool OnPickup (RLCharacter c, Level level){ return false; }
	virtual public bool OnUse (RLCharacter c, Level level){ 
		useCount++;
		return false; 
	}
	virtual public string SvgIcon(){
		return "";
	}
	virtual public string DisplayText(){
		return "";
	}
	virtual public int saleValue {
		get{ return Mathf.Max(0, 4-useCount); }
	}
	public int useCount = 0;
	public int actionPointModifier;
	public static PowerUp GetPowerup(){
		switch (Random.Range (0, 6)) {
		case 0:
			return new PUDelayedHeal ();
		case 1:
			return new PUHealthUp ();
		case 2:
			return new PUFastMove ();
		case 3:
			return new PUScoreUp ();
		case 4:
			return new PUAPRefresh ();
		default:
			return new PUOverwatch ();
		}
	}
}

public class PUOverwatch : PowerUp {
	override public string DisplayText(){ return "OVRWTCH"; }
	override public string DescriptionText(){
		return "END TURN AND FIRE IN RANGE ON ENEMY TURN";
	}
	override public string SvgIcon(){
		return "Overwatch";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		base.OnUse (c, level);
		c.canUsePowerup = false;
		c.SetState("overwatch",true);
		return true;
	}
}
public class PUEndTurn : PowerUp {
	override public string DescriptionText(){
		return "END TURN";
	}
	override public string SvgIcon(){ return "EndTurn"; }
	override public string DisplayText(){ return "END"; }
	override public bool OnPickup (RLCharacter c, Level level){
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		return true;
	}
}
public class PUHealthUp : PowerUp {
	override public string DisplayText(){ return "ADD HP"; }
	override public string DescriptionText(){
		return "IMMEDIATELY ADD 2 TO HEALTH";
	}
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
	public static int scoreValue = 4;
	override public string DisplayText(){ return "SCORE UP"; }
	override public string DescriptionText(){
		return "ADD "+scoreValue+" TO CREDITS";
	}
	override public string SvgIcon(){
		return "scoreToken";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		// adds 5 to score
		level.gameInfo.creditsEarned += scoreValue;
		return false;
	}
}
public class PUFastMove : PowerUp {
	override public string DisplayText(){ return "MOVE FAST"; }
	override public string DescriptionText(){
		return "MOVE FAST IN DIRECTION";
	}
	override public string SvgIcon(){
		return "fastMove";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		base.OnUse (c, level);
		// set fast move on the character, and allow them to take another turn
		c.canUsePowerup = false;
		c.SetState("fastMove",true);
		return false;
	}
}
public class PUDelayedHeal : PowerUp {
	int charges = 3;
	override public string DisplayText(){ return "HEAL NEIGHBORS ("+charges+")"; }
	override public string DescriptionText(){
		return "HEAL NEIGHBORS";
	}
	override public string SvgIcon(){
		return "delayedHeal";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		base.OnUse (c, level);
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
public class PUAPRefresh : PowerUp {
	int charges = 3;
	override public string DisplayText(){ return "PSV AP+1"; }
	override public string DescriptionText(){
		return "AP MAX+1";
	}

	override public string SvgIcon(){
		return "apRefresh";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		c.actionPoints++;
		this.actionPointModifier = 1;
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		// should use this anytime they spend the third action point
		base.OnUse (c, level);
		return false;
	}
}