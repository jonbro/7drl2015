using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Cale/Compression")]
public class Compression : ImageEffectBase {
	RenderTexture  accumTexture;
	public Texture flow;
	public Texture stop;
	public Vector2 offset;
	public float fade = .9f;
	public float phase = 0;
	public float phasePerSecond = 0;
	public Vector2 scrollPerSecond = Vector2.zero;
	Vector2 scroll = Vector2.zero;

	
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
		
		material.SetVector("_x", new Vector4(offset.x,offset.y,phase,fade));
		material.SetVector("_scroll", new Vector4(scroll.x,scroll.y,0,0));
		material.SetTexture("_Last", accumTexture);
		material.SetTexture("_Flow",flow);
		material.SetTexture("_Stop",stop);
		Graphics.Blit (source, accumTexture, material);
		Graphics.Blit(accumTexture,destination);
	}
}
