// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/**
 *	Z_SHADER_METADATA ../Polybrush Metadata/z_UnlitTextureBlend_WithMetaData
 */

Shader "Polybrush/Example/Unlit 5 Texture Blend"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Texture1 ("Texture 1", 2D) = "white" {}
		_Texture2 ("Texture 2", 2D) = "white" {}
		_Texture3 ("Texture 3", 2D) = "white" {}
		_Texture4 ("Texture 4", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _Texture1;
			sampler2D _Texture2;
			sampler2D _Texture3;
			sampler2D _Texture4;
			float4 _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 base = tex2D(_MainTex, i.uv);
				fixed4 col1 = tex2D(_Texture1, i.uv);
				fixed4 col2 = tex2D(_Texture2, i.uv);
				fixed4 col3 = tex2D(_Texture3, i.uv);
				fixed4 col4 = tex2D(_Texture4, i.uv);

				fixed4 col = lerp(lerp(lerp(lerp(base, col1, i.color.r), col2, i.color.g), col3, i.color.b), col4, i.color.a);

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}
			ENDCG
		}
	}
}
