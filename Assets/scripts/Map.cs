using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

// finally getting around to making a reusable mapping system
// just because I write this fucking boilerplate over and over again

public static class Vector2Extensions {
	public static Vector2i ToVector2i (this Vector2 vector2) {
		int[] intVector2 = new int[2];
		for (int i = 0; i < 2; ++i) intVector2[i] = Mathf.RoundToInt(vector2[i]);
		return new Vector2i (intVector2);
	}
}

public struct Vector2i  {
	public int x, y;
	public Vector2i(int _x, int _y){
		x = _x;
		y = _y;
	}
	public Vector2i (int[] xy) {
		x = xy[0];
		y = xy[1];
	}
	public bool Equals(Vector2i v){
		return v.x == x && v.y == y;
	}
	static public Vector2i operator +(Vector2i a, Vector2i v){
		a.x += v.x;
		a.y += v.y;
		return a;
	}
	static public Vector2i operator -(Vector2i a, Vector2i v){
		a.x -= v.x;
		a.y -= v.y;
		return a;
	}
	public float Distance(Vector2i a){
		return Mathf.Sqrt (DistanceSq(a));
	}
	public float DistanceSq(Vector2i a){
		return Mathf.Pow (x - a.x, 2) + Mathf.Pow (y - a.y, 2);
	}
	public int ManhattanDistance(Vector2i a){
		return (int)(Mathf.Abs (x - a.x) + Mathf.Abs (y - a.y));
	}
	public string ToString(){
		return x + " : " + y;
	}
}

namespace RL{
	public class CharacterMap<T>{
		private T[,] map;
		private int offsetX, offsetY;
		public CharacterMap(int x, int y, int _offsetX = 0, int _offsetY = 0){
			map = new T[x,y];
			offsetX = _offsetX;
			offsetY = _offsetY;
		}
		public T this[int indexX, int indexY] 
		{
			get 
			{ 
				if (
					indexX + offsetX < 0 || indexX + offsetX >= map.GetLength (0)
					|| indexY + offsetY < 0 || indexY + offsetY >= map.GetLength (1))
					return default(T);
				return map[indexX+offsetX, indexY+offsetY]; 
			}
			set 
			{ 
				map[indexX+offsetX, indexY+offsetY] = value; 
			}
		}
		public void Clear(){
			for (int x = 0; x < map.GetLength (0); x++) {
				for (int y = 0; y < map.GetLength (1); y++) {
					map [x, y] = default(T);
				}
			}
		}
	}
	public delegate int CostCallback(int x, int y);
	[System.Serializable]
	public class MapCell{
		public int x, y;
		public RL.Objects obj;
	}
	[System.Serializable]
	public class MapData{

		[XmlArray("mapcells")]
		public List<MapCell> mapcells = new List<MapCell>();
		public void BuildFromMap(Map m){
			for (int x = 0; x < m.sx; x++) {
				for (int y = 0; y < m.sy; y++) {
					MapCell mc = new MapCell ();
					mc.x = x;
					mc.y = y;
					mc.obj = m.GetTile (x, y);
					mapcells.Add (mc);
				}
			}
		}
		public void MoveToMap(Map m){
			foreach (MapCell mc in mapcells) {
				m.SetTile (mc.x, mc.y, mc.obj);
			}
		}
	}
	public class Map
	{
		public int sx, sy;
		int offsetX, offsetY;

		RL.Objects[,] layout;


		int[,] openCache;
		public int[,] buffer; // used for floodfills

		public static int[,] nDir = { {0, -1}, {-1, 0}, {1, 0}, {0, 1},
			{-1, -1}, {1, -1},
			{-1, 1}, {1, 1} };

		public static int[,] nDirOrdered = {
			{0, -1}, {-1, 0}, {0, 1}, {1, 0}, // down, left, up, right
			{-1, -1}, {-1, 1}, {1, 1}, {1, -1} // dl, ul, ur, dr
		};
		public Map(){
			// default constructor :?

		}
		public RL.Objects this[int x, int y]
		{
			get
			{
				return GetTile(x, y);
			}
			set
			{
				SetTile(x, y, value);
			}
		}

