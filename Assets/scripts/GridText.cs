using UnityEngine;
using System.Collections;

public class GridText : DisplayElement {
	public TextMesh text;
	public string textString;
	public static GridText Create(int x, int y, string text, Grid.Offset _offset = Grid.Offset.CENTER_LEFT){
		GameObject go = (GameObject)(Instantiate(Resources.Load("GridText") as GameObject, Grid.GridToWorld(x,y)+Grid.OffsetToVector(_offset), Quaternion.identity));
		GridText gt = go.GetComponent<GridText> ();
		gt.textString = text;
		gt.text.text = text;
		return gt;
	}
	void Awake(){
		text = GetComponent<TextMesh> ();
	}
	override public void Destroy(){
		StartCoroutine (DestroySlow());
	}
	IEnumerator DestroySlow(){
		// hide slow, then destroy the object
		yield return StartCoroutine(HideSlowCoro ());
		Destroy(gameObject);
	}
	public void Hide(){
		StartCoroutine (HideSlowCoro());
	}
	IEnumerator HideSlowCoro(){
		while (textString.Length > 0) {
			// 1/5 chance to replace a letter rather than removing
			if(Random.Range(0,4)==0){
				textString = textString.Insert(Random.Range (0, textString.Length - 1), ((char)Random.Range (30, 128)).ToString());
			}else{
				// pick a random letter from the string and remove it
				int letterToRemove = Random.Range (0, textString.Length);
				textString = textString.Remove (letterToRemove, 1);
			}
			text.text = textString;
			yield return new WaitForSeconds (0.02f);
		}
		yield return null;
	}
}
