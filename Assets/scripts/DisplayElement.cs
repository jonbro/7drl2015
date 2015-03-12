using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class DisplayElement : MonoBehaviour {
	public List<Panel> OnPanels = new List<Panel>();
	public void HideImmediate(){
		if(GetComponent<Renderer> ())
			GetComponent<Renderer> ().enabled = false;
		foreach (Renderer sr in GetComponentsInChildren<Renderer>()) {
			sr.enabled = false;
		}
	}
	public void Hide(){
		HideImmediate ();
	}
	public void Show(){
		if (GetComponent<Renderer> ()) {
			GetComponent<Renderer> ().enabled = true;
		}
		foreach (Renderer sr in GetComponentsInChildren<Renderer>()) {
			sr.enabled = true;
		}
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
