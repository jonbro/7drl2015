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

}
