using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ContractInfo{
	public string 	svgName = "";
	public enum 	NameType {
		SHIP,
		PLANET
	};
	public NameType nameType;
	public string 	name;
	public Color	contractColor;
	public int 		days;
	public int 		rooms;
	public static ContractInfo[] contracts = new ContractInfo[] {
		new ContractInfo{
			svgName = "derilict_0",
			nameType = NameType.SHIP
		},
		new ContractInfo{
			svgName = "derilict_1",
			nameType = NameType.SHIP
		},
		new ContractInfo{
			svgName = "derilict_2",
			nameType = NameType.SHIP
		},
		new ContractInfo{
			svgName = "derilict_3",
			nameType = NameType.PLANET
		},
		new ContractInfo{
			svgName = "derilict_4",
			nameType = NameType.PLANET
		},
		new ContractInfo{
			svgName = "derilict_5",
			nameType = NameType.PLANET
		}

	};
	public void Init(){
		rooms = Random.Range (3, 6);
		days = Random.Range (2, 5);
		switch(nameType){
		case NameType.SHIP:
			name = NameGen.GetShipName ();
			contractColor = GameColors.GetColor ("derelict" + Random.Range (0, 4));
			break;
		case NameType.PLANET:
			name = NameGen.GetPlanetName ();
			contractColor = GameColors.GetColor ("planet" + Random.Range (0, 4));
			break;
		}
	}
}
public class NameGen {

	Dictionary<string, object> opDict;
	List<object> opStrings;

	Dictionary<string, object> NSADict;
	List<object> NSAStrings;

	static NameGen _instance;
	public static NameGen instance{
		get{
			if (_instance == null)
				_instance = new NameGen ();
			return _instance;
		}
	}
	public NameGen(){
		// load in all the dictionaries
		opDict = Json.Deserialize((Resources.Load("textAssets/militaryOperations") as TextAsset).text) as Dictionary<string,object>;
		opStrings = (List<object>)opDict ["operations"];

		NSADict = Json.Deserialize((Resources.Load("textAssets/nsaNames") as TextAsset).text) as Dictionary<string,object>;
		NSAStrings = (List<object>)NSADict ["codenames"];
	}
	public static string GetShipName(){
		return instance.opStrings[Random.Range(0, instance.opStrings.Count)].ToString().ToUpper();
	}
	public static string GetPlanetName(){
		string postfix = "";
		if (Random.Range (0, 4) == 0) {
			postfix = Random.Range (0, 200).ToString();
		}
		string mainString = instance.NSAStrings [Random.Range (0, instance.NSAStrings.Count)].ToString ();
		if (mainString.Length < 7 || (mainString.Split (' ').Length == 1 && Random.Range(0, 5) == 0)) {
			mainString += " " + instance.NSAStrings [Random.Range (0, instance.NSAStrings.Count)].ToString ();
		}
		return mainString + postfix;
	}
}
public class ContractPicker : MonoBehaviour {
	string[] introStrings;
	Panel panel;
	List<ContractInfo> contracts = new List<ContractInfo>();
	public float lineHeight = 0;
	public System.Action<ContractInfo> StartGame;
	bool pickingContract = true;
	// Use this for initialization
	bool setup = false;
	public void Init (GameInfo gameInfo) {
		Compression.PopBlur (transform, 0, 0.1f, 0);
		string shipName = NameGen.GetShipName ();
		string introductionText = "crew of the {0}, you made a grave mistake. \nYou thought you could afford those bodies,\nor at least escape from your creditors.\n" +
			"now you have {1} days to pay back the {2} credits you owe,\nbefore you are repossesed.";
		introductionText = System.String.Format (introductionText, gameInfo.shipName, gameInfo.daysRemaining, gameInfo.totalCredits-gameInfo.creditsEarned);
		introStrings = introductionText.Split ('\n');

		// add all the contracts that we can take out
		List<ContractInfo> contractsToShuffle = new List<ContractInfo> (ContractInfo.contracts);
		contractsToShuffle = contractsToShuffle.Shuffle ();
		for (int i = 0; i < 3; i++) {
			contractsToShuffle [i].Init ();
			contracts.Add (contractsToShuffle [i]);
		}
		panel = Panel.Create ();
		for (int i = 0; i < 3; i++) {
			GridSVG contractSvg = GridSVG.CreateFromSvg (-4.5f, 3.5f + i * 2, contracts [i].svgName);
			contractSvg.color = contracts [i].contractColor;
			panel.Add (contractSvg);
		}
		setup = true;
	}
	void Update(){
		if (setup)
			DisplayUpdate ();
	}
	// Update is called once per frame
	void DisplayUpdate () {
		// input for selecting contracts
		Vector2i gp = Grid.WorldToGrid (Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y)));
		int selectedContract = -1;
		if (gp.x >= -5&&gp.x<=6
			&& gp.y >= 3 && gp.y <= 8
		) {
			selectedContract = (gp.y - 3) / 2;
			if (Input.GetMouseButtonDown (0) && pickingContract) {
				StartGame (contracts [selectedContract]);
				pickingContract = false;
				Destroy (gameObject);
			}
		}
		// display the story information
		if (introStrings != null) {
			VectorGui.SetPosition (new Vector2(-5.35f, 0.35f));
			foreach (string s in introStrings) {
				VectorGui.Label (s, 0.1f, Color.white);
			}
		}
		// display the information for the contracts
		for (int i = 0; i < contracts.Count; i++) {
			Color c = Color.white;
			if (i == selectedContract)
				c = GameColors.GetColor("player");
			VectorGui.SetPosition (new Vector2(-2.35f, -2*i-2.65f));
			VectorGui.Label (contracts[i].name, 0.2f, c);
			VectorGui.SetPosition (new Vector2(-2.35f, -2*i-3.65f));
			VectorGui.Label ("Days: "+contracts[i].days, 0.1f, c);
			VectorGui.SetPosition (new Vector2(1.65f, -2*i-3.65f));
			VectorGui.Label ("Min Credits: "+contracts[i].rooms*PUScoreUp.scoreValue, 0.1f, c);
		}
		VectorGui.SetPosition (new Vector2(-2.35f, -8.65f));
		VectorGui.Label ("Click To Select Contract", 0.1f, Color.white);
	}
	void OnDestroy(){
		if (panel != null) {
			// destroy everything on the current panel and get the game displaying
			foreach (DisplayElement de in panel.elements) {
				de.Destroy ();
			}
			Destroy (panel.gameObject);
		}
	}
}
