using UnityEngine;
using System.Collections;

public class CameraToGridDisplay : MonoBehaviour {
	Camera cam;
	// Use this for initialization
	Material mat;
	void Start () {
		cam = Camera.main;
		mat = renderer.sharedMaterial;
	}
	
	// Update is called once per frame
	void Update () {
		// apply the camera size to the grid size
//		if (Input.GetKeyDown (KeyCode.G)) {

			// convert world unit size into pixel size
			float pixelSize = Mathf.Abs(cam.WorldToScreenPoint (Vector3.zero).x - cam.WorldToScreenPoint (Vector3.one).x);
		Vector3 screenOffset = -cam.WorldToScreenPoint (Vector3.zero);
		mat.SetVector ("_Shape", new Vector4 (pixelSize, 0.5f, pixelSize * 1f/3f, 0));
			mat.SetVector ("_Offset", screenOffset);
//		}
	}
}
