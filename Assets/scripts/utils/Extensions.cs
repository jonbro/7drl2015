using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Extensions {
	public static List<T> Shuffle<T> (this List<T> Input)
	{
		// shuffle the list of directions
		List<T> ArrayStart = new List<T> ();
		List<T> ArrayEnd = new List<T> ();
		for (int i = 0; i < Input.Count; i++) {
			ArrayStart.Add (Input [i]);
		}
		while (ArrayStart.Count > 0) {
			int counter = UnityEngine.Random.Range (0, ArrayStart.Count);
			ArrayEnd.Add (ArrayStart [counter]);
			ArrayStart.RemoveAt (counter);
		}
		return ArrayEnd;
	}
	#region Vector2 extensions
	public static Vector2 Rotate(this Vector2 v, float a){
		float px = v.x*Mathf.Cos(a) - v.y*Mathf.Sin(a);
		float py = v.x*Mathf.Sin(a) + v.y*Mathf.Cos(a);
		return new Vector2(px, py);
	}
	public static float Angle(this Vector2 v){
		return Mathf.Atan2(v.x, v.y)*180.0f/Mathf.PI;
	}
	public static Vector2 Variation(this Vector2 v, float amount){
		return new Vector2(v.x+(Random.value-0.5f)*amount, v.y+(Random.value-0.5f)*amount);	
	}
	public static Vector3 ToVector3(this Vector2 v, float z = 0){
		return new Vector3 (v.x, v.y, z);
	}
	public static Vector2 Mul(this Vector2 v, Vector2 mul){
		return new Vector2 (v.x * mul.x, v.y * mul.y);
	}
	#endregion

	#region Vector3 extensions
	public static Vector3 Variation(this Vector3 v, Vector3 amount){
		return new Vector3(v.x+(Random.value*amount.x)-amount.x*0.5f, v.y+(Random.value*amount.y)-amount.y*0.5f, v.z+(Random.value-0.5f)*amount.z);
	}
	public static Vector3 Variation(this Vector3 v, float amount){
		return new Vector3(v.x+(Random.value-0.5f)*amount, v.y+(Random.value-0.5f)*amount, v.z+(Random.value-0.5f)*amount);
	}
	public static Vector3 Mul(this Vector3 v, Vector3 mul){
		return new Vector3 (v.x * mul.x, v.y * mul.y, v.z * mul.z);
	}
	#endregion
	#region Color extensions
	public static Color Alpha(this Color c, float newAlpha){
		return new Color(c.r, c.g, c.b, newAlpha);
	}
	#endregion

}
