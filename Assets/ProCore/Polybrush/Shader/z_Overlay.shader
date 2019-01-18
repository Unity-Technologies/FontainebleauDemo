Shader "Hidden/Polybrush/Overlay" 
{
	Properties
	{
		_Alpha ("Alpha", Range(0,1)) = .5
	}

	SubShader
	{
		Tags { "IgnoreProjector"="True" "RenderType"="Transparent" }
		Lighting Off
		ZTest LEqual
		ZWrite Off
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			AlphaTest Greater .25

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float _Alpha;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
			};

			v2f vert (appdata v)
			{
				v2f o;

				/// https://www.opengl.org/discussion_boards/showthread.php/166719-Clean-Wireframe-Over-Solid-Mesh
				o.pos = mul(UNITY_MATRIX_MV, v.vertex);
				o.pos.xyz *= .99;
				o.pos = mul(UNITY_MATRIX_P, o.pos);
				o.color = v.color;

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				return fixed4(i.color.rgb, i.color.a * _Alpha);
			}

			ENDCG
		}
	}
}
