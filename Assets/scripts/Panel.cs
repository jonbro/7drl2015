using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// a container for grid elements, so that we can display / hide chunks of the ui at once
public class Panel : MonoBehaviour {
	public List<DisplayElement> elements;
	public static Panel Create(){
		GameObject go = (GameObject)(Instantiate(Resources.Load("Panel") as GameObject, Vector3.zero, Quaternion.identity));
		return go.GetComponent<Panel> ();
	}
	public DisplayElement Add(DisplayElement element){
		if (element.OnPanels.Count == 0)
			element.transform.SetParent (transform);
		element.OnPanels.Add (this);
		elements.Add (element);
		return element;
	}
}
