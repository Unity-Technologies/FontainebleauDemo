
//-------------------------------------------------------------------------------------
// Fill SurfaceData/Builtin data function
//-------------------------------------------------------------------------------------

/* plundered from BuiltinData.cs.hlsl at some point, probably not up to date anymore...

		struct BuiltinData
		{
			float opacity;
			float3 bakeDiffuseLighting;
			float3 emissiveColor;
			float emissiveIntensity;
			float2 velocity;
			float2 distortion;
			float distortionBlur;
			float depthOffset;
		};

		struct FragInputs
		{
			// Contain value return by SV_POSITION (That is name positionCS in PackedVarying).
			// xy: unormalized screen position (offset by 0.5), z: device depth, w: depth in view space
			// Note: SV_POSITION is the result of the clip space position provide to the vertex shaders that is transform by the viewport
			float4 unPositionSS; // In case depth offset is use, positionWS.w is equal to depth offset
			float3 positionWS;
			float2 texCoord0;
			float2 texCoord1;
			float2 texCoord2;
			float2 texCoord3;
			float3 tangentToWorld[3];
			float4 color; // vertex color

			// For two sided lighting
			bool isFrontFace;
		};

*/

void GetSurfaceAndBuiltinData(FragInputs input, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
{
    ZERO_INITIALIZE(BuiltinData, builtinData);

	// Translation, scale
	float alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texCoord0.xy).a * input.color.a;

#ifdef _ALPHATEST_ON
    clip(alpha - _AlphaCutoff);
#endif

	/////////////////////////////////////////
	// Surface Data
	surfaceData.color = input.color.rgb;

	/////////////////////////////////////////
    // Builtin Data
	builtinData.opacity = alpha;
}
