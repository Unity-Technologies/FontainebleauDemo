// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/**
 *	This is a simple example of a shader that blends 6 textures and applies a vertex color
 * 	tint and works with Polybrush.
 */

Shader "Polybrush/Example/Simple Texture Blend"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Texture1 ("Texture 1", 2D) = "white" {}
		_Texture2 ("Texture 2", 2D) = "white" {}
		_Texture3 ("Texture 3", 2D) = "white" {}
		_Texture4 ("Texture 4", 2D) = "white" {}
		_Texture5 ("Texture 5", 2D) = "white" {}
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

			// tell Polybrush that this shader supports 6 texture channels
			#define Z_TEXTURE_CHANNELS 6

			// this tells Polybrush that the shader expects data in the uv3 and uv4 channels
			// (4 components in uv3, 2 in uv4)
			#define Z_MESH_ATTRIBUTES UV3 UV4

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 uv2 : TEXCOORD2;
				float4 uv3 : TEXCOORD3;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 uv2 : TEXCOORD1;
				float4 uv3 : TEXCOORD2;
				float4 color : COLOR;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _Texture1;
			sampler2D _Texture2;
			sampler2D _Texture3;
			sampler2D _Texture4;
			sampler2D _Texture5;
			float4 _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				o.uv2 = v.uv2;
				o.uv3 = v.uv3;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col1 = tex2D(_MainTex, i.uv) * i.uv2.x;
				fixed4 col2 = tex2D(_Texture1, i.uv) * i.uv2.y;
				fixed4 col3 = tex2D(_Texture2, i.uv) * i.uv2.z;
				fixed4 col4 = tex2D(_Texture3, i.uv) * i.uv2.w;
				fixed4 col5 = tex2D(_Texture4, i.uv) * i.uv3.x;
				fixed4 col6 = tex2D(_Texture5, i.uv) * i.uv3.y;

				fixed4 col = col1 + col2 + col3 + col4 + col5 + col6;

				// multiply vertex color 
				col *= i.color;

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}
			ENDCG
		}
	}
}
