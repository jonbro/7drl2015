using UnityEngine;
using System.Collections;

public class RLHighlight : GridSVG{
	Color tweenColor = Color.clear;
	float startTime = 0;
	private IEnumerator colorRoutine;

	override public Color color{
		set{
			if (colorRoutine != null)
				StopCoroutine (colorRoutine);
			colorRoutine = TweenColor (GetComponent<SvgRenderer> ().colorProperty, value, 0.25f);
			StartCoroutine(colorRoutine);
		}
	}
	IEnumerator TweenColor(Color startColor, Color endColor, float time){
		float startTime = Time.time;
		while (Time.time - startTime < time) {
			float percent = (Time.time - startTime)/time;
			GetComponent<SvgRenderer> ().colorProperty = Color.Lerp (startColor, endColor, percent);
			yield return new WaitForEndOfFrame ();
		}
		GetComponent<SvgRenderer> ().colorProperty = endColor;
	}
	public static RLHighlight CreateFromSvg(int x, int y, string resourceName, Grid.Offset _offset = Grid.Offset.CENTER){
		GameObject go = (GameObject)(Instantiate(Resources.Load("RLHighlight") as GameObject, Grid.GridToWorld(x,y)+Grid.OffsetToVector(_offset), Quaternion.identity));
		RLHighlight gs = go.GetComponent<RLHighlight>();
		gs.GetComponent<SvgRenderer> ().LoadSvgFromResources (resourceName);
		gs.position = new Vector2i (x, y);
		return gs;
	}
	public Vector2i position;
}