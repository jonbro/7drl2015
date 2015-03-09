
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RL
{
	// use to index the actual objects

	[System.Flags]
	[Serializable]
	public enum Objects : long
	{
		EMPTY = 0,

		OPEN 							= 1 << 0,
		WALL							= 1 << 1,
		HARD_WALL						= 1 << 1,
		ENTRANCE						= 1 << 3,
		EXIT							= 1 << 4,


		CANTWALK						= (WALL | HARD_WALL)
	}
	public static class ObjectExtensions{
		static BitArray[] nouns;

		static ObjectExtensions(){
			nouns = new BitArray[Enum.GetValues(typeof(RL.Objects)).Length];
			BitArray initialArray = new BitArray(Enum.GetValues(typeof(Objects)).Length);

			// setup all the initial bit arrays
			float startTime = Time.time;

			for ( int i = 0 ; i < Enum.GetValues(typeof(RL.Objects)).Length ; i++ ) {
				initialArray.Set(i,true);
				nouns[i] = new BitArray(initialArray);
				initialArray.Set(i,false);
			}

		}
		public static void Init(){

		}
		public static void SetComposition(this Objects compositeIndex, BitArray composite){
			nouns [(int)compositeIndex] = composite;
		}
		public static BitArray ToBitArray(this Objects obj){
			return (BitArray)nouns [(int)obj].Clone();
		}
	}
}

