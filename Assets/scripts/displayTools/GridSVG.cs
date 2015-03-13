using UnityEngine;
using System.Collections;

public class GridSVG : DisplayElement {
	virtual public Color color{
		set{
			GetComponent<SvgRenderer> ().colorProperty = value;
		}
	}
	public static GridSVG CreateFromSvg(float x, float y, string resourceName, Grid.Offset _offset = Grid.Offset.CENTER){
		GameObject go = (GameObject)(Instantiate(Resources.Load("GridSvg") as GameObject, Grid.GridToWorld(x,y)+Grid.OffsetToVector(_offset), Quaternion.identity));
		GridSVG gs = go.GetComponent<GridSVG>();
		gs.GetComponent<SvgRenderer> ().LoadSvgFromResources (resourceName);
		return gs;
	}
	public static GridSVG Create(int x, int y, string resourceName, Grid.Offset _offset = Grid.Offset.CENTER){
		GameObject go = (GameObject)(Instantiate(Resources.Load(resourceName) as GameObject, Grid.GridToWorld(x,y)+Grid.OffsetToVector(_offset), Quaternion.identity));
		return go.GetComponent<GridSVG>();
	}

}
