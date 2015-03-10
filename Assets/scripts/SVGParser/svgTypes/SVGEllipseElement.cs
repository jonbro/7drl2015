
public class SVGEllipseElement : SVGTransformable {
  private SVGLength _cx;
  private SVGLength _cy;
  private SVGLength _rx;
  private SVGLength _ry;
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
  public SVGEllipseElement(AttributeList attrList,
              SVGTransformList inheritTransformList) : base(inheritTransformList) {
    this._attrList = attrList;
    this._cx = new SVGLength(attrList.GetValue("cx"));
    this._cy = new SVGLength(attrList.GetValue("cy"));
    this._rx = new SVGLength(attrList.GetValue("rx"));
    this._ry = new SVGLength(attrList.GetValue("ry"));
    this.currentTransformList = new SVGTransformList(attrList.GetValue("transform"));
  }
}
