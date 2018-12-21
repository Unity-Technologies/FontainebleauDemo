Shader "HDRenderPipeline/Text Render"
{
    Properties
    {
		_MainTex("Font Texture", 2D) = "white" {}

        [ToggleOff]  _AlphaCutoffEnable("Alpha Cutoff Enable", Float) = 0.0
        _AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        // Blending state
        [HideInInspector] _SurfaceType("__surfacetype", Float) = 0.0
        [HideInInspector] _BlendMode("__blendmode", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _CullMode("__cullmode", Float) = 2.0
        [HideInInspector] _ZTestMode("_ZTestMode", Int) = 8
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    //-------------------------------------------------------------------------------------
    // Variant
    //-------------------------------------------------------------------------------------

    #pragma shader_feature _ALPHATEST_ON
    #pragma shader_feature _SOFTPARTICLES_ON
	#pragma shader_feature _CAMERAFADING_ON

    //-------------------------------------------------------------------------------------
    // Include
    //-------------------------------------------------------------------------------------

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

    //-------------------------------------------------------------------------------------
    // variable declaration
    //-------------------------------------------------------------------------------------

    #include "TextProperties.hlsl"

    // All our shaders use same name for entry point
    #pragma vertex Vert
    #pragma fragment Frag

    ENDHLSL

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "ForwardDisplayDebug" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_CullMode]

            HLSLPROGRAM

            #define DEBUG_DISPLAY
            #define SHADERPASS SHADERPASS_FORWARD_UNLIT
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/Unlit.hlsl"
            #include "ShaderPass/TextPass.hlsl"
            #include "TextData.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassForwardUnlit.hlsl"

            ENDHLSL
        }

		// Forward rendering for blended
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "Forward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_CullMode]

            HLSLPROGRAM

            #define SHADERPASS SHADERPASS_FORWARD_UNLIT
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/Unlit.hlsl"
            #include "ShaderPass/TextPass.hlsl"
            #include "TextData.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassForwardUnlit.hlsl"

            ENDHLSL
        }

		// Unlit opaque material need to be render with ForwardOnlyOpaque. Unlike Lit that can be both deferred and forward, 
		// unlit require to be forward only, that's why we need this pass. Unlit transparent will use regular Forward pass
		// (Code is exactly the same as "Forward", it simply allow our system to filter objects correctly)
		Pass
		{
			Name "ForwardUnlit"
			Tags{"LightMode" = "ForwardOnlyOpaque"}

			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			Cull[_CullMode]

			HLSLPROGRAM

			#define SHADERPASS SHADERPASS_FORWARD_UNLIT
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/Unlit.hlsl"
			#include "ShaderPass/TextPass.hlsl"
			#include "TextData.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassForwardUnlit.hlsl"

			ENDHLSL
		}

    }

    CustomEditor "Experimental.Rendering.HDPipeline.TextShaderGUI"
}
