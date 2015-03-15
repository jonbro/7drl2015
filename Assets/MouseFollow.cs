using UnityEngine;
using System.Collections;

public class MouseFollow : MonoBehaviour {

	Camera cam;

	// Use this for initialization
	void Start () {
		cam = FindObjectOfType<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = cam.ScreenToWorldPoint(Input.mousePosition);
	}
}
