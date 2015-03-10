using System.Collections.Generic;
using UnityEngine;

public class SVGRectElement : SVGTransformable, SVGParser.IElementToVector {
  private SVGLength _x;
  private SVGLength _y;
  private SVGLength _width;
  private SVGLength _height;
  private SVGLength _rx;
  private SVGLength _ry;
  //================================================================================
  private AttributeList _attrList;
  //================================================================================
  public SVGLength x {
    get {
      return this._x;
    }
  }

  public SVGLength y {
    get {
      return this._y;
    }
  }

  public SVGLength width {
    get {
      return this._width;
    }
  }

  public SVGLength height {
    get {
      return this._height;
    }
  }


  public SVGLength rx {
    get {
      return this._rx;
    }
  }

  public SVGLength ry {
    get {
      return this._ry;
    }
  }
  //================================================================================
  public SVGRectElement(AttributeList attrList,
              SVGTransformList inheritTransformList) : base(inheritTransformList) {
    this._attrList = attrList;
    this._x = new SVGLength(attrList.GetValue("x"));
    this._y = new SVGLength(attrList.GetValue("y"));
    this._width = new SVGLength(attrList.GetValue("width"));
    this._height = new SVGLength(attrList.GetValue("height"));
    this._rx = new SVGLength(attrList.GetValue("rx"));
    this._ry = new SVGLength(attrList.GetValue("ry"));
  }
	public List<Vector2> GetPoints(){
		List<Vector2> points = new List<Vector2> ();
		points.Add (new Vector2 (x.value, y.value));
		points.Add (new Vector2 (x.value+width.value, y.value));
		points.Add (new Vector2 (x.value+width.value, y.value+height.value));
		points.Add (new Vector2 (x.value, y.value+height.value));
		points.Add (new Vector2 (x.value, y.value));
		return points;
	}
}
