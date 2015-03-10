using UnityEngine;
using System.Collections.Generic;

public class SVGPolygonElement : SVGTransformable, SVGParser.IElementToVector {
  private List<Vector2> _listPoints;
  //================================================================================
  private AttributeList _attrList;
  //================================================================================
  public List<Vector2> listPoints {
    get { return this._listPoints; }
  }
  //================================================================================
  public SVGPolygonElement(  AttributeList attrList,
                SVGTransformList inheritTransformList) : base(inheritTransformList) {
    this._attrList = attrList;
    this._listPoints = ExtractPoints(this._attrList.GetValue("points"));
  }
  //================================================================================
  private List<Vector2> ExtractPoints(string inputText) {
    List<Vector2> _return = new List<Vector2>();
    string[] _lstStr = SVGStringExtractor.ExtractTransformValue(inputText);

    int len = _lstStr.Length;

    for(int i = 0; i < len -1; i++) {
      string value1, value2;
      value1 = _lstStr[i];
      value2 = _lstStr[i+1];
      SVGLength _length1 = new SVGLength(value1);
      SVGLength _length2 = new SVGLength(value2);
      Vector2 _point = new Vector2(_length1.value, _length2.value);
      _return.Add(_point);
      i++;
    }
    return _return;
  }
	public List<Vector2> GetPoints(){
		List<Vector2> returnPoints = new List<Vector2>(listPoints);
		returnPoints.Add (returnPoints [0]);
		return returnPoints;
	}
}
