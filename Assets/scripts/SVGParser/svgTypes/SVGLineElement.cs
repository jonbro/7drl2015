using System.Collections.Generic;
using UnityEngine;

public class SVGLineElement : SVGTransformable,SVGParser.IElementToVector {
  private SVGLength _x1;
  private SVGLength _y1;
  private SVGLength _x2;
  private SVGLength _y2;
  /***********************************************************************************/
  private AttributeList _attrList;
  /***********************************************************************************/
  public SVGLength x1 {
    get {
      return this._x1;
    }
  }

  public SVGLength y1 {
    get {
      return this._y1;
    }
  }

  public SVGLength x2 {
    get {
      return this._x2;
    }
  }

  public SVGLength y2 {
    get {
      return this._y2;
    }
  }
  /***********************************************************************************/
  public SVGLineElement(  AttributeList attrList,
              SVGTransformList inheritTransformList) : base(inheritTransformList) {
    this._attrList = attrList;
    this._x1 = new SVGLength(attrList.GetValue("x1"));
    this._y1 = new SVGLength(attrList.GetValue("y1"));
    this._x2 = new SVGLength(attrList.GetValue("x2"));
    this._y2 = new SVGLength(attrList.GetValue("y2"));
  }
  /***********************************************************************************/
	public List<Vector2> GetPoints(){
		List<Vector2> points = new List<Vector2> ();
		points.Add (new Vector2 (x1.value, y1.value));
		points.Add (new Vector2 (x2.value, y2.value));
		return points;

	}
}
