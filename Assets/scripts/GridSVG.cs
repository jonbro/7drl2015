using UnityEngine;
using System.Collections;

public class GridSVG : DisplayElement {
	public static GridSVG Create(int x, int y, string resourceName, Grid.Offset _offset = Grid.Offset.CENTER){
		GameObject go = (GameObject)(Instantiate(Resources.Load(resourceName) as GameObject, Grid.GridToWorld(x,y)+Grid.OffsetToVector(_offset), Quaternion.identity));
		return go.GetComponent<GridSVG>();
	}
}
