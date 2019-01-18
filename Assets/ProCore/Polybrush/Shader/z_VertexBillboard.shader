Shader "Hidden/Polybrush/z_VertexBillboard"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Scale("Scale", Range(1,5)) = 2.0
	}

	SubShader
	{
		Tags { "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="True" }
		Lighting Off
		ZTest LEqual
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass 
		{
			AlphaTest Greater .25

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Scale;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			v2f vert (appdata v)
			{
				v2f o;

				// o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.pos = mul(UNITY_MATRIX_MV, v.vertex);
				o.pos.xyz *= .99;
				o.pos = mul(UNITY_MATRIX_P, o.pos);

				// convert vertex to screen space, add pixel-unit xy to vertex, then transform back to clip space.
				float4 clip = o.pos;

				clip.xy /= clip.w;
				clip.xy = clip.xy * .5 + .5;
				clip.xy *= _ScreenParams.xy;

				clip.xy += v.texcoord1.xy * _Scale;
				clip.z -= (.0001 + v.normal.x) * (1 - UNITY_MATRIX_P[3][3]);

				clip.xy /= _ScreenParams.xy;
				clip.xy = (clip.xy - .5) / .5;
				clip.xy *= clip.w;

				o.pos = clip;
				o.uv = v.texcoord.xy;
				o.color = v.color;

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				return tex2D(_MainTex, i.uv) * i.color;
			}

			ENDCG
		}
	}
}
