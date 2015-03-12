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
		switch (Random.Range (0, 3)) {
		case 0:
			return new PUEndTurn ();
		case 1:
			return new PUHealthUp ();
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
		c.overwatch = true;
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