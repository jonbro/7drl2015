using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
	public string InventoryText(){
		return DisplayText () + "(" + saleValue + ")";
	}
	public int saleValue = 4;
	public int useCount = 0;
	public int actionPointModifier;
	public int rangeModifier;
	public static List<PowerUp> shuffledPowerups = new List<PowerUp> ();

	public static PowerUp GetPowerup(){
		if (shuffledPowerups.Count == 0) {
			shuffledPowerups.Add (new PUDelayedHeal ());
			shuffledPowerups.Add (new PUHealthUp ());
			shuffledPowerups.Add (new PUFastMove ());
//			shuffledPowerups.Add (new PUScoreUp ());
			shuffledPowerups.Add (new PUAPToCredit ());
			shuffledPowerups.Add (new PUOverwatch ());
			shuffledPowerups = shuffledPowerups.Shuffle ();
		}
		PowerUp toReturn = shuffledPowerups[shuffledPowerups.Count-1];
		shuffledPowerups.Remove (toReturn);
		return toReturn;
	}
}

public class PUOverwatch : PowerUp {
	override public string DisplayText(){ return "+1 RANGE"; }
	override public string DescriptionText(){
		return "+1 Range";
	}
	override public string SvgIcon(){
		return "Overwatch";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		this.saleValue = 1;
		this.rangeModifier = 1;
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		return false;
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
	override public string DisplayText(){ return "1x FILL HP"; }

	override public string DescriptionText(){
		return "REFILL HP";
	}
	override public string SvgIcon(){
		return "HealthUp";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		this.saleValue = 3;
		return true;
	}
	override public bool OnUse(RLCharacter c, Level level){
		// should destroy self and immediately add 2 to the players health
		c.healthPoints = 3;
		c.powerups.Remove (this);
		return true;
	}
}
public class PUScoreUp : PowerUp {
	public int saleValue = 2;
	override public string DisplayText(){ return "CREDITS"; }
	override public string DescriptionText(){
		return "SELL FOR "+saleValue+" CREDITS";
	}
	override public string SvgIcon(){
		return "scoreToken";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		this.saleValue = 2;
		// adds 5 to score
		return true;
	}
	override public bool OnUse(RLCharacter c, Level level){
		return false;
	}
}
public class PUFastMove : PowerUp {
	int charges = 5;
	override public string DisplayText(){ return charges+"x FAST"; }
	override public string DescriptionText(){
		return "MOVE FAST IN DIRECTION";
	}
	override public string SvgIcon(){
		return "fastMove";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		this.saleValue = 1;
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		base.OnUse (c, level);
		// set fast move on the character, and allow them to take another turn
		c.canUsePowerup = false;
		c.SetState("fastMove",true);
		charges--;
		if (charges <= 0) {
			c.powerups.Remove (this);
		}
		return false;
	}
}
public class PUDelayedHeal : PowerUp {
	int charges = 3;
	override public string DisplayText(){ return charges+ "x HEAL CREW"; }
	override public string DescriptionText(){
		return "HEAL CREW";
	}
	override public string SvgIcon(){
		return "delayedHeal";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		this.saleValue = 1;
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		base.OnUse (c, level);
		foreach (RLCharacter p in level.players) {
			if (p != c)
				p.healthPoints++;
		}
		charges--;
		if (charges <= 0) {
			c.powerups.Remove (this);
		}
		this.saleValue = Mathf.Max (this.saleValue - 1, 0);
		return true;
	}
}
public class PUAPRefresh : PowerUp {
	int charges = 3;
	override public string DisplayText(){ return charges+"x AP+1"; }
	override public string DescriptionText(){
		return "ADD ONE TO AP";
	}

	override public string SvgIcon(){
		return "apRefresh";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		this.actionPointModifier = 1;
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		// should use this anytime they spend the third action point
		if (charges > 0) {
			base.OnUse (c, level);
			level.playerActionPoints++;
			charges--;
		}
		return false;
	}
}

public class PUAPToCredit : PowerUp {
	int charges = 5;
	override public string DisplayText(){ return charges+"x VALUE++"; }
	override public string DescriptionText(){
		return "INCREASE SALE VALUE";
	}
	override public string SvgIcon(){
		return "ap_to_credit";
	}
	override public bool OnPickup (RLCharacter c, Level level){
		this.saleValue = 0;
		return true;
	}
	override public bool OnUse (RLCharacter c, Level level){
		// should use this anytime they spend the third action point
		if (charges > 0) {
			charges--;
			this.saleValue++;
			return true;
		}
		return false;
	}
}