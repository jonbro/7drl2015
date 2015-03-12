using UnityEngine;
using System.Collections;

public class PowerUp {
	private string _descriptionText;
	public string descriptionText{
		set{ _descriptionText = value; }
		get{ return _descriptionText; }
	}
	virtual public bool OnPickup (RLCharacter c){ return false; }
	virtual public bool OnUse (RLCharacter c){ return false; }
	virtual public string SvgIcon(){
		return "";
	}
	virtual public string DisplayText(){
		return "";
	}
	public static PowerUp GetPowerup(){
		switch (Random.Range (1, 4)) {
		case 1:
			return new PUHealthUp ();
		case 2:
			return new PUFastMove ();
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
	override public bool OnPickup(RLCharacter c){
		return true;
	}
	override public bool OnUse(RLCharacter c){
		c.canUsePowerup = false;
		c.SetState("overwatch",true);
		return true;
	}
}
public class PUEndTurn : PowerUp {
	public string descriptionText = "END TURN";
	override public string SvgIcon(){ return "EndTurn"; }
	override public string DisplayText(){ return"END"; }
	override public bool OnPickup(RLCharacter c){
		return true;
	}
	override public bool OnUse(RLCharacter c){
		return true;
	}
}
public class PUHealthUp : PowerUp {
	override public string DisplayText(){ return "ADD HP"; }
	public string descriptionText = "IMMEDIATELY ADD 2 TO HEALTH";
	override public string SvgIcon(){
		return "HealthUp";
	}
	override public bool OnPickup(RLCharacter c){
		// should destroy self and immediately add 2 to the players health
		c.healthPoints = Mathf.Max(c.healthPoints+2, 3);
		return false;
	}
}
public class PUFastMove : PowerUp {
	override public string DisplayText(){ return "MOVE FAST"; }
	public string descriptionText = "MOVE REALLY FAST IN DIRECTION";
	override public string SvgIcon(){
		return "fastMove";
	}
	override public bool OnPickup(RLCharacter c){
		return true;
	}
	override public bool OnUse(RLCharacter c){
		// set fast move on the character, and allow them to take another turn
		c.canUsePowerup = false;
		c.SetState("fastMove",true);
		return false;
	}
}