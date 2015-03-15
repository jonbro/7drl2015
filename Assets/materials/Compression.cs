using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Cale/Compression")]
public class Compression : ImageEffectBase {
	static Compression instance;

	RenderTexture  accumTexture;
	public Texture flow;
	public Texture stop;
	public Vector2 offset;
	public float fade = .9f;
	public float phase = 0;
	public float phasePerSecond = 0;
	public Vector2 scrollPerSecond = Vector2.zero;
	Vector2 scroll = Vector2.zero;
	public Transform center;

	void Awake(){
		instance = this;
	}
	public static void PopBlur(Transform target, float amount, float time, float minTarget = 0.5f){
		//instance.center.transform.localPosition = Vector3.zero;
		instance.StartCoroutine(instance.PopBlurCoro(amount, time, minTarget, target));
	}
	IEnumerator PopBlurCoro(float amount, float time, float minTarget, Transform targetCenter){
		float startTime = Time.time;
		minTarget = Mathf.Max (0.7f, minTarget);
		while (Time.time - startTime < time) {
			float percent = (Time.time - startTime)/time;
			fade = Mathf.Lerp (amount, minTarget, percent);
			yield return new WaitForEndOfFrame ();
		}
		if(targetCenter != null)
			center.transform.position = targetCenter.position;
		fade = 0.7f;
	}
	// Called by camera to apply image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination) {
		scroll.x+=scrollPerSecond.x*Time.deltaTime;
		scroll.y+=scrollPerSecond.y*Time.deltaTime;
		if (accumTexture == null || accumTexture.width != source.width || accumTexture.height != source.height)
		{
			DestroyImmediate(accumTexture);
			accumTexture = new RenderTexture(source.width, source.height, 0);
			accumTexture.hideFlags = HideFlags.HideAndDontSave;
			Graphics.Blit( source, accumTexture );
		}
		accumTexture.MarkRestoreExpected();
		phase+=phasePerSecond*Time.deltaTime;

		Vector3 p = new Vector3(center.position.x,center.position.y,center.position.z);
		p = camera.WorldToViewportPoint(p);
		material.SetVector("_center",new Vector4(p.x,p.y,p.z,0.0f));

		material.SetVector("_x", new Vector4(offset.x,offset.y,phase,fade));
		material.SetVector("_scroll", new Vector4(scroll.x,scroll.y,0,0));
		material.SetTexture("_Last", accumTexture);
		material.SetTexture("_Flow",flow);
		material.SetTexture("_Stop",stop);
		Graphics.Blit (source, accumTexture, material);
		Graphics.Blit(accumTexture,destination);
	}
}
