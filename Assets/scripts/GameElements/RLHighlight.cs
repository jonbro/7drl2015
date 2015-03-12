using UnityEngine;
using System.Collections;

public class RLHighlight : GridSVG{
	public static RLHighlight CreateFromSvg(int x, int y, string resourceName, Grid.Offset _offset = Grid.Offset.CENTER){
		GameObject go = (GameObject)(Instantiate(Resources.Load("RLHighlight") as GameObject, Grid.GridToWorld(x,y)+Grid.OffsetToVector(_offset), Quaternion.identity));
		RLHighlight gs = go.GetComponent<RLHighlight>();
		gs.GetComponent<SvgRenderer> ().LoadSvgFromResources (resourceName);
		gs.position = new Vector2i (x, y);
		return gs;
	}
	public Vector2i position;
}