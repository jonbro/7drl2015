using UnityEngine;
using System.Collections;

public class CCC : MonoBehaviour {

	public static CCC instance;

	public Texture[] textures;
	int toShow;
	float intensity = 0;
	CCColorCorrection c;

	public static void SetTexture(int i,float attack=.4f, float sustain = 1.0f, float release=0.5f  ){
		if(i>=instance.textures.Length)return;
		if(instance.intensity==0){
			instance.c.textureRamp = instance.textures[i];
			instance.StartCoroutine(instance.ChangeColorCoro(0.2f,0.5f,0.5f));
		}
	}
	IEnumerator ChangeColorCoro(float attack, float sustain, float release ){
		float startTime = Time.time;
		while (Time.time - startTime < attack) {
			float percent = (Time.time - startTime)/attack;
			intensity = percent;
			yield return new WaitForEndOfFrame ();
		}
		intensity = 1;
		startTime = Time.time;
		while (Time.time - startTime < sustain) {
			yield return new WaitForEndOfFrame ();
		}
		startTime = Time.time;
		while (Time.time - startTime < release) {
			float percent = (Time.time - startTime)/release;
			intensity = 1.0f-percent;
			yield return new WaitForEndOfFrame ();
		}
		intensity = 0;
	}

	// Use this for initialization
	void Start () {
		instance = this;
		c = FindObjectOfType<CCColorCorrection>();
	}
	
	// Update is called once per frame
	void Update () {
		c.offset.Set(intensity,intensity,intensity,intensity);

		/*if(Input.GetKeyDown(KeyCode.O)){
			SetTexture(Mathf.FloorToInt(Random.value*textures.Length));
		}*/
	}
}