		public Map (int _sx, int _sy, int _offsetX=0, int _offsetY=0)
		{
			sx = _sx;
			sy = _sy;
			layout = new RL.Objects[sx,sy];
			offsetX = _offsetX;
			offsetY = _offsetY;
			RebuildCache ();
			InitMap();
		}
		public void RebuildCache(){
			buffer = new int[sx,sy];
			openCache = new int[sx, sy];		
		}
		public void InitMap(){
			ClearOpenCache();
			for (int x = 0; x < sx; x++) {
				for (int y = 0; y < sy; y++) {
					if (x == 0 || x == sx - 1 || y == 0 || y == sy - 1) {
						layout [x, y] = Objects.HARD_WALL;
					} else {
						layout [x, y] = Objects.OPEN;
					}
				}
			}
		}
		public void ClearOpenCache(){
			for (int x = 0; x < sx; x++) {
				for (int y = 0; y < sy; y++) {
					openCache [x, y] = -1;
				}
			}
		}
		// for backwards compatibility should autoreturn the type of tile this is
		public RL.Objects GetTile(int x, int y){
			x += offsetX;
			y += offsetY;
			return layout [x, y];
		}
		public void SetTile(int x, int y, RL.Objects t){
			x += offsetX;
			y += offsetY;
			layout [x, y] = t;
			openCache [x, y] = -1;
		}
		public bool IsValidTile(int x, int y, bool useoffset = true){
			if (useoffset) {
				x += offsetX;
				y += offsetY;
			}
			if (x < 0 || y < 0 || y >= sy || x >= sx)
				return false;
			return true;		
		}
		public void ReplaceAt(int x, int y, RL.Objects target, RL.Objects replacement, bool offset = true){
			if (offset) {
				x += offsetX;
				y += offsetY;
			}
			if ((layout [x, y] & target) > 0) {
				layout [x, y] |= replacement;
				layout [x, y] ^= target;
			}
			ClearOpenCache ();
		}
		public void Replace(RL.Objects target, RL.Objects replacement){
			for (int x = 0; x < sx; x++) {
				for (int y = 0; y < sy; y++) {
					ReplaceAt (x, y, target, replacement, false);
				}
			}
		}
		public void Remove(RL.Objects a){
			for (int x = 0; x < sx; x++) {
				for (int y = 0; y < sy; y++) {
					layout [x, y] ^= a;
				}
			}
			ClearOpenCache ();
		}
		public int CountType(RL.Objects t){
			int ret = 0;
			for (int x = 0; x < sx; x++) {
				for (int y = 0; y < sy; y++) {
					if ((layout [x, y] & t) > 0)
						ret++;
				}
			}
			return ret;
		}
		public bool IsOpenTile(int x, int y){
			x += offsetX;
			y += offsetY;
			// use the open tile cache
//			Debug.Log (x + " " + y);
			if (openCache.GetLength (0) <= x || openCache.GetLength (1) <= y || x < 0 || y < 0)
				return false;
			if (openCache [x, y] < 0) {
				bool open = IsValidTile (x, y, false) && (layout [x, y] & RL.Objects.CANTWALK) <= 0;
				openCache [x, y] = (open ? 0 : 1);
			}
			return openCache [x, y] == 0 ? true : false;
		}
		public Vector2i GetPath(int startX, int startY, int endX, int endY){
			startX += offsetX;
			startY += offsetY;
			endX += offsetX;
			endY += offsetY;
			return GetPath(startX, startY, endX, endY, ((x, y) => { return 1; }));
		}

		// maybe should just return the next position
		// this method is super slow, need to rebuild so it isn't
		public Vector2i GetPath(int startX, int startY, int endX, int endY, CostCallback costFn){
			clearBuffer();
			floodHeight(endX, endY, 1, ref buffer, costFn);
			int lowestNeighbor = -1;
			int lowestNeighborDir = 0;
			for(int i=0;i<4;i++){
				int tx = startX+nDir[i,0];
				int ty = startY+nDir[i,1];
				if(tx>=0&&tx<buffer.GetLength(0)&&ty>=0&&ty<buffer.GetLength(1) &&
					IsOpenTile(tx, ty) &&
					//!ContainsSnake(tx, ty) && // need to have a delegate for calculating tile movement costs, perhaps pass into the flood height as well
					(lowestNeighbor == -1 || lowestNeighbor > buffer[tx, ty])
				){
					lowestNeighbor = buffer[tx, ty];
					lowestNeighborDir = i;
				}
			}
			if (lowestNeighbor == -1) {
				// return out of map for invalid tile
				new Vector2i(new int[]{-1, -1});
			}
			return new Vector2i (startX + nDir [lowestNeighborDir, 0], startY + nDir [lowestNeighborDir, 1]);
		}
		// an a* pathfinder
		public void Pathfind(Vector2i start, Vector2i end){
		
		}
		public void floodHeight(int x, int y, int marker, ref int[,] buffer, CostCallback fn, RL.Objects allowedFill = RL.Objects.OPEN){
			// make sure to only visit each cell once
			buffer[x,y] = marker;		
			for(int i=0;i<4;i++){
				int tx = x+nDir[i,0];
				int ty = y+nDir[i,1];
				if(tx>=0&&tx<buffer.GetLength(0)&&ty>=0&&ty<buffer.GetLength(1)
					&& (layout[tx,ty] & allowedFill) > 0
					&& (buffer[tx, ty]==0 || (marker+fn(tx, ty)) < buffer[tx,ty]) )
				{
					floodHeight(tx, ty, marker+fn(tx, ty), ref buffer, fn);
				}
			}
		}
		public void clearBuffer(){
			for(int x=0;x<sx;x++){
				for(int y=0;y<sy;y++){
					buffer[x,y] = 0;
				}
			}
		}
		static List<Vector2i> points = new List<Vector2i>();
		public static Vector2i[] Line(Vector2i s, Vector2i e)
		{
			int x0 = s.x;
			int y0 = s.y;
			int x1 = e.x;
			int y1 = e.y;

			points.Clear();
			bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
			bool reverse = false;
			if (steep) {
				int tx = x0;
				x0 = y0;
				y0 = tx;

				tx = x1;
				x1 = y1;
				y1 = tx;

			}
			if (x0 > x1) {
				int tx = x1; x1 = x0; x0 = tx;
				tx = y1; y1 = y0; y0 = tx;
				reverse = true;
			}

			int dX = (x1 - x0);
			int dY = Mathf.Abs(y1 - y0);
			int err = (dX / 2);
			int ystep = (y0 < y1 ? 1 : -1);
			int y = y0;

			for (int x = x0; x <= x1; ++x)
			{
				if(steep){
					points.Add(new Vector2i(y,x));
				}else{
					points.Add(new Vector2i(x,y));
				}
				err = err - dY;
				if (err < 0) { y += ystep;  err += dX; }
			}
			if(reverse){
				points.Reverse();
			}
			Vector2i[] _points = new Vector2i[points.Count];
			points.CopyTo(_points);
			return _points;
		}

	}
}