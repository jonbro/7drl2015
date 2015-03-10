using System.Collections.Generic;
using UnityEngine;

public class SVGCircleElement : SVGTransformable, SVGParser.IElementToVector {
  private SVGLength _cx;
  private SVGLength _cy;
  private SVGLength _r;
  //================================================================================
  private AttributeList _attrList;
  //================================================================================
  public SVGLength cx {
    get {
      return this._cx;
    }
  }

  public SVGLength cy {
    get {
      return this._cy;
    }
  }

  public SVGLength r {
    get {
      return this._r;
    }
  }
  //================================================================================
	public SVGCircleElement(AttributeList attrList,
		SVGTransformList inheritTransformList) : base(inheritTransformList) {
    this._attrList = attrList;
    this._cx = new SVGLength(attrList.GetValue("cx"));
    this._cy = new SVGLength(attrList.GetValue("cy"));
    this._r = new SVGLength(attrList.GetValue("r"));
  }
	public List<Vector2> GetPoints(){
		List<Vector2> points = new List<Vector2> ();
		int circleDensity = 30;
		for(int i=0;i<circleDensity+1;i++){
			float normalX = Mathf.Cos(i/(float)circleDensity*Mathf.PI*2);
			float normalY = Mathf.Sin(i/(float)circleDensity*Mathf.PI*2);
			points.Add (new Vector2 (cx.value+normalX*r.value, cy.value+normalY*r.value));
		}
		return points;
	}
}
