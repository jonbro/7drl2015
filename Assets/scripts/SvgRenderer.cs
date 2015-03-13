using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SvgRenderer : LineRenderer {
	int currentLine = 0;
	public TextAsset SVGFile = null;
	float scale = 100;
	bool svgLoaded = false;
	List<List<Vector2>> points;
	// Use this for initialization
	void Start () {
		// get single pixel width
		if(!svgLoaded && SVGFile != null)
			LoadSvgFromTextAsset (SVGFile);
	}
	public void LoadSvgFromResources(string svgName){
		TextAsset asset = Resources.Load ("svgs/Generated Assets/" + svgName) as TextAsset;
		if (asset != null) {
			LoadSvgFromTextAsset (asset);
		}
	}
	public void LoadSvgFromTextAsset(TextAsset asset){
		LineThickness = pixelSize*1;
		SVGParser parsedSVG = new SVGParser (asset.text);
		List<object> elementsStack = new List<object> ();
		parsedSVG.GetElementList (elementsStack, new SVGTransformList());
		Vector2 flipVertical = new Vector2 (1, -1);
		List<UIVertex> vbo = new List<UIVertex>();
		vbo.Clear();
		points = new List<List<Vector2>> ();
		foreach (object o in elementsStack) {
			points.Add(((SVGParser.IElementToVector)o).GetPoints ());
			List<Vector2> lastPoints = points[points.Count-1];
			if (lastPoints.Count <= 1)
				continue;
			// flip and scale the points
			for (int i=0;i<lastPoints.Count;i++) {
				lastPoints[i] = lastPoints[i].Mul(flipVertical) * 1 / scale;
			}
		}
		foreach (List<Vector2> subPoints in points) {
			OnFillVBO (vbo, subPoints);
		}
		UpdateMeshWithNewPoints (vbo);
		svgLoaded = true;
	}
	public List<List<Vector2>> GetPoints(){
		List<List<Vector2>> newPoints = new List<List<Vector2>> ();
		foreach (List<Vector2> originalLine in points) {
			newPoints.Add (new List<Vector2> (originalLine));
		}
		return newPoints;
	}

}
