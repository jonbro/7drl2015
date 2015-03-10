Shader "Cale/Compression" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Last ("last frame", 2D) = "white" {}
	_Flow ("flow control", 2D) = "white" {}
	_Stop ("stop control", 2D) = "white" {}
	_x("x-SampleX y-SampleY z-strength w-null",Vector) = (0,0,.1,0)
	_scroll("x-ScrollX y-ScrollY z-null w-null",Vector) = (0,0,0,0)
}

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
				
CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest 
#pragma target 3.0
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform sampler2D _Last;
uniform sampler2D _Flow;
uniform sampler2D _Stop;
uniform float4 _x;
uniform float4 _scroll;
uniform float4 _center;

fixed4 frag (v2f_img i) : COLOR
{
	float2 uv = i.uv;
	float4 c = tex2D(_MainTex,uv);
	uv.y = 1.0-uv.y;
	float4 f = tex2D(_Flow,uv+_scroll.xy);
	float4 stop = tex2D(_Stop,uv);
	float angle = atan2(uv.y-_center.y,uv.x-_center.x);
	uv.x -= cos(angle)*f.r*_x.x*stop.x;
	uv.y -= sin(angle)*f.r*_x.y*stop.y;
	float4 s = tex2D(_Last,uv);
	return max(c,lerp(c,s,_x.w));
}
ENDCG

	}
}

Fallback off

}