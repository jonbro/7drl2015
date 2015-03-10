using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class DisplayElement : MonoBehaviour {
	public List<Panel> OnPanels = new List<Panel>();
	public void HideImmediate(){
		GetComponent<SpriteRenderer> ().enabled = false;
		foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>()) {
			sr.enabled = false;
		}
	}
	public void Hide(){
		HideImmediate ();
	}
	public void Show(){
		GetComponent<SpriteRenderer> ().enabled = true;
		foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>()) {
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
