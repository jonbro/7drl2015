Shader "Custom/rlGrid" {
	Properties {
		_Color("Color",Color) = (1.0,1.0,1.0,1.0)
		_Background("Background",Color) = (0.0,0.0,0.0,0.0)
		_Shape("Shape",Vector) = (0.0,0.0,0.0,0.0)
	}
	SubShader {
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater 0.5
		Pass{
			Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
			LOD 200
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members screen)
#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma target 3.0
			#include "UnityCG.cginc"
			
			uniform float4 _Shape;
			uniform float4 _Color;
			uniform float4 _Background;
			
			struct appdata {
				float4 vertex : POSITION;
			};
			struct v2f {
				float4 vertex : POSITION;
				float4 screen;
			};
			v2f vert(appdata v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.screen = o.vertex;
				return o;
			};
			

			fixed4 frag (v2f i) : COLOR{
				float4 p = i.screen;//mul (UNITY_MATRIX_MVP, i.screen);
				p.xy /= p.w;
				p.xy = 0.5*(p.xy+1.0) * _ScreenParams.xy;
				float2 uv = p.xy;
				float f = step(abs(mod(uv.x,_Shape.x)-_Shape.x*.5),_Shape.y);
				f = max(f,step(abs(mod(uv.y,_Shape.x)-_Shape.x*.5),_Shape.y));	
				uv+=_Shape.x*.5;
				f*=1.0- step(abs(mod(uv.x,_Shape.x)-_Shape.x*.5),_Shape.z);
				f*=1.0- step(abs(mod(uv.y,_Shape.x)-_Shape.x*.5),_Shape.z);
				return mix(_Background, _Color, f);
			};

			ENDCG
		} 
	}
	FallBack "Diffuse"
}
