using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class DisplayElement : MonoBehaviour {
	public List<Panel> OnPanels = new List<Panel>();
	public void HideImmediate(){

	}
	public void Hide(){
		HideImmediate ();
	}
	virtual public void Destroy(){
		GameObject.Destroy (gameObject);
	}
	void OnDestroy(){
		foreach (Panel p in OnPanels) {
			p.elements.Remove (this);
		}
	}
}
