using UnityEngine;
using System.Collections;

public class PowerUp {
	private string _displayText;
	public string displayText{
		set{ _displayText = value; }
		get{ return _displayText; }
	}
	private string _descriptionText;
	public string descriptionText{
		set{ _descriptionText = value; }
		get{ return _descriptionText; }
	}
	virtual public bool OnPickup (RLCharacter c){ return false; }
	virtual public bool OnUse (){ return false; }
	virtual public string SvgIcon(){
		return "";
	}
	public static PowerUp GetPowerup(){
		switch (Random.Range (0, 2)) {
		case 1:
			return new PUHealthUp ();
		default:
			return new PUOverwatch ();
		}
	}
}

public class PUOverwatch : PowerUp {
	public string displayText = "OVRWTCH";
	public string descriptionText = "END TURN AND FIRE IN RANGE ON ENEMY TURN";
	override public string SvgIcon(){
		return "Overwatch";
	}
}
public class PUHealthUp : PowerUp {
	public string displayText = "INCREASE HEALTH";
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