using UnityEngine;
using System.Collections;

public class GridSprite : DisplayElement {
	public SpriteRenderer spriteR;
	public static GridSprite Create(int x, int y, Sprite _sprite, Grid.Offset _offset = Grid.Offset.CENTER){
		GameObject go = (GameObject)(Instantiate(Resources.Load("GridSprite") as GameObject, Grid.GridToWorld(x,y)+Grid.OffsetToVector(_offset), Quaternion.identity));
		go.name = _sprite.name;
		GridSprite gs = go.GetComponent<GridSprite> ();
		gs.spriteR.sprite = _sprite;
		return gs;
	}
	void Awake(){
		spriteR = GetComponent<SpriteRenderer> ();
	}
}
