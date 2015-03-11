using UnityEngine;
using System.Collections;

public class RLItem : GridSVG{
	public static RLItem CreateFromSvg(int x, int y, string resourceName, Grid.Offset _offset = Grid.Offset.CENTER){
		GameObject go = (GameObject)(Instantiate(Resources.Load("RLItem") as GameObject, Grid.GridToWorld(x,y)+Grid.OffsetToVector(_offset), Quaternion.identity));
		RLItem gs = go.GetComponent<RLItem>();
		gs.GetComponent<SvgRenderer> ().LoadSvgFromResources (resourceName);
		gs.position = new Vector2i (x, y);
		return gs;
	}
	public Vector2i position;
	public PowerUp powerUp;
}