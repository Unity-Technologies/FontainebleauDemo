Shader "Hidden/HDRenderPipeline/Sky/FlowmapSky"
{
	Properties
	{
		_Period("Period", Float) = 15
	}
    HLSLINCLUDE

    #pragma vertex Vert
    #pragma fragment Frag

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonLighting.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

    TEXTURECUBE(_Cubemap);
    SAMPLER(sampler_Cubemap);

    float4   _SkyParam; // x exposure, y multiplier, z rotation
    float4x4 _PixelCoordToViewDirWS; // Actually just 3x3, but Unity can only set 4x4
	float _Period;

    struct Attributes
    {
        uint vertexID : SV_VertexID;
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID, UNITY_RAW_FAR_CLIP_VALUE);
        return output;
    }

	void Unity_Divide_float(float A, float B, out float Out)
	{
		Out = A / B;
	}

	void Unity_Modulo_float(float A, float B, out float Out)
	{
		Out = fmod(A, B);
	}

	void Unity_Multiply_float3(float3 A, float3 B, out float3 Out)
	{
		Out = A * B;
	}

	void Unity_Add_float3(float3 A, float3 B, out float3 Out)
	{
		Out = A + B;
	}

	void Unity_Multiply_float(float A, float B, out float Out)
	{
		Out = A * B;
	}

	void Unity_Add_float(float A, float B, out float Out)
	{
		Out = A + B;
	}

	void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
	{
		Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
	}

	void Unity_Absolute_float(float In, out float Out)
	{
		Out = abs(In);
	}

	void Unity_Saturate_float(float In, out float Out)
	{
		Out = saturate(In);
	}

	void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
	{
		Out = lerp(A, B, T);
	}

	void Unity_DegreesToRadians_float(float In, out float Out)
	{
		Out = radians(In);
	}

	void Unity_Cosine_float(float In, out float Out)
	{
		Out = cos(In);
	}

	void Unity_Sine_float(float In, out float Out)
	{
		Out = sin(In);
	}

	void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA)
	{
		RGBA = float4(R, G, B, A);
	}

	void Unity_DotProduct_float(float3 A, float3 B, out float Out)
	{
		Out = dot(A, B);
	}

    float4 Frag(Varyings input) : SV_Target
    {
		// Points towards the camera
		float3 viewDirWS = normalize(mul(float3(input.positionCS.xy, 1.0), (float3x3)_PixelCoordToViewDirWS));
		// Reverse it to point into the scene
		float3 dir = -viewDirWS;

		float _Divide_64014540_Out;
		Unity_Divide_float(_Time.y, _Period, _Divide_64014540_Out);
		float _Modulo_E0A7B260_Out;
		_Modulo_E0A7B260_Out = frac(_Divide_64014540_Out);
		float _Multiply_D0AC52AC_Out;
		Unity_Multiply_float(_Modulo_E0A7B260_Out, _SkyParam.z, _Multiply_D0AC52AC_Out);
		float _DegreesToRadians_B440AE49_Out;
		Unity_DegreesToRadians_float(_Multiply_D0AC52AC_Out, _DegreesToRadians_B440AE49_Out);
		float _Cosine_300F1031_Out;
		Unity_Cosine_float(_DegreesToRadians_B440AE49_Out, _Cosine_300F1031_Out);
		float _Sine_4DEAB51D_Out;
		Unity_Sine_float(_DegreesToRadians_B440AE49_Out, _Sine_4DEAB51D_Out);
		float _Multiply_42D23D79_Out;
		Unity_Multiply_float(_Sine_4DEAB51D_Out, -1, _Multiply_42D23D79_Out);
		float4 _Combine_BDC82348_RGBA;
		Unity_Combine_float(_Cosine_300F1031_Out, 0, _Multiply_42D23D79_Out, 0, _Combine_BDC82348_RGBA);
		float _DotProduct_750FF31A_Out;
		Unity_DotProduct_float((_Combine_BDC82348_RGBA.xyz), dir, _DotProduct_750FF31A_Out);
		float _Split_5183804D_G = dir.y;
		float4 _Combine_767679EA_RGBA;
		Unity_Combine_float(_Sine_4DEAB51D_Out, 0, _Cosine_300F1031_Out, 0, _Combine_767679EA_RGBA);
		float _DotProduct_45E87791_Out;
		Unity_DotProduct_float((_Combine_767679EA_RGBA.xyz), dir, _DotProduct_45E87791_Out);
		float4 _Combine_BFEDF9AF_RGBA;
		Unity_Combine_float(_DotProduct_750FF31A_Out, _Split_5183804D_G, _DotProduct_45E87791_Out, 0, _Combine_BFEDF9AF_RGBA);
		float3 _SampleCubemap_88512B66_Out = ClampToFloat16Max(SAMPLE_TEXTURECUBE_LOD(_Cubemap, sampler_Cubemap, _Combine_BFEDF9AF_RGBA.xyz, 0).rgb * exp2(_SkyParam.x) * _SkyParam.y);
		float _Multiply_6E04A480_Out;
		Unity_Multiply_float(_Period, 0.5, _Multiply_6E04A480_Out);
		float _Add_6A22050E_Out;
		Unity_Add_float(_Time.y, _Multiply_6E04A480_Out, _Add_6A22050E_Out);
		float _Divide_788C0FAB_Out;
		Unity_Divide_float(_Add_6A22050E_Out, _Period, _Divide_788C0FAB_Out);
		float _Modulo_A2EC3309_Out;
		_Modulo_A2EC3309_Out = frac(_Divide_788C0FAB_Out);
		float _Multiply_E0CC831D_Out;
		Unity_Multiply_float(_Modulo_A2EC3309_Out, _SkyParam.z, _Multiply_E0CC831D_Out);
		float _DegreesToRadians_B5EA090D_Out;
		Unity_DegreesToRadians_float(_Multiply_E0CC831D_Out, _DegreesToRadians_B5EA090D_Out);
		float _Cosine_66112E76_Out;
		Unity_Cosine_float(_DegreesToRadians_B5EA090D_Out, _Cosine_66112E76_Out);
		float _Sine_416FD968_Out;
		Unity_Sine_float(_DegreesToRadians_B5EA090D_Out, _Sine_416FD968_Out);
		float _Multiply_83E18F89_Out;
		Unity_Multiply_float(_Sine_416FD968_Out, -1, _Multiply_83E18F89_Out);
		float4 _Combine_ED2D18C7_RGBA;
		Unity_Combine_float(_Cosine_66112E76_Out, 0, _Multiply_83E18F89_Out, 0, _Combine_ED2D18C7_RGBA);
		float _DotProduct_61F36425_Out;
		Unity_DotProduct_float((_Combine_ED2D18C7_RGBA.xyz), dir, _DotProduct_61F36425_Out);
		float4 _Combine_DBD41F04_RGBA;
		Unity_Combine_float(_Sine_416FD968_Out, 0, _Cosine_66112E76_Out, 0, _Combine_DBD41F04_RGBA);
		float _DotProduct_14E9C03C_Out;
		Unity_DotProduct_float((_Combine_DBD41F04_RGBA.xyz), dir, _DotProduct_14E9C03C_Out);
		float4 _Combine_D3F45736_RGBA;
		Unity_Combine_float(_DotProduct_61F36425_Out, _Split_5183804D_G, _DotProduct_14E9C03C_Out, 0, _Combine_D3F45736_RGBA);
		float3 _SampleCubemap_DF479950_Out = ClampToFloat16Max(SAMPLE_TEXTURECUBE_LOD(_Cubemap, sampler_Cubemap, _Combine_D3F45736_RGBA.xyz, 0).rgb * exp2(_SkyParam.x) * _SkyParam.y);
		float _Remap_4EED814F_Out;
		Unity_Remap_float(_Modulo_E0A7B260_Out, float2 (0, 1), float2 (-1, 1), _Remap_4EED814F_Out);
		float _Absolute_A4D15DCF_Out;
		Unity_Absolute_float(_Remap_4EED814F_Out, _Absolute_A4D15DCF_Out);
		float _Remap_5B4A58E6_Out;
		Unity_Remap_float(_Absolute_A4D15DCF_Out, float2 (0.025, 0.75), float2 (0, 1), _Remap_5B4A58E6_Out);
		float _Saturate_38FDDCE5_Out;
		Unity_Saturate_float(_Remap_5B4A58E6_Out, _Saturate_38FDDCE5_Out);
		float4 _Lerp_DEAA6A6F_Out;
		Unity_Lerp_float4(float4 (_SampleCubemap_88512B66_Out,1), float4 (_SampleCubemap_DF479950_Out,1), (_Saturate_38FDDCE5_Out.xxxx), _Lerp_DEAA6A6F_Out);

		float3 Color = (_Lerp_DEAA6A6F_Out.xyz);

        // Rotate direction
        //float phi = DegToRad(_SkyParam.z);
        //float cosPhi, sinPhi;
        //sincos(phi, sinPhi, cosPhi);
        //float3 rotDirX = float3(cosPhi, 0, -sinPhi);
        //float3 rotDirY = float3(sinPhi, 0, cosPhi);
        //dir = float3(dot(rotDirX, dir), dir.y, dot(rotDirY, dir));

        return float4 (Color,1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
            ENDHLSL

        }

        Pass
        {
            ZWrite Off
            ZTest LEqual
            Blend Off
            Cull Off

            HLSLPROGRAM
            ENDHLSL
        }

    }
    Fallback Off
}
