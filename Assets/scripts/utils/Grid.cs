using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// we are using 1 unity unit to 1 grid unit, but this takes an offset such that top left is 0,0
// going to go with at 30x20 grid 
public class Grid : MonoBehaviour {
	public enum Offset
	{
		UPPER_LEFT,
		CENTER_LEFT,
		CENTER
	}

	static Grid _instance;
	static float gridSizeX, gridSizeY;

	static float offsetX = -15;
	static float offsetY = 10;
	public static int[,] nDir = { {0, -1}, {-1, 0}, {1, 0}, {0, 1},
		{-1, -1}, {1, -1},
		{-1, 1}, {1, 1} };

	public static Grid instance{
		get{ 
			if (_instance == null) {
				GameObject gridGO = new GameObject("grid");
				_instance = gridGO.AddComponent<Grid> ();
			}
			return _instance;
		}
	}
	public static Vector3 GridToWorld(Vector2 v){
		return new Vector3 (v.x + Grid.offsetX, -v.y + Grid.offsetY);
	}
	public static Vector3 GridToWorld(int x, int y){
		return GridToWorld (new Vector2 (x, y));
	}
	public static void SetCameraSize(int _gridSizeX, int _gridSizeY, float paddingX, float paddingY){
		Camera cam = Camera.main;
		gridSizeX = _gridSizeX;
		gridSizeY = _gridSizeY;
		offsetX = -gridSizeX / 2f;
		offsetY = gridSizeY / 2f;
		// determine which size is larger when compared to the aspect ratio

		// for now, just force based on vertical grid size
		cam.orthographicSize = ((float)gridSizeY+paddingY) / 2f;
	}
	public static List<Vector2> GetNeighborPositions(Vector2 center){
		List<Vector2> neighbors = new List<Vector2>();
		for (int i = 0; i < 4; i++) {
			neighbors.Add(new Vector2(nDir[i,0]+center.x, nDir[i, 1]+center.y));
		}
		return neighbors;
	}
	public static Vector3 OffsetToVector(Offset p){
		switch (p) {
		case Offset.UPPER_LEFT:
			return new Vector3 (-0.5f, 0.5f, 0);
			break;
		case Offset.CENTER_LEFT:
			return new Vector3 (-0.5f, 0, 0);
			break;
		default: // center
			return Vector3.zero;
			break;
		}
		return Vector3.zero;
	}
}
