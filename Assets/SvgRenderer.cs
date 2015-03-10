using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SvgRenderer : MonoBehaviour {
	Vector3[] verts;
	Vector2[] uv;
	int[] indices;
	Color32[] cs;
	int currentLine = 0;
	int maxLines = 300;
	private Mesh mesh;
	public Material material;
	public TextAsset SVGFile = null;
	float scale = 100;
	public float LineThickness = 0.01f;
	public Color color;
	void Awake () {
		if (GetComponent<MeshFilter> () == null) {
			gameObject.AddComponent<MeshFilter> ();
		}
		if (GetComponent<MeshRenderer> () == null) {
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer> ();
			meshRenderer.material = material;
		}
		if(mesh == null){
			GetComponent<MeshFilter>().mesh = mesh = new Mesh();
			mesh.hideFlags = HideFlags.HideAndDontSave;
		}
		mesh.Clear();
		// instantiate all the bits that we are going to need to fill out the line mesh
		verts = new Vector3[maxLines*2];
		uv = new Vector2[maxLines*2];
		cs = new Color32[maxLines*2];
		// blank everything so it doesn't go out of bounds
		for(int i=0;i<maxLines*2;i++){
			verts[i] = Vector3.zero;
		}
		indices = new int[maxLines*2];
		// SetupMesh();
		mesh.vertices = verts;
		mesh.SetIndices(indices, MeshTopology.Lines, 0);
	}

	// Use this for initialization
	void Start () {
		// get single pixel width
		float pixelSize = Mathf.Abs(Camera.main.ScreenToWorldPoint (Vector3.zero).x - Camera.main.ScreenToWorldPoint (Vector3.one).x);;
		LineThickness = pixelSize*2;
		SVGParser parsedSVG = new SVGParser (SVGFile.text);
		List<object> elementsStack = new List<object> ();
		parsedSVG.GetElementList (elementsStack, new SVGTransformList());
		Vector2 flipVertical = new Vector2 (1, -1);
		List<UIVertex> vbo = new List<UIVertex>();
		vbo.Clear();
		foreach (object o in elementsStack) {
			List<Vector2> points = ((SVGParser.IElementToVector)o).GetPoints ();
			if (points.Count <= 1)
				continue;
			// flip and scale the points
			for (int i=0;i<points.Count;i++) {
				points[i] = points[i].Mul(flipVertical) * 1 / scale;
			}
			OnFillVBO (vbo, points);
		}
		verts = new Vector3[vbo.Count];
		cs = new Color32[vbo.Count];
		uv = new Vector2[vbo.Count];
		indices = new int[vbo.Count];
		for(int i=0;i<vbo.Count;i++){
			verts [i] = vbo [i].position;
			cs [i] = vbo [i].color;
			uv [i] = vbo [i].uv0;
			indices [i] = i;
		}
		mesh.vertices = verts;
		mesh.colors32 = cs;
		mesh.uv = uv;
		mesh.SetIndices(indices, MeshTopology.Quads, 0);
	}
	void OnFillVBO(List<UIVertex> vbo, List<Vector2> Points = null, float capSize = 0.1f)
	{
		capSize = LineThickness * 0.5f;
		// requires sets of quads
		if (Points == null || Points.Count < 2)
			return;

		// build a new set of points taking into account the cap sizes. 
		// would be cool to support corners too, but that might be a bit tough :)
		var pointList = new List<Vector2> ();
		pointList.Add (Points [0]);
		var capPoint = Points [0] + (Points [1] - Points [0]).normalized * capSize;
		pointList.Add (capPoint);

		// should bail before the last point to add another cap point
		for (int i = 1; i < Points.Count-1; i++)
		{
			pointList.Add (Points [i]);
		}
		capPoint = Points [Points.Count-1] - (Points [Points.Count-1] - Points [Points.Count-2]).normalized * capSize;
		pointList.Add (capPoint);
		pointList.Add (Points [Points.Count - 1]);

		var TempPoints = pointList.ToArray ();

		Vector2 prevV1 = Vector2.zero;
		Vector2 prevV2 = Vector2.zero;

		for (int i = 1; i < TempPoints.Length; i++)
		{
			var prev = TempPoints[i - 1];
			var cur = TempPoints[i];
			prev = new Vector2(prev.x, prev.y);
			cur = new Vector2(cur.x, cur.y);

			float angle = Mathf.Atan2(cur.y - prev.y, cur.x - prev.x) * 180f / Mathf.PI;

			var v1 = prev + new Vector2(0, -LineThickness / 2);
			var v2 = prev + new Vector2(0, +LineThickness / 2);
			var v3 = cur + new Vector2(0, +LineThickness / 2);
			var v4 = cur + new Vector2(0, -LineThickness / 2);

			v1 = RotatePointAroundPivot(v1, prev, new Vector3(0, 0, angle));
			v2 = RotatePointAroundPivot(v2, prev, new Vector3(0, 0, angle));
			v3 = RotatePointAroundPivot(v3, cur, new Vector3(0, 0, angle));
			v4 = RotatePointAroundPivot(v4, cur, new Vector3(0, 0, angle));

			Vector2 uvTopLeft = Vector2.zero;
			Vector2 uvBottomLeft = new Vector2(0, 1);

			Vector2 uvTopCenter = new Vector2(0.5f, 0);
			Vector2 uvBottomCenter = new Vector2(0.5f, 1);

			Vector2 uvTopRight = new Vector2(1, 0);
			Vector2 uvBottomRight = new Vector2(1, 1);

			Vector2[] uvs = new[]{ uvTopCenter,uvBottomCenter,uvBottomCenter,uvTopCenter };

			if (i > 1)
				SetVbo(vbo, new[] { prevV1, prevV2, v1, v2 }, uvs);

			if(i==1)
				uvs = new[]{ uvTopLeft,uvBottomLeft,uvBottomCenter,uvTopCenter };
			else if(i==TempPoints.Length-1)
				uvs = new[]{uvTopCenter,uvBottomCenter, uvBottomRight, uvTopRight };

			SetVbo(vbo, new[] { v1, v2, v3, v4 }, uvs);


			prevV1 = v3;
			prevV2 = v4;
		}
	}

	protected void SetVbo(List<UIVertex> vbo, Vector2[] vertices, Vector2[] uvs)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			var vert = UIVertex.simpleVert;
			vert.color = color;
			vert.position = vertices[i];
			vert.uv0 = uvs [i];
			vbo.Add(vert);
		}
	}
	public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
	{
		Vector3 dir = point - pivot; // get point direction relative to pivot
		dir = Quaternion.Euler(angles)*dir; // rotate it
		point = dir + pivot; // calculate rotated point
		return point; // return it
	}

	// Update is called once per frame
	void Update () {
	
	}
}
