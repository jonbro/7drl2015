using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/CampCult/Color Correction")]
public class CCColorCorrection : ImageEffectBase {
	public Texture  textureRamp;
	public Vector4 offset;

	// Called by camera to apply image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination) {
		material.SetTexture ("_RampTex", textureRamp);
		material.SetVector ("_Off", offset);
		Graphics.Blit (source, destination, material);
	}
}
