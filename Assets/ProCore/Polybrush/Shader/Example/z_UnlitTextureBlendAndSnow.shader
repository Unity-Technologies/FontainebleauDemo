// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/**
 *	Unlit 4 texture blend with snow channel.
 *	
 *	This example shows how one might pack 4 textures
 *	and a generated parameter into a single channel.
 *
 *	_MainTex serves as the base texture, with Texture1-3
 *	showing based on Color.GBA values.  Another parameter,
 *	`Snow` is blended on top of the textures via the 
 *	Color.R field.
 *
 *	Z_SHADER_METADATA ../Polybrush Metadata/z_UnlitTextureBlendAndSnow
 */

Shader "Polybrush/Example/Unlit Texture Blend with Snow"
{
	Properties
	{
		_MainTex ("Base", 2D) = "white" {}
		_Texture1 ("Texture 1", 2D) = "white" {}
		_Texture2 ("Texture 2", 2D) = "white" {}
		_Texture3 ("Texture 3", 2D) = "white" {}
		_SnowColor ("Snow Color", Color) = (.9, .9, 1, 1)
		_SnowTiling ("Snow Tiling", Range (1,50)) = 2
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
			#include "../Noise.cginc"

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

			fixed4 _SnowColor;
			float _SnowTiling;

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
				fixed4 col1 = tex2D(_MainTex, i.uv);
				fixed4 col2 = tex2D(_Texture1, i.uv) * i.color.y;
				fixed4 col3 = tex2D(_Texture2, i.uv) * i.color.z;
				fixed4 col4 = tex2D(_Texture3, i.uv) * i.color.w;

				float snowMix = i.color.x * ((cnoise(i.uv * _SnowTiling) + 1) * .5);
				fixed4 final_color = lerp(lerp(lerp(col1, col2, i.color.y), col3, i.color.z), col4, i.color.w);
				final_color = lerp(final_color, _SnowColor, snowMix);

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, final_color);

				return final_color;
			}
			ENDCG
		}
	}
}
