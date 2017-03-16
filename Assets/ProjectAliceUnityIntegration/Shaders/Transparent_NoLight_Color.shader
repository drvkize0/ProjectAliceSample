// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// litilei 2015.4.12
// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Noitom_Mobile/Unlit_Transparent_NoLight_Color" {
Properties {
	_MixedColor ("Mixed Color", Color) = (1.0,1.0,1.0,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha  // Alpha blending
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma exclude_path:prepass noforwardadd
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _MixedColor;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				// o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.texcoord = v.texcoord;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                fixed4 texCol = tex2D(_MainTex, i.texcoord);
               // texCol.a = 1;
				fixed4 col = texCol * _MixedColor;
				return col;
			}
		ENDCG
	}
}

}
