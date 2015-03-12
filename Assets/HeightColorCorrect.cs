using UnityEngine;
using System.Collections;

public class HeightColorCorrect : MonoBehaviour
{
	public Transform toTrack;
	public float minY = 0;
	public float maxY = 100;
	CCColorCorrection cc;
	// Use this for initialization
	void Start (){
		if(toTrack==null)toTrack = this.transform;
		cc = FindObjectOfType<CCColorCorrection>();
	}

	// Update is called once per frame
	void Update (){
		if(cc==null)return;
		float f = Mathf.Lerp(0,1,Mathf.Clamp01((toTrack.transform.position.y-minY)/(maxY-minY)));
		cc.offset.Set(f,f,f,f);
	}
}

