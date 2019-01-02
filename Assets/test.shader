Shader "Fabric Master"
{
    Properties
    {
        _uvBaseMask("Base UV Mask", Vector) = (1,0,0,0)
_uvBaseST("Base UV Scale Transform", Vector) = (1,1,0,0)
[NoScaleOffset] _BaseColorMap("Base Color Map", 2D) = "white" {}
_BaseColor("Base Color", Color) = (0.7294118,0.7294118,0.7294118,0)
[NoScaleOffset] _MaskMap("Mask Map T(R) AO(G) SSS(B)", 2D) = "white" {}
_SmoothnessMin("Smoothness Min", Range(0, 1)) = 0
_SmoothnessMax("Smoothness Max", Range(0, 1)) = 1
[NoScaleOffset] _SpecColorMap("Specular Color Map S(A)", 2D) = "white" {}
_SpecularColor("Specular Color Multiplier", Color) = (0.2,0.2,0.2,0)
[NoScaleOffset] _NormalMap("Normal Map", 2D) = "bump" {}
_NormalMapStrength("Normal Map Strength", Range(0, 8)) = 1
[Toggle] _useThreadMap("use Thread Map", Float) = 1
[NoScaleOffset] _ThreadMap("Thread Map", 2D) = "grey" {}
_uvThreadMask("Thread UV Mask", Vector) = (1,0,0,0)
_uvThreadST("Thread UV Scale Transform", Vector) = (1,1,0,0)
_ThreadAOStrength01("Thread AO Strength", Range(0, 1)) = 0,5
_ThreadNormalStrength("Thread Normal Strength", Range(0, 2)) = 0,5
_ThreadSmoothnessScale("Thread Smoothness Scale", Range(0, 1)) = 0,5
[NoScaleOffset] _FuzzMap("Fuzz Map", 2D) = "black" {}
_FuzzMapUVScale("Fuzz Map UV scale", Float) = 0,1
_FuzzStrength("Fuzz Strength", Range(0, 2)) = 1
[HideInInspector] _EmissionColor("Color", Color) = (1,1,1,1)

    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="HDRenderPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry+0"
        }
        
        Pass
        {
            // based on FabricPass.template
            Name "META"
            Tags { "LightMode" = "Meta" }
        
            //-------------------------------------------------------------------------------------
            // Render Modes (Blend, Cull, ZTest, Stencil, etc)
            //-------------------------------------------------------------------------------------
                Blend One Zero
        
                Cull Off
        
                ZTest LEqual
        
                ZWrite On
        
                ZClip [_ZClip]
        
                // Default Stencil
        
                
            //-------------------------------------------------------------------------------------
            // End Render Modes
            //-------------------------------------------------------------------------------------
        
            HLSLPROGRAM
        
                #pragma target 4.5
                #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
                //#pragma enable_d3d11_debug_symbols
        
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
        
                #pragma multi_compile _ LOD_FADE_CROSSFADE
        
            //-------------------------------------------------------------------------------------
            // Variant Definitions (active field translations to HDRP defines)
            //-------------------------------------------------------------------------------------
                #define _MATERIAL_FEATURE_COTTON_WOOL 1
                #define _EMISSION 1
                #define _SPECULAR_OCCLUSION_FROM_AO 1
                #define _ENERGY_CONSERVING_SPECULAR 1
                #define _DISABLE_SSR 1
            //-------------------------------------------------------------------------------------
            // End Variant Definitions
            //-------------------------------------------------------------------------------------
        
            #pragma vertex Vert
            #pragma fragment Frag
        
            #define UNITY_MATERIAL_FABRIC      // Need to be define before including Material.hlsl
        
            // This will be enabled in an upcoming change. 
            // #define SURFACE_GRADIENT
        
            // If we use subsurface scattering, enable output split lighting (for forward pass)
            #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
            #define OUTPUT_SPLIT_LIGHTING
            #endif
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Wind.hlsl"
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
        
            // define FragInputs structure
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
        
            //-------------------------------------------------------------------------------------
            // Defines
            //-------------------------------------------------------------------------------------
                #define SHADERPASS SHADERPASS_LIGHT_TRANSPORT
        
                // this translates the new dependency tracker into the old preprocessor definitions for the existing HDRP shader code
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_TEXCOORD2
                #define ATTRIBUTES_NEED_TEXCOORD3
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_TEXCOORD2
                #define VARYINGS_NEED_TEXCOORD3
        
            //-------------------------------------------------------------------------------------
            // End Defines
            //-------------------------------------------------------------------------------------
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #ifdef DEBUG_DISPLAY
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif
        
            #if (SHADERPASS == SHADERPASS_FORWARD)
                // used for shaders that want to do lighting (and materials)
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
            #else
                // used for shaders that don't need lighting
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #endif
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
        
            //Used by SceneSelectionPass
            int _ObjectId;
            int _PassValue;
        
            // this function assumes the bitangent flip is encoded in tangentWS.w
            // TODO: move this function to HDRP shared file, once we merge with HDRP repo
            float3x3 BuildWorldToTangent(float4 tangentWS, float3 normalWS)
            {
                // tangentWS must not be normalized (mikkts requirement)
        
                // Normalize normalWS vector but keep the renormFactor to apply it to bitangent and tangent
        	    float3 unnormalizedNormalWS = normalWS;
                float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
                // bitangent on the fly option in xnormal to reduce vertex shader outputs.
        	    // this is the mikktspace transformation (must use unnormalized attributes)
                float3x3 worldToTangent = CreateWorldToTangent(unnormalizedNormalWS, tangentWS.xyz, tangentWS.w > 0.0 ? 1.0 : -1.0);
        
        	    // surface gradient based formulation requires a unit length initial normal. We can maintain compliance with mikkts
        	    // by uniformly scaling all 3 vectors since normalization of the perturbed normal will cancel it.
                worldToTangent[0] = worldToTangent[0] * renormFactor;
                worldToTangent[1] = worldToTangent[1] * renormFactor;
                worldToTangent[2] = worldToTangent[2] * renormFactor;		// normalizes the interpolated vertex normal
                return worldToTangent;
            }
        
            //-------------------------------------------------------------------------------------
            // Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
        // Generated Type: AttributesMesh
        struct AttributesMesh {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL; // optional
            float4 tangentOS : TANGENT; // optional
            float4 uv0 : TEXCOORD0; // optional
            float4 uv1 : TEXCOORD1; // optional
            float4 uv2 : TEXCOORD2; // optional
            float4 uv3 : TEXCOORD3; // optional
            float4 color : COLOR; // optional
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        
        // Generated Type: VaryingsMeshToPS
        struct VaryingsMeshToPS {
            float4 positionCS : SV_Position;
            float4 texCoord0; // optional
            float4 texCoord1; // optional
            float4 texCoord2; // optional
            float4 texCoord3; // optional
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        struct PackedVaryingsMeshToPS {
            float4 interp00 : TEXCOORD0; // auto-packed
            float4 interp01 : TEXCOORD1; // auto-packed
            float4 interp02 : TEXCOORD2; // auto-packed
            float4 interp03 : TEXCOORD3; // auto-packed
            float4 positionCS : SV_Position; // unpacked
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC; // unpacked
            #endif // INSTANCING_ON
        };
        PackedVaryingsMeshToPS PackVaryingsMeshToPS(VaryingsMeshToPS input)
        {
            PackedVaryingsMeshToPS output;
            output.positionCS = input.positionCS;
            output.interp00.xyzw = input.texCoord0;
            output.interp01.xyzw = input.texCoord1;
            output.interp02.xyzw = input.texCoord2;
            output.interp03.xyzw = input.texCoord3;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        VaryingsMeshToPS UnpackVaryingsMeshToPS(PackedVaryingsMeshToPS input)
        {
            VaryingsMeshToPS output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp00.xyzw;
            output.texCoord1 = input.interp01.xyzw;
            output.texCoord2 = input.interp02.xyzw;
            output.texCoord3 = input.interp03.xyzw;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        
        // Generated Type: VaryingsMeshToDS
        struct VaryingsMeshToDS {
            float3 positionRWS;
            float3 normalWS;
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        struct PackedVaryingsMeshToDS {
            float3 interp00 : TEXCOORD0; // auto-packed
            float3 interp01 : TEXCOORD1; // auto-packed
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC; // unpacked
            #endif // INSTANCING_ON
        };
        PackedVaryingsMeshToDS PackVaryingsMeshToDS(VaryingsMeshToDS input)
        {
            PackedVaryingsMeshToDS output;
            output.interp00.xyz = input.positionRWS;
            output.interp01.xyz = input.normalWS;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        VaryingsMeshToDS UnpackVaryingsMeshToDS(PackedVaryingsMeshToDS input)
        {
            VaryingsMeshToDS output;
            output.positionRWS = input.interp00.xyz;
            output.normalWS = input.interp01.xyz;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        
            //-------------------------------------------------------------------------------------
            // End Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
        
            //-------------------------------------------------------------------------------------
            // Graph generated code
            //-------------------------------------------------------------------------------------
                // Shared Graph Properties (uniform inputs)
                    CBUFFER_START(UnityPerMaterial)
                    float4 _uvBaseMask;
                    float4 _uvBaseST;
                    float4 _BaseColor;
                    float _SmoothnessMin;
                    float _SmoothnessMax;
                    float4 _SpecularColor;
                    float _NormalMapStrength;
                    float _useThreadMap;
                    float4 _uvThreadMask;
                    float4 _uvThreadST;
                    float _ThreadAOStrength01;
                    float _ThreadNormalStrength;
                    float _ThreadSmoothnessScale;
                    float _FuzzMapUVScale;
                    float _FuzzStrength;
                    float4 _EmissionColor;
                    CBUFFER_END
                
                    TEXTURE2D(_BaseColorMap); SAMPLER(sampler_BaseColorMap); float4 _BaseColorMap_TexelSize;
                    TEXTURE2D(_MaskMap); SAMPLER(sampler_MaskMap); float4 _MaskMap_TexelSize;
                    TEXTURE2D(_SpecColorMap); SAMPLER(sampler_SpecColorMap); float4 _SpecColorMap_TexelSize;
                    TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap); float4 _NormalMap_TexelSize;
                    TEXTURE2D(_ThreadMap); SAMPLER(sampler_ThreadMap); float4 _ThreadMap_TexelSize;
                    TEXTURE2D(_FuzzMap); SAMPLER(sampler_FuzzMap); float4 _FuzzMap_TexelSize;
                
                // Pixel Graph Inputs
                    struct SurfaceDescriptionInputs {
                        float4 uv0; // optional
                        float4 uv1; // optional
                        float4 uv2; // optional
                        float4 uv3; // optional
                    };
                // Pixel Graph Outputs
                    struct SurfaceDescription
                    {
                        float3 Albedo;
                        float3 Normal;
                        float Smoothness;
                        float Occlusion;
                        float3 Specular;
                        float DiffusionProfile;
                        float SubsurfaceMask;
                        float Thickness;
                        float3 Emission;
                        float Alpha;
                    };
                    
                // Shared Graph Node Functions
                
                    void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
                    {
                        RGBA = float4(R, G, B, A);
                        RGB = float3(R, G, B);
                        RG = float2(R, G);
                    }
                
                    void Unity_Multiply_float (float2 A, float2 B, out float2 Out)
                    {
                        Out = A * B;
                    }
                
                    void Unity_Add_float2(float2 A, float2 B, out float2 Out)
                    {
                        Out = A + B;
                    }
                
                    // Subgraph function
                    void sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(float4 _uvST, float4 _uvMask, SurfaceDescriptionInputs IN, out float4 Output1)
                    {
                    float4 _UV_23AF8552_Out = IN.uv0;
                    float _Split_7957D60_R = _UV_23AF8552_Out[0];
                    float _Split_7957D60_G = _UV_23AF8552_Out[1];
                    float _Split_7957D60_B = _UV_23AF8552_Out[2];
                    float _Split_7957D60_A = _UV_23AF8552_Out[3];
                    float4 _Combine_5396A6C7_RGBA;
                    float3 _Combine_5396A6C7_RGB;
                    float2 _Combine_5396A6C7_RG;
                    Unity_Combine_float(_Split_7957D60_R, _Split_7957D60_G, 0, 0, _Combine_5396A6C7_RGBA, _Combine_5396A6C7_RGB, _Combine_5396A6C7_RG);
                    float4 _Property_CB55E443_Out = _uvMask;
                    float _Split_6086B0A5_R = _Property_CB55E443_Out[0];
                    float _Split_6086B0A5_G = _Property_CB55E443_Out[1];
                    float _Split_6086B0A5_B = _Property_CB55E443_Out[2];
                    float _Split_6086B0A5_A = _Property_CB55E443_Out[3];
                    float2 _Multiply_FC550A07_Out;
                    Unity_Multiply_float(_Combine_5396A6C7_RG, (_Split_6086B0A5_R.xx), _Multiply_FC550A07_Out);
                    
                    float4 _UV_3B1D042C_Out = IN.uv1;
                    float _Split_107320B6_R = _UV_3B1D042C_Out[0];
                    float _Split_107320B6_G = _UV_3B1D042C_Out[1];
                    float _Split_107320B6_B = _UV_3B1D042C_Out[2];
                    float _Split_107320B6_A = _UV_3B1D042C_Out[3];
                    float4 _Combine_2E8D3795_RGBA;
                    float3 _Combine_2E8D3795_RGB;
                    float2 _Combine_2E8D3795_RG;
                    Unity_Combine_float(_Split_107320B6_R, _Split_107320B6_G, 0, 0, _Combine_2E8D3795_RGBA, _Combine_2E8D3795_RGB, _Combine_2E8D3795_RG);
                    float2 _Multiply_FDA7BA1E_Out;
                    Unity_Multiply_float(_Combine_2E8D3795_RG, (_Split_6086B0A5_G.xx), _Multiply_FDA7BA1E_Out);
                    
                    float2 _Add_92015245_Out;
                    Unity_Add_float2(_Multiply_FC550A07_Out, _Multiply_FDA7BA1E_Out, _Add_92015245_Out);
                    float4 _UV_49BE4158_Out = IN.uv2;
                    float _Split_A24186AD_R = _UV_49BE4158_Out[0];
                    float _Split_A24186AD_G = _UV_49BE4158_Out[1];
                    float _Split_A24186AD_B = _UV_49BE4158_Out[2];
                    float _Split_A24186AD_A = _UV_49BE4158_Out[3];
                    float4 _Combine_6951B6BC_RGBA;
                    float3 _Combine_6951B6BC_RGB;
                    float2 _Combine_6951B6BC_RG;
                    Unity_Combine_float(_Split_A24186AD_R, _Split_A24186AD_G, 0, 0, _Combine_6951B6BC_RGBA, _Combine_6951B6BC_RGB, _Combine_6951B6BC_RG);
                    float2 _Multiply_1480B81_Out;
                    Unity_Multiply_float(_Combine_6951B6BC_RG, (_Split_6086B0A5_B.xx), _Multiply_1480B81_Out);
                    
                    float4 _UV_9CA65C2_Out = IN.uv3;
                    float _Split_9EC6EA10_R = _UV_9CA65C2_Out[0];
                    float _Split_9EC6EA10_G = _UV_9CA65C2_Out[1];
                    float _Split_9EC6EA10_B = _UV_9CA65C2_Out[2];
                    float _Split_9EC6EA10_A = _UV_9CA65C2_Out[3];
                    float4 _Combine_633F7D3D_RGBA;
                    float3 _Combine_633F7D3D_RGB;
                    float2 _Combine_633F7D3D_RG;
                    Unity_Combine_float(_Split_9EC6EA10_R, _Split_9EC6EA10_G, 0, 0, _Combine_633F7D3D_RGBA, _Combine_633F7D3D_RGB, _Combine_633F7D3D_RG);
                    float2 _Multiply_2A2B5227_Out;
                    Unity_Multiply_float(_Combine_633F7D3D_RG, (_Split_6086B0A5_A.xx), _Multiply_2A2B5227_Out);
                    
                    float2 _Add_B5E7679D_Out;
                    Unity_Add_float2(_Multiply_1480B81_Out, _Multiply_2A2B5227_Out, _Add_B5E7679D_Out);
                    float2 _Add_892742E3_Out;
                    Unity_Add_float2(_Add_92015245_Out, _Add_B5E7679D_Out, _Add_892742E3_Out);
                    float4 _Property_8DA1B077_Out = _uvST;
                    float _Split_1AB0DA31_R = _Property_8DA1B077_Out[0];
                    float _Split_1AB0DA31_G = _Property_8DA1B077_Out[1];
                    float _Split_1AB0DA31_B = _Property_8DA1B077_Out[2];
                    float _Split_1AB0DA31_A = _Property_8DA1B077_Out[3];
                    float4 _Combine_44459F1_RGBA;
                    float3 _Combine_44459F1_RGB;
                    float2 _Combine_44459F1_RG;
                    Unity_Combine_float(_Split_1AB0DA31_R, _Split_1AB0DA31_G, 0, 0, _Combine_44459F1_RGBA, _Combine_44459F1_RGB, _Combine_44459F1_RG);
                    float2 _Multiply_38815E23_Out;
                    Unity_Multiply_float(_Add_892742E3_Out, _Combine_44459F1_RG, _Multiply_38815E23_Out);
                    
                    float _Split_35A1DC4_R = _Property_8DA1B077_Out[0];
                    float _Split_35A1DC4_G = _Property_8DA1B077_Out[1];
                    float _Split_35A1DC4_B = _Property_8DA1B077_Out[2];
                    float _Split_35A1DC4_A = _Property_8DA1B077_Out[3];
                    float4 _Combine_91984BDF_RGBA;
                    float3 _Combine_91984BDF_RGB;
                    float2 _Combine_91984BDF_RG;
                    Unity_Combine_float(_Split_35A1DC4_B, _Split_35A1DC4_A, 0, 0, _Combine_91984BDF_RGBA, _Combine_91984BDF_RGB, _Combine_91984BDF_RG);
                    float2 _Add_63012CEE_Out;
                    Unity_Add_float2(_Multiply_38815E23_Out, _Combine_91984BDF_RG, _Add_63012CEE_Out);
                    Output1 = (float4(_Add_63012CEE_Out, 0.0, 1.0));
                    }
                
                    void Unity_Multiply_float (float4 A, float4 B, out float4 Out)
                    {
                        Out = A * B;
                    }
                
                    void Unity_Lerp_float(float A, float B, float T, out float Out)
                    {
                        Out = lerp(A, B, T);
                    }
                
                    void Unity_Add_float4(float4 A, float4 B, out float4 Out)
                    {
                        Out = A + B;
                    }
                
                    void Unity_Saturate_float4(float4 In, out float4 Out)
                    {
                        Out = saturate(In);
                    }
                
                    void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
                    {
                        Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
                    }
                
                    void Unity_Normalize_float3(float3 In, out float3 Out)
                    {
                        Out = normalize(In);
                    }
                
                    void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
                    {
                        Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
                    }
                
                    void Unity_NormalUnpack_float(float4 In, out float3 Out)
                    {
                                    Out = UnpackNormalmapRGorAG(In);
                                }
                
                    void Unity_NormalBlend_float(float3 A, float3 B, out float3 Out)
                    {
                        Out = normalize(float3(A.rg + B.rg, A.b * B.b));
                    }
                
                    void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
                    {
                        Out = lerp(False, True, Predicate);
                    }
                
                    void Unity_Add_float(float A, float B, out float Out)
                    {
                        Out = A + B;
                    }
                
                    void Unity_Saturate_float(float In, out float Out)
                    {
                        Out = saturate(In);
                    }
                
                    void Unity_Branch_float(float Predicate, float True, float False, out float Out)
                    {
                        Out = lerp(False, True, Predicate);
                    }
                
                    void Unity_Multiply_float (float A, float B, out float Out)
                    {
                        Out = A * B;
                    }
                
                    // Subgraph function
                    void sg_SGRThreadMapDetail_SurfaceDescriptionInputs_64D53B52(float2 _UV, TEXTURE2D_ARGS(_ThreadMap, sampler_ThreadMap), float _ThreadSmoothnessStrength, float _AmbientOcclusion, float _UseThreadMap, float _ThreadAOStrength, float _ThreadNormalStrength, float _Smoothness, float3 _Normals, float _Alpha, SurfaceDescriptionInputs IN, out float4 Output1, out float4 Output2, out float4 Output3, out float4 Output4)
                    {
                    float _Property_7B789410_Out = _UseThreadMap;
                    float3 _Property_D380C535_Out = _Normals;
                    float2 _Property_247E83DC_Out = _UV;
                    float4 _SampleTexture2D_B39DD828_RGBA = SAMPLE_TEXTURE2D(_ThreadMap, sampler_ThreadMap, _Property_247E83DC_Out);
                    float _SampleTexture2D_B39DD828_R = _SampleTexture2D_B39DD828_RGBA.r;
                    float _SampleTexture2D_B39DD828_G = _SampleTexture2D_B39DD828_RGBA.g;
                    float _SampleTexture2D_B39DD828_B = _SampleTexture2D_B39DD828_RGBA.b;
                    float _SampleTexture2D_B39DD828_A = _SampleTexture2D_B39DD828_RGBA.a;
                    float4 _Combine_3989CE7_RGBA;
                    float3 _Combine_3989CE7_RGB;
                    float2 _Combine_3989CE7_RG;
                    Unity_Combine_float(_SampleTexture2D_B39DD828_A, _SampleTexture2D_B39DD828_G, 1, 1, _Combine_3989CE7_RGBA, _Combine_3989CE7_RGB, _Combine_3989CE7_RG);
                    float3 _NormalUnpack_6B39F6EC_Out;
                    Unity_NormalUnpack_float((float4(_Combine_3989CE7_RGB, 1.0)), _NormalUnpack_6B39F6EC_Out);
                    float3 _Normalize_1F52E5EC_Out;
                    Unity_Normalize_float3(_NormalUnpack_6B39F6EC_Out, _Normalize_1F52E5EC_Out);
                    float _Property_2E175598_Out = _ThreadNormalStrength;
                    float3 _NormalStrength_A15875A3_Out;
                    Unity_NormalStrength_float(_Normalize_1F52E5EC_Out, _Property_2E175598_Out, _NormalStrength_A15875A3_Out);
                    float3 _NormalBlend_191D51BE_Out;
                    Unity_NormalBlend_float(_Property_D380C535_Out, _NormalStrength_A15875A3_Out, _NormalBlend_191D51BE_Out);
                    float3 _Normalize_4D9B04E_Out;
                    Unity_Normalize_float3(_NormalBlend_191D51BE_Out, _Normalize_4D9B04E_Out);
                    float3 _Branch_54FF636E_Out;
                    Unity_Branch_float3(_Property_7B789410_Out, _Normalize_4D9B04E_Out, _Property_D380C535_Out, _Branch_54FF636E_Out);
                    float _Property_B5560A97_Out = _UseThreadMap;
                    float _Property_6FAEC412_Out = _Smoothness;
                    float _Remap_C272A01C_Out;
                    Unity_Remap_float(_SampleTexture2D_B39DD828_B, float2 (0,1), float2 (-1,1), _Remap_C272A01C_Out);
                    float _Property_CF380DCA_Out = _ThreadSmoothnessStrength;
                    float _Lerp_1EB6EBC0_Out;
                    Unity_Lerp_float(0, _Remap_C272A01C_Out, _Property_CF380DCA_Out, _Lerp_1EB6EBC0_Out);
                    float _Add_2975BB_Out;
                    Unity_Add_float(_Property_6FAEC412_Out, _Lerp_1EB6EBC0_Out, _Add_2975BB_Out);
                    float _Saturate_1F46047D_Out;
                    Unity_Saturate_float(_Add_2975BB_Out, _Saturate_1F46047D_Out);
                    float _Branch_1C4EA1E2_Out;
                    Unity_Branch_float(_Property_B5560A97_Out, _Saturate_1F46047D_Out, _Property_6FAEC412_Out, _Branch_1C4EA1E2_Out);
                    float _Property_57F076E2_Out = _UseThreadMap;
                    float _Property_829FEB4F_Out = _ThreadAOStrength;
                    float _Lerp_1DC743E3_Out;
                    Unity_Lerp_float(1, _SampleTexture2D_B39DD828_R, _Property_829FEB4F_Out, _Lerp_1DC743E3_Out);
                    float _Property_416E73AE_Out = _AmbientOcclusion;
                    float _Multiply_FBD87ACD_Out;
                    Unity_Multiply_float(_Lerp_1DC743E3_Out, _Property_416E73AE_Out, _Multiply_FBD87ACD_Out);
                    
                    float _Branch_A5F3B7F9_Out;
                    Unity_Branch_float(_Property_57F076E2_Out, _Multiply_FBD87ACD_Out, _Property_416E73AE_Out, _Branch_A5F3B7F9_Out);
                    float _Property_5FDD4914_Out = _Alpha;
                    float _Multiply_716B151B_Out;
                    Unity_Multiply_float(_SampleTexture2D_B39DD828_R, _Property_5FDD4914_Out, _Multiply_716B151B_Out);
                    
                    Output1 = (float4(_Branch_54FF636E_Out, 1.0));
                    Output2 = (_Branch_1C4EA1E2_Out.xxxx);
                    Output3 = (_Branch_A5F3B7F9_Out.xxxx);
                    Output4 = (_Multiply_716B151B_Out.xxxx);
                    }
                
                // Pixel Graph Evaluation
                    SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                    {
                        SurfaceDescription surface = (SurfaceDescription)0;
                        float4 _Property_90FAF786_Out = _BaseColor;
                        float4 _Property_1E040901_Out = _uvBaseMask;
                        float4 _Property_97A7EF85_Out = _uvBaseST;
                        float4 _Subgraph_8DDCEE61_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_97A7EF85_Out, _Property_1E040901_Out, IN, _Subgraph_8DDCEE61_Output1);
                        float4 _SampleTexture2D_11CFD011_RGBA = SAMPLE_TEXTURE2D(_BaseColorMap, sampler_BaseColorMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_11CFD011_R = _SampleTexture2D_11CFD011_RGBA.r;
                        float _SampleTexture2D_11CFD011_G = _SampleTexture2D_11CFD011_RGBA.g;
                        float _SampleTexture2D_11CFD011_B = _SampleTexture2D_11CFD011_RGBA.b;
                        float _SampleTexture2D_11CFD011_A = _SampleTexture2D_11CFD011_RGBA.a;
                        float4 _Multiply_98A7A079_Out;
                        Unity_Multiply_float(_Property_90FAF786_Out, _SampleTexture2D_11CFD011_RGBA, _Multiply_98A7A079_Out);
                    
                        float _Property_7C6435CB_Out = _FuzzMapUVScale;
                        float4 _Multiply_18C3A780_Out;
                        Unity_Multiply_float(_Subgraph_8DDCEE61_Output1, (_Property_7C6435CB_Out.xxxx), _Multiply_18C3A780_Out);
                    
                        float4 _SampleTexture2D_4D82F05E_RGBA = SAMPLE_TEXTURE2D(_FuzzMap, sampler_FuzzMap, (_Multiply_18C3A780_Out.xy));
                        float _SampleTexture2D_4D82F05E_R = _SampleTexture2D_4D82F05E_RGBA.r;
                        float _SampleTexture2D_4D82F05E_G = _SampleTexture2D_4D82F05E_RGBA.g;
                        float _SampleTexture2D_4D82F05E_B = _SampleTexture2D_4D82F05E_RGBA.b;
                        float _SampleTexture2D_4D82F05E_A = _SampleTexture2D_4D82F05E_RGBA.a;
                        float _Property_6CCE2816_Out = _FuzzStrength;
                        float _Lerp_2C953D15_Out;
                        Unity_Lerp_float(0, _SampleTexture2D_4D82F05E_R, _Property_6CCE2816_Out, _Lerp_2C953D15_Out);
                        float4 _Add_A30FF2E2_Out;
                        Unity_Add_float4(_Multiply_98A7A079_Out, (_Lerp_2C953D15_Out.xxxx), _Add_A30FF2E2_Out);
                        float4 _Saturate_69BD2FF3_Out;
                        Unity_Saturate_float4(_Add_A30FF2E2_Out, _Saturate_69BD2FF3_Out);
                        float _Property_1E54B66A_Out = _useThreadMap;
                        float4 _Property_8AE14795_Out = _uvThreadMask;
                        float4 _Property_958B7FC9_Out = _uvThreadST;
                        float4 _Subgraph_B567E108_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_958B7FC9_Out, _Property_8AE14795_Out, IN, _Subgraph_B567E108_Output1);
                        float4 _Property_FEDB20A0_Out = _uvBaseMask;
                        float4 _Property_F42AAF3B_Out = _uvBaseST;
                        float4 _Subgraph_9D4E0F1_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_F42AAF3B_Out, _Property_FEDB20A0_Out, IN, _Subgraph_9D4E0F1_Output1);
                        float4 _SampleTexture2D_105B35B3_RGBA = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, (_Subgraph_9D4E0F1_Output1.xy));
                        _SampleTexture2D_105B35B3_RGBA.rgb = UnpackNormalmapRGorAG(_SampleTexture2D_105B35B3_RGBA);
                        float _SampleTexture2D_105B35B3_R = _SampleTexture2D_105B35B3_RGBA.r;
                        float _SampleTexture2D_105B35B3_G = _SampleTexture2D_105B35B3_RGBA.g;
                        float _SampleTexture2D_105B35B3_B = _SampleTexture2D_105B35B3_RGBA.b;
                        float _SampleTexture2D_105B35B3_A = _SampleTexture2D_105B35B3_RGBA.a;
                        float _Property_82D183C3_Out = _NormalMapStrength;
                        float3 _NormalStrength_BFF5C35E_Out;
                        Unity_NormalStrength_float((_SampleTexture2D_105B35B3_RGBA.xyz), _Property_82D183C3_Out, _NormalStrength_BFF5C35E_Out);
                        float3 _Normalize_ACA4E10E_Out;
                        Unity_Normalize_float3(_NormalStrength_BFF5C35E_Out, _Normalize_ACA4E10E_Out);
                        float4 _SampleTexture2D_8C3CF01A_RGBA = SAMPLE_TEXTURE2D(_SpecColorMap, sampler_SpecColorMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_8C3CF01A_R = _SampleTexture2D_8C3CF01A_RGBA.r;
                        float _SampleTexture2D_8C3CF01A_G = _SampleTexture2D_8C3CF01A_RGBA.g;
                        float _SampleTexture2D_8C3CF01A_B = _SampleTexture2D_8C3CF01A_RGBA.b;
                        float _SampleTexture2D_8C3CF01A_A = _SampleTexture2D_8C3CF01A_RGBA.a;
                        float _Property_B948927_Out = _SmoothnessMin;
                        float _Property_2962A49E_Out = _SmoothnessMax;
                        float2 _Vector2_9C783D17_Out = float2(_Property_B948927_Out,_Property_2962A49E_Out);
                        float _Remap_10DEF6A_Out;
                        Unity_Remap_float(_SampleTexture2D_8C3CF01A_A, float2 (0,1), _Vector2_9C783D17_Out, _Remap_10DEF6A_Out);
                        float _Split_EB0B739F_R = _Saturate_69BD2FF3_Out[0];
                        float _Split_EB0B739F_G = _Saturate_69BD2FF3_Out[1];
                        float _Split_EB0B739F_B = _Saturate_69BD2FF3_Out[2];
                        float _Split_EB0B739F_A = _Saturate_69BD2FF3_Out[3];
                        float4 _SampleTexture2D_EECA7933_RGBA = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_EECA7933_R = _SampleTexture2D_EECA7933_RGBA.r;
                        float _SampleTexture2D_EECA7933_G = _SampleTexture2D_EECA7933_RGBA.g;
                        float _SampleTexture2D_EECA7933_B = _SampleTexture2D_EECA7933_RGBA.b;
                        float _SampleTexture2D_EECA7933_A = _SampleTexture2D_EECA7933_RGBA.a;
                        float _Property_88B45C0E_Out = _ThreadAOStrength01;
                        float _Property_FC0CC4C0_Out = _ThreadNormalStrength;
                        float _Property_AC495D22_Out = _ThreadSmoothnessScale;
                        float4 _Subgraph_E494B5B1_Output1;
                        float4 _Subgraph_E494B5B1_Output2;
                        float4 _Subgraph_E494B5B1_Output3;
                        float4 _Subgraph_E494B5B1_Output4;
                        sg_SGRThreadMapDetail_SurfaceDescriptionInputs_64D53B52((_Subgraph_B567E108_Output1.xy), TEXTURE2D_PARAM(_ThreadMap, sampler_ThreadMap), _Property_AC495D22_Out, _SampleTexture2D_EECA7933_G, _Property_1E54B66A_Out, _Property_88B45C0E_Out, _Property_FC0CC4C0_Out, _Remap_10DEF6A_Out, _Normalize_ACA4E10E_Out, _Split_EB0B739F_A, IN, _Subgraph_E494B5B1_Output1, _Subgraph_E494B5B1_Output2, _Subgraph_E494B5B1_Output3, _Subgraph_E494B5B1_Output4);
                        float4 _Property_BFE334DC_Out = _SpecularColor;
                        surface.Albedo = (_Saturate_69BD2FF3_Out.xyz);
                        surface.Normal = (_Subgraph_E494B5B1_Output1.xyz);
                        surface.Smoothness = (_Subgraph_E494B5B1_Output2).x;
                        surface.Occlusion = (_Subgraph_E494B5B1_Output3).x;
                        surface.Specular = (_Property_BFE334DC_Out.xyz);
                        surface.DiffusionProfile = 4;
                        surface.SubsurfaceMask = _SampleTexture2D_EECA7933_B;
                        surface.Thickness = _SampleTexture2D_EECA7933_R;
                        surface.Emission = float3(0, 0, 0);
                        surface.Alpha = (_Subgraph_E494B5B1_Output4).x;
                        return surface;
                    }
                    
            //-------------------------------------------------------------------------------------
            // End graph generated code
            //-------------------------------------------------------------------------------------
        
        
        
        //-------------------------------------------------------------------------------------
        // TEMPLATE INCLUDE : SharedCode.template.hlsl
        //-------------------------------------------------------------------------------------
            FragInputs BuildFragInputs(VaryingsMeshToPS input)
            {
                FragInputs output;
                ZERO_INITIALIZE(FragInputs, output);
        
                // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                // to compute normals which are then passed on elsewhere to compute other values...
                output.worldToTangent = k_identity3x3;
                output.positionSS = input.positionCS;       // input.positionCS is SV_Position
        
                output.texCoord0 = input.texCoord0;
                output.texCoord1 = input.texCoord1;
                output.texCoord2 = input.texCoord2;
                output.texCoord3 = input.texCoord3;
                #if SHADER_STAGE_FRAGMENT
                #endif // SHADER_STAGE_FRAGMENT
        
                return output;
            }
        
            SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
                output.uv0 =                         input.texCoord0;
                output.uv1 =                         input.texCoord1;
                output.uv2 =                         input.texCoord2;
                output.uv3 =                         input.texCoord3;
        
                return output;
            }
        
            // existing HDRP code uses the combined function to go directly from packed to frag inputs
            FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
            {
                VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                return BuildFragInputs(unpacked);
            }
        
        //-------------------------------------------------------------------------------------
        // END TEMPLATE INCLUDE : SharedCode.template.hlsl
        //-------------------------------------------------------------------------------------
        
        
            void ApplyDecalToSurfaceData(DecalSurfaceData decalSurfaceData, inout SurfaceData surfaceData)
            {
                // using alpha compositing https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch23.html
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_DIFFUSE)
                {
                    surfaceData.baseColor.xyz = surfaceData.baseColor.xyz * decalSurfaceData.baseColor.w + decalSurfaceData.baseColor.xyz;
                }
        
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_NORMAL)
                {
                    surfaceData.normalWS.xyz = normalize(surfaceData.normalWS.xyz * decalSurfaceData.normalWS.w + decalSurfaceData.normalWS.xyz);
                }
        
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_MASK)
                {
            #ifdef DECALS_4RT // only smoothness in 3RT mode
                    // Don't apply any metallic modification
                    surfaceData.ambientOcclusion = surfaceData.ambientOcclusion * decalSurfaceData.MAOSBlend.y + decalSurfaceData.mask.y;
            #endif
        
                    surfaceData.perceptualSmoothness = surfaceData.perceptualSmoothness * decalSurfaceData.mask.w + decalSurfaceData.mask.z;
                }
            }
        
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, out SurfaceData surfaceData)
            {
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(SurfaceData, surfaceData);
        
                // copy across graph values, if defined
                surfaceData.baseColor =                 surfaceDescription.Albedo;
        
        
                surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
        
                surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
        
                surfaceData.specularColor =             surfaceDescription.Specular;
        
                surfaceData.diffusionProfile =          surfaceDescription.DiffusionProfile;
        
                surfaceData.subsurfaceMask =            surfaceDescription.SubsurfaceMask;
        
                surfaceData.thickness =                 surfaceDescription.Thickness;
        
                surfaceData.diffusionProfile =          surfaceDescription.DiffusionProfile;
        
                
                // These static material feature allow compile time optimization
                surfaceData.materialFeatures = 0;
        
                // Transform the preprocess macro into a material feature (note that silk flag is deduced from the abscence of this one)
                #ifdef _MATERIAL_FEATURE_COTTON_WOOL
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_COTTON_WOOL;
                #endif
        
                #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_SUBSURFACE_SCATTERING;
                #endif
        
                #ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_TRANSMISSION;
                #endif
        
        
        #if defined (_ENERGY_CONSERVING_SPECULAR)
                // Require to have setup baseColor
                // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
        #endif
        
                // tangent-space normal
                float3 normalTS = float3(0.0f, 0.0f, 1.0f);
                normalTS = surfaceDescription.Normal;
        
                // compute world space normal
                GetNormalWS(fragInputs, normalTS, surfaceData.normalWS);
        
                surfaceData.geomNormalWS = fragInputs.worldToTangent[2];
        
                surfaceData.tangentWS = normalize(fragInputs.worldToTangent[0].xyz);    // The tangent is not normalize in worldToTangent for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
        
                // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                // If user provide bent normal then we process a better term
                surfaceData.specularOcclusion = 1.0;
        
        #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                // Just use the value passed through via the slot (not active otherwise)
        #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                // If we have bent normal and ambient occlusion, process a specular occlusion
                surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
        #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
        #else
                surfaceData.specularOcclusion = 1.0;
                surfaceData.specularOcclusion = 1.0;
        #endif
        
        #ifdef DEBUG_DISPLAY
                // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                // as it can modify attribute use for static lighting
                ApplyDebugToSurfaceData(fragInputs.worldToTangent, surfaceData);
        #endif
            }
        
            void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
            {
        #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                uint3 fadeMaskSeed = asuint((int3)(V * _ScreenSize.xyx)); // Quantize V to _ScreenSize values
                LODDitheringTransition(fadeMaskSeed, unity_LODFade.x);
        #endif
        
                // this applies the double sided tangent space correction -- see 'ApplyDoubleSidedFlipOrMirror()'
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
        
                BuildSurfaceData(fragInputs, surfaceDescription, V, surfaceData);
        
        #if HAVE_DECALS && _DECALS
                DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, surfaceDescription.Alpha);
                ApplyDecalToSurfaceData(decalSurfaceData, surfaceData);
        #endif
        
                // Builtin Data
                // For back lighting we use the oposite vertex normal 
                InitBuiltinData(surfaceDescription.Alpha, surfaceData.normalWS, -fragInputs.worldToTangent[2], fragInputs.positionRWS, fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
        
                builtinData.emissiveColor = surfaceDescription.Emission;
        
                builtinData.depthOffset = 0.0;                        // ApplyPerPixelDisplacement(input, V, layerTexCoord, blendMasks); #ifdef _DEPTHOFFSET_ON : ApplyDepthOffsetPositionInput(V, depthOffset, GetWorldToHClipMatrix(), posInput);
        
                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
            }
        
            //-------------------------------------------------------------------------------------
            // Pass Includes
            //-------------------------------------------------------------------------------------
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassLightTransport.hlsl"
            //-------------------------------------------------------------------------------------
            // End Pass Includes
            //-------------------------------------------------------------------------------------
        
            ENDHLSL
        }
        
        Pass
        {
            // based on FabricPass.template
            Name "SceneSelectionPass"
            Tags { "LightMode" = "SceneSelectionPass" }
        
            //-------------------------------------------------------------------------------------
            // Render Modes (Blend, Cull, ZTest, Stencil, etc)
            //-------------------------------------------------------------------------------------
                Blend One Zero
        
                Cull Back
        
                ZTest LEqual
        
                ZWrite On
        
                ZClip [_ZClip]
        
                // Default Stencil
        
                ColorMask 0
        
            //-------------------------------------------------------------------------------------
            // End Render Modes
            //-------------------------------------------------------------------------------------
        
            HLSLPROGRAM
        
                #pragma target 4.5
                #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
                //#pragma enable_d3d11_debug_symbols
        
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
        
                #pragma multi_compile _ LOD_FADE_CROSSFADE
        
            //-------------------------------------------------------------------------------------
            // Variant Definitions (active field translations to HDRP defines)
            //-------------------------------------------------------------------------------------
                #define _MATERIAL_FEATURE_COTTON_WOOL 1
                #define _SPECULAR_OCCLUSION_FROM_AO 1
                #define _ENERGY_CONSERVING_SPECULAR 1
                #define _DISABLE_SSR 1
            //-------------------------------------------------------------------------------------
            // End Variant Definitions
            //-------------------------------------------------------------------------------------
        
            #pragma vertex Vert
            #pragma fragment Frag
        
            #define UNITY_MATERIAL_FABRIC      // Need to be define before including Material.hlsl
        
            // This will be enabled in an upcoming change. 
            // #define SURFACE_GRADIENT
        
            // If we use subsurface scattering, enable output split lighting (for forward pass)
            #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
            #define OUTPUT_SPLIT_LIGHTING
            #endif
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Wind.hlsl"
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
        
            // define FragInputs structure
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
        
            //-------------------------------------------------------------------------------------
            // Defines
            //-------------------------------------------------------------------------------------
                #define SHADERPASS SHADERPASS_DEPTH_ONLY
                #define SCENESELECTIONPASS
        
                // this translates the new dependency tracker into the old preprocessor definitions for the existing HDRP shader code
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_TEXCOORD2
                #define ATTRIBUTES_NEED_TEXCOORD3
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_TEXCOORD2
                #define VARYINGS_NEED_TEXCOORD3
        
            //-------------------------------------------------------------------------------------
            // End Defines
            //-------------------------------------------------------------------------------------
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #ifdef DEBUG_DISPLAY
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif
        
            #if (SHADERPASS == SHADERPASS_FORWARD)
                // used for shaders that want to do lighting (and materials)
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
            #else
                // used for shaders that don't need lighting
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #endif
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
        
            //Used by SceneSelectionPass
            int _ObjectId;
            int _PassValue;
        
            // this function assumes the bitangent flip is encoded in tangentWS.w
            // TODO: move this function to HDRP shared file, once we merge with HDRP repo
            float3x3 BuildWorldToTangent(float4 tangentWS, float3 normalWS)
            {
                // tangentWS must not be normalized (mikkts requirement)
        
                // Normalize normalWS vector but keep the renormFactor to apply it to bitangent and tangent
        	    float3 unnormalizedNormalWS = normalWS;
                float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
                // bitangent on the fly option in xnormal to reduce vertex shader outputs.
        	    // this is the mikktspace transformation (must use unnormalized attributes)
                float3x3 worldToTangent = CreateWorldToTangent(unnormalizedNormalWS, tangentWS.xyz, tangentWS.w > 0.0 ? 1.0 : -1.0);
        
        	    // surface gradient based formulation requires a unit length initial normal. We can maintain compliance with mikkts
        	    // by uniformly scaling all 3 vectors since normalization of the perturbed normal will cancel it.
                worldToTangent[0] = worldToTangent[0] * renormFactor;
                worldToTangent[1] = worldToTangent[1] * renormFactor;
                worldToTangent[2] = worldToTangent[2] * renormFactor;		// normalizes the interpolated vertex normal
                return worldToTangent;
            }
        
            //-------------------------------------------------------------------------------------
            // Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
        // Generated Type: AttributesMesh
        struct AttributesMesh {
            float3 positionOS : POSITION;
            float4 uv0 : TEXCOORD0; // optional
            float4 uv1 : TEXCOORD1; // optional
            float4 uv2 : TEXCOORD2; // optional
            float4 uv3 : TEXCOORD3; // optional
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        
        // Generated Type: VaryingsMeshToPS
        struct VaryingsMeshToPS {
            float4 positionCS : SV_Position;
            float4 texCoord0; // optional
            float4 texCoord1; // optional
            float4 texCoord2; // optional
            float4 texCoord3; // optional
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        struct PackedVaryingsMeshToPS {
            float4 interp00 : TEXCOORD0; // auto-packed
            float4 interp01 : TEXCOORD1; // auto-packed
            float4 interp02 : TEXCOORD2; // auto-packed
            float4 interp03 : TEXCOORD3; // auto-packed
            float4 positionCS : SV_Position; // unpacked
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC; // unpacked
            #endif // INSTANCING_ON
        };
        PackedVaryingsMeshToPS PackVaryingsMeshToPS(VaryingsMeshToPS input)
        {
            PackedVaryingsMeshToPS output;
            output.positionCS = input.positionCS;
            output.interp00.xyzw = input.texCoord0;
            output.interp01.xyzw = input.texCoord1;
            output.interp02.xyzw = input.texCoord2;
            output.interp03.xyzw = input.texCoord3;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        VaryingsMeshToPS UnpackVaryingsMeshToPS(PackedVaryingsMeshToPS input)
        {
            VaryingsMeshToPS output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp00.xyzw;
            output.texCoord1 = input.interp01.xyzw;
            output.texCoord2 = input.interp02.xyzw;
            output.texCoord3 = input.interp03.xyzw;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        
        // Generated Type: VaryingsMeshToDS
        struct VaryingsMeshToDS {
            float3 positionRWS;
            float3 normalWS;
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        struct PackedVaryingsMeshToDS {
            float3 interp00 : TEXCOORD0; // auto-packed
            float3 interp01 : TEXCOORD1; // auto-packed
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC; // unpacked
            #endif // INSTANCING_ON
        };
        PackedVaryingsMeshToDS PackVaryingsMeshToDS(VaryingsMeshToDS input)
        {
            PackedVaryingsMeshToDS output;
            output.interp00.xyz = input.positionRWS;
            output.interp01.xyz = input.normalWS;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        VaryingsMeshToDS UnpackVaryingsMeshToDS(PackedVaryingsMeshToDS input)
        {
            VaryingsMeshToDS output;
            output.positionRWS = input.interp00.xyz;
            output.normalWS = input.interp01.xyz;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        
            //-------------------------------------------------------------------------------------
            // End Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
        
            //-------------------------------------------------------------------------------------
            // Graph generated code
            //-------------------------------------------------------------------------------------
                // Shared Graph Properties (uniform inputs)
                    CBUFFER_START(UnityPerMaterial)
                    float4 _uvBaseMask;
                    float4 _uvBaseST;
                    float4 _BaseColor;
                    float _SmoothnessMin;
                    float _SmoothnessMax;
                    float4 _SpecularColor;
                    float _NormalMapStrength;
                    float _useThreadMap;
                    float4 _uvThreadMask;
                    float4 _uvThreadST;
                    float _ThreadAOStrength01;
                    float _ThreadNormalStrength;
                    float _ThreadSmoothnessScale;
                    float _FuzzMapUVScale;
                    float _FuzzStrength;
                    float4 _EmissionColor;
                    CBUFFER_END
                
                    TEXTURE2D(_BaseColorMap); SAMPLER(sampler_BaseColorMap); float4 _BaseColorMap_TexelSize;
                    TEXTURE2D(_MaskMap); SAMPLER(sampler_MaskMap); float4 _MaskMap_TexelSize;
                    TEXTURE2D(_SpecColorMap); SAMPLER(sampler_SpecColorMap); float4 _SpecColorMap_TexelSize;
                    TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap); float4 _NormalMap_TexelSize;
                    TEXTURE2D(_ThreadMap); SAMPLER(sampler_ThreadMap); float4 _ThreadMap_TexelSize;
                    TEXTURE2D(_FuzzMap); SAMPLER(sampler_FuzzMap); float4 _FuzzMap_TexelSize;
                
                // Pixel Graph Inputs
                    struct SurfaceDescriptionInputs {
                        float4 uv0; // optional
                        float4 uv1; // optional
                        float4 uv2; // optional
                        float4 uv3; // optional
                    };
                // Pixel Graph Outputs
                    struct SurfaceDescription
                    {
                        float Alpha;
                    };
                    
                // Shared Graph Node Functions
                
                    void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
                    {
                        RGBA = float4(R, G, B, A);
                        RGB = float3(R, G, B);
                        RG = float2(R, G);
                    }
                
                    void Unity_Multiply_float (float2 A, float2 B, out float2 Out)
                    {
                        Out = A * B;
                    }
                
                    void Unity_Add_float2(float2 A, float2 B, out float2 Out)
                    {
                        Out = A + B;
                    }
                
                    // Subgraph function
                    void sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(float4 _uvST, float4 _uvMask, SurfaceDescriptionInputs IN, out float4 Output1)
                    {
                    float4 _UV_23AF8552_Out = IN.uv0;
                    float _Split_7957D60_R = _UV_23AF8552_Out[0];
                    float _Split_7957D60_G = _UV_23AF8552_Out[1];
                    float _Split_7957D60_B = _UV_23AF8552_Out[2];
                    float _Split_7957D60_A = _UV_23AF8552_Out[3];
                    float4 _Combine_5396A6C7_RGBA;
                    float3 _Combine_5396A6C7_RGB;
                    float2 _Combine_5396A6C7_RG;
                    Unity_Combine_float(_Split_7957D60_R, _Split_7957D60_G, 0, 0, _Combine_5396A6C7_RGBA, _Combine_5396A6C7_RGB, _Combine_5396A6C7_RG);
                    float4 _Property_CB55E443_Out = _uvMask;
                    float _Split_6086B0A5_R = _Property_CB55E443_Out[0];
                    float _Split_6086B0A5_G = _Property_CB55E443_Out[1];
                    float _Split_6086B0A5_B = _Property_CB55E443_Out[2];
                    float _Split_6086B0A5_A = _Property_CB55E443_Out[3];
                    float2 _Multiply_FC550A07_Out;
                    Unity_Multiply_float(_Combine_5396A6C7_RG, (_Split_6086B0A5_R.xx), _Multiply_FC550A07_Out);
                    
                    float4 _UV_3B1D042C_Out = IN.uv1;
                    float _Split_107320B6_R = _UV_3B1D042C_Out[0];
                    float _Split_107320B6_G = _UV_3B1D042C_Out[1];
                    float _Split_107320B6_B = _UV_3B1D042C_Out[2];
                    float _Split_107320B6_A = _UV_3B1D042C_Out[3];
                    float4 _Combine_2E8D3795_RGBA;
                    float3 _Combine_2E8D3795_RGB;
                    float2 _Combine_2E8D3795_RG;
                    Unity_Combine_float(_Split_107320B6_R, _Split_107320B6_G, 0, 0, _Combine_2E8D3795_RGBA, _Combine_2E8D3795_RGB, _Combine_2E8D3795_RG);
                    float2 _Multiply_FDA7BA1E_Out;
                    Unity_Multiply_float(_Combine_2E8D3795_RG, (_Split_6086B0A5_G.xx), _Multiply_FDA7BA1E_Out);
                    
                    float2 _Add_92015245_Out;
                    Unity_Add_float2(_Multiply_FC550A07_Out, _Multiply_FDA7BA1E_Out, _Add_92015245_Out);
                    float4 _UV_49BE4158_Out = IN.uv2;
                    float _Split_A24186AD_R = _UV_49BE4158_Out[0];
                    float _Split_A24186AD_G = _UV_49BE4158_Out[1];
                    float _Split_A24186AD_B = _UV_49BE4158_Out[2];
                    float _Split_A24186AD_A = _UV_49BE4158_Out[3];
                    float4 _Combine_6951B6BC_RGBA;
                    float3 _Combine_6951B6BC_RGB;
                    float2 _Combine_6951B6BC_RG;
                    Unity_Combine_float(_Split_A24186AD_R, _Split_A24186AD_G, 0, 0, _Combine_6951B6BC_RGBA, _Combine_6951B6BC_RGB, _Combine_6951B6BC_RG);
                    float2 _Multiply_1480B81_Out;
                    Unity_Multiply_float(_Combine_6951B6BC_RG, (_Split_6086B0A5_B.xx), _Multiply_1480B81_Out);
                    
                    float4 _UV_9CA65C2_Out = IN.uv3;
                    float _Split_9EC6EA10_R = _UV_9CA65C2_Out[0];
                    float _Split_9EC6EA10_G = _UV_9CA65C2_Out[1];
                    float _Split_9EC6EA10_B = _UV_9CA65C2_Out[2];
                    float _Split_9EC6EA10_A = _UV_9CA65C2_Out[3];
                    float4 _Combine_633F7D3D_RGBA;
                    float3 _Combine_633F7D3D_RGB;
                    float2 _Combine_633F7D3D_RG;
                    Unity_Combine_float(_Split_9EC6EA10_R, _Split_9EC6EA10_G, 0, 0, _Combine_633F7D3D_RGBA, _Combine_633F7D3D_RGB, _Combine_633F7D3D_RG);
                    float2 _Multiply_2A2B5227_Out;
                    Unity_Multiply_float(_Combine_633F7D3D_RG, (_Split_6086B0A5_A.xx), _Multiply_2A2B5227_Out);
                    
                    float2 _Add_B5E7679D_Out;
                    Unity_Add_float2(_Multiply_1480B81_Out, _Multiply_2A2B5227_Out, _Add_B5E7679D_Out);
                    float2 _Add_892742E3_Out;
                    Unity_Add_float2(_Add_92015245_Out, _Add_B5E7679D_Out, _Add_892742E3_Out);
                    float4 _Property_8DA1B077_Out = _uvST;
                    float _Split_1AB0DA31_R = _Property_8DA1B077_Out[0];
                    float _Split_1AB0DA31_G = _Property_8DA1B077_Out[1];
                    float _Split_1AB0DA31_B = _Property_8DA1B077_Out[2];
                    float _Split_1AB0DA31_A = _Property_8DA1B077_Out[3];
                    float4 _Combine_44459F1_RGBA;
                    float3 _Combine_44459F1_RGB;
                    float2 _Combine_44459F1_RG;
                    Unity_Combine_float(_Split_1AB0DA31_R, _Split_1AB0DA31_G, 0, 0, _Combine_44459F1_RGBA, _Combine_44459F1_RGB, _Combine_44459F1_RG);
                    float2 _Multiply_38815E23_Out;
                    Unity_Multiply_float(_Add_892742E3_Out, _Combine_44459F1_RG, _Multiply_38815E23_Out);
                    
                    float _Split_35A1DC4_R = _Property_8DA1B077_Out[0];
                    float _Split_35A1DC4_G = _Property_8DA1B077_Out[1];
                    float _Split_35A1DC4_B = _Property_8DA1B077_Out[2];
                    float _Split_35A1DC4_A = _Property_8DA1B077_Out[3];
                    float4 _Combine_91984BDF_RGBA;
                    float3 _Combine_91984BDF_RGB;
                    float2 _Combine_91984BDF_RG;
                    Unity_Combine_float(_Split_35A1DC4_B, _Split_35A1DC4_A, 0, 0, _Combine_91984BDF_RGBA, _Combine_91984BDF_RGB, _Combine_91984BDF_RG);
                    float2 _Add_63012CEE_Out;
                    Unity_Add_float2(_Multiply_38815E23_Out, _Combine_91984BDF_RG, _Add_63012CEE_Out);
                    Output1 = (float4(_Add_63012CEE_Out, 0.0, 1.0));
                    }
                
                    void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
                    {
                        Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
                    }
                
                    void Unity_Normalize_float3(float3 In, out float3 Out)
                    {
                        Out = normalize(In);
                    }
                
                    void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
                    {
                        Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
                    }
                
                    void Unity_Multiply_float (float4 A, float4 B, out float4 Out)
                    {
                        Out = A * B;
                    }
                
                    void Unity_Lerp_float(float A, float B, float T, out float Out)
                    {
                        Out = lerp(A, B, T);
                    }
                
                    void Unity_Add_float4(float4 A, float4 B, out float4 Out)
                    {
                        Out = A + B;
                    }
                
                    void Unity_Saturate_float4(float4 In, out float4 Out)
                    {
                        Out = saturate(In);
                    }
                
                    void Unity_NormalUnpack_float(float4 In, out float3 Out)
                    {
                                    Out = UnpackNormalmapRGorAG(In);
                                }
                
                    void Unity_NormalBlend_float(float3 A, float3 B, out float3 Out)
                    {
                        Out = normalize(float3(A.rg + B.rg, A.b * B.b));
                    }
                
                    void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
                    {
                        Out = lerp(False, True, Predicate);
                    }
                
                    void Unity_Add_float(float A, float B, out float Out)
                    {
                        Out = A + B;
                    }
                
                    void Unity_Saturate_float(float In, out float Out)
                    {
                        Out = saturate(In);
                    }
                
                    void Unity_Branch_float(float Predicate, float True, float False, out float Out)
                    {
                        Out = lerp(False, True, Predicate);
                    }
                
                    void Unity_Multiply_float (float A, float B, out float Out)
                    {
                        Out = A * B;
                    }
                
                    // Subgraph function
                    void sg_SGRThreadMapDetail_SurfaceDescriptionInputs_64D53B52(float2 _UV, TEXTURE2D_ARGS(_ThreadMap, sampler_ThreadMap), float _ThreadSmoothnessStrength, float _AmbientOcclusion, float _UseThreadMap, float _ThreadAOStrength, float _ThreadNormalStrength, float _Smoothness, float3 _Normals, float _Alpha, SurfaceDescriptionInputs IN, out float4 Output1, out float4 Output2, out float4 Output3, out float4 Output4)
                    {
                    float _Property_7B789410_Out = _UseThreadMap;
                    float3 _Property_D380C535_Out = _Normals;
                    float2 _Property_247E83DC_Out = _UV;
                    float4 _SampleTexture2D_B39DD828_RGBA = SAMPLE_TEXTURE2D(_ThreadMap, sampler_ThreadMap, _Property_247E83DC_Out);
                    float _SampleTexture2D_B39DD828_R = _SampleTexture2D_B39DD828_RGBA.r;
                    float _SampleTexture2D_B39DD828_G = _SampleTexture2D_B39DD828_RGBA.g;
                    float _SampleTexture2D_B39DD828_B = _SampleTexture2D_B39DD828_RGBA.b;
                    float _SampleTexture2D_B39DD828_A = _SampleTexture2D_B39DD828_RGBA.a;
                    float4 _Combine_3989CE7_RGBA;
                    float3 _Combine_3989CE7_RGB;
                    float2 _Combine_3989CE7_RG;
                    Unity_Combine_float(_SampleTexture2D_B39DD828_A, _SampleTexture2D_B39DD828_G, 1, 1, _Combine_3989CE7_RGBA, _Combine_3989CE7_RGB, _Combine_3989CE7_RG);
                    float3 _NormalUnpack_6B39F6EC_Out;
                    Unity_NormalUnpack_float((float4(_Combine_3989CE7_RGB, 1.0)), _NormalUnpack_6B39F6EC_Out);
                    float3 _Normalize_1F52E5EC_Out;
                    Unity_Normalize_float3(_NormalUnpack_6B39F6EC_Out, _Normalize_1F52E5EC_Out);
                    float _Property_2E175598_Out = _ThreadNormalStrength;
                    float3 _NormalStrength_A15875A3_Out;
                    Unity_NormalStrength_float(_Normalize_1F52E5EC_Out, _Property_2E175598_Out, _NormalStrength_A15875A3_Out);
                    float3 _NormalBlend_191D51BE_Out;
                    Unity_NormalBlend_float(_Property_D380C535_Out, _NormalStrength_A15875A3_Out, _NormalBlend_191D51BE_Out);
                    float3 _Normalize_4D9B04E_Out;
                    Unity_Normalize_float3(_NormalBlend_191D51BE_Out, _Normalize_4D9B04E_Out);
                    float3 _Branch_54FF636E_Out;
                    Unity_Branch_float3(_Property_7B789410_Out, _Normalize_4D9B04E_Out, _Property_D380C535_Out, _Branch_54FF636E_Out);
                    float _Property_B5560A97_Out = _UseThreadMap;
                    float _Property_6FAEC412_Out = _Smoothness;
                    float _Remap_C272A01C_Out;
                    Unity_Remap_float(_SampleTexture2D_B39DD828_B, float2 (0,1), float2 (-1,1), _Remap_C272A01C_Out);
                    float _Property_CF380DCA_Out = _ThreadSmoothnessStrength;
                    float _Lerp_1EB6EBC0_Out;
                    Unity_Lerp_float(0, _Remap_C272A01C_Out, _Property_CF380DCA_Out, _Lerp_1EB6EBC0_Out);
                    float _Add_2975BB_Out;
                    Unity_Add_float(_Property_6FAEC412_Out, _Lerp_1EB6EBC0_Out, _Add_2975BB_Out);
                    float _Saturate_1F46047D_Out;
                    Unity_Saturate_float(_Add_2975BB_Out, _Saturate_1F46047D_Out);
                    float _Branch_1C4EA1E2_Out;
                    Unity_Branch_float(_Property_B5560A97_Out, _Saturate_1F46047D_Out, _Property_6FAEC412_Out, _Branch_1C4EA1E2_Out);
                    float _Property_57F076E2_Out = _UseThreadMap;
                    float _Property_829FEB4F_Out = _ThreadAOStrength;
                    float _Lerp_1DC743E3_Out;
                    Unity_Lerp_float(1, _SampleTexture2D_B39DD828_R, _Property_829FEB4F_Out, _Lerp_1DC743E3_Out);
                    float _Property_416E73AE_Out = _AmbientOcclusion;
                    float _Multiply_FBD87ACD_Out;
                    Unity_Multiply_float(_Lerp_1DC743E3_Out, _Property_416E73AE_Out, _Multiply_FBD87ACD_Out);
                    
                    float _Branch_A5F3B7F9_Out;
                    Unity_Branch_float(_Property_57F076E2_Out, _Multiply_FBD87ACD_Out, _Property_416E73AE_Out, _Branch_A5F3B7F9_Out);
                    float _Property_5FDD4914_Out = _Alpha;
                    float _Multiply_716B151B_Out;
                    Unity_Multiply_float(_SampleTexture2D_B39DD828_R, _Property_5FDD4914_Out, _Multiply_716B151B_Out);
                    
                    Output1 = (float4(_Branch_54FF636E_Out, 1.0));
                    Output2 = (_Branch_1C4EA1E2_Out.xxxx);
                    Output3 = (_Branch_A5F3B7F9_Out.xxxx);
                    Output4 = (_Multiply_716B151B_Out.xxxx);
                    }
                
                // Pixel Graph Evaluation
                    SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                    {
                        SurfaceDescription surface = (SurfaceDescription)0;
                        float _Property_1E54B66A_Out = _useThreadMap;
                        float4 _Property_8AE14795_Out = _uvThreadMask;
                        float4 _Property_958B7FC9_Out = _uvThreadST;
                        float4 _Subgraph_B567E108_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_958B7FC9_Out, _Property_8AE14795_Out, IN, _Subgraph_B567E108_Output1);
                        float4 _Property_FEDB20A0_Out = _uvBaseMask;
                        float4 _Property_F42AAF3B_Out = _uvBaseST;
                        float4 _Subgraph_9D4E0F1_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_F42AAF3B_Out, _Property_FEDB20A0_Out, IN, _Subgraph_9D4E0F1_Output1);
                        float4 _SampleTexture2D_105B35B3_RGBA = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, (_Subgraph_9D4E0F1_Output1.xy));
                        _SampleTexture2D_105B35B3_RGBA.rgb = UnpackNormalmapRGorAG(_SampleTexture2D_105B35B3_RGBA);
                        float _SampleTexture2D_105B35B3_R = _SampleTexture2D_105B35B3_RGBA.r;
                        float _SampleTexture2D_105B35B3_G = _SampleTexture2D_105B35B3_RGBA.g;
                        float _SampleTexture2D_105B35B3_B = _SampleTexture2D_105B35B3_RGBA.b;
                        float _SampleTexture2D_105B35B3_A = _SampleTexture2D_105B35B3_RGBA.a;
                        float _Property_82D183C3_Out = _NormalMapStrength;
                        float3 _NormalStrength_BFF5C35E_Out;
                        Unity_NormalStrength_float((_SampleTexture2D_105B35B3_RGBA.xyz), _Property_82D183C3_Out, _NormalStrength_BFF5C35E_Out);
                        float3 _Normalize_ACA4E10E_Out;
                        Unity_Normalize_float3(_NormalStrength_BFF5C35E_Out, _Normalize_ACA4E10E_Out);
                        float4 _Property_1E040901_Out = _uvBaseMask;
                        float4 _Property_97A7EF85_Out = _uvBaseST;
                        float4 _Subgraph_8DDCEE61_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_97A7EF85_Out, _Property_1E040901_Out, IN, _Subgraph_8DDCEE61_Output1);
                        float4 _SampleTexture2D_8C3CF01A_RGBA = SAMPLE_TEXTURE2D(_SpecColorMap, sampler_SpecColorMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_8C3CF01A_R = _SampleTexture2D_8C3CF01A_RGBA.r;
                        float _SampleTexture2D_8C3CF01A_G = _SampleTexture2D_8C3CF01A_RGBA.g;
                        float _SampleTexture2D_8C3CF01A_B = _SampleTexture2D_8C3CF01A_RGBA.b;
                        float _SampleTexture2D_8C3CF01A_A = _SampleTexture2D_8C3CF01A_RGBA.a;
                        float _Property_B948927_Out = _SmoothnessMin;
                        float _Property_2962A49E_Out = _SmoothnessMax;
                        float2 _Vector2_9C783D17_Out = float2(_Property_B948927_Out,_Property_2962A49E_Out);
                        float _Remap_10DEF6A_Out;
                        Unity_Remap_float(_SampleTexture2D_8C3CF01A_A, float2 (0,1), _Vector2_9C783D17_Out, _Remap_10DEF6A_Out);
                        float4 _Property_90FAF786_Out = _BaseColor;
                        float4 _SampleTexture2D_11CFD011_RGBA = SAMPLE_TEXTURE2D(_BaseColorMap, sampler_BaseColorMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_11CFD011_R = _SampleTexture2D_11CFD011_RGBA.r;
                        float _SampleTexture2D_11CFD011_G = _SampleTexture2D_11CFD011_RGBA.g;
                        float _SampleTexture2D_11CFD011_B = _SampleTexture2D_11CFD011_RGBA.b;
                        float _SampleTexture2D_11CFD011_A = _SampleTexture2D_11CFD011_RGBA.a;
                        float4 _Multiply_98A7A079_Out;
                        Unity_Multiply_float(_Property_90FAF786_Out, _SampleTexture2D_11CFD011_RGBA, _Multiply_98A7A079_Out);
                    
                        float _Property_7C6435CB_Out = _FuzzMapUVScale;
                        float4 _Multiply_18C3A780_Out;
                        Unity_Multiply_float(_Subgraph_8DDCEE61_Output1, (_Property_7C6435CB_Out.xxxx), _Multiply_18C3A780_Out);
                    
                        float4 _SampleTexture2D_4D82F05E_RGBA = SAMPLE_TEXTURE2D(_FuzzMap, sampler_FuzzMap, (_Multiply_18C3A780_Out.xy));
                        float _SampleTexture2D_4D82F05E_R = _SampleTexture2D_4D82F05E_RGBA.r;
                        float _SampleTexture2D_4D82F05E_G = _SampleTexture2D_4D82F05E_RGBA.g;
                        float _SampleTexture2D_4D82F05E_B = _SampleTexture2D_4D82F05E_RGBA.b;
                        float _SampleTexture2D_4D82F05E_A = _SampleTexture2D_4D82F05E_RGBA.a;
                        float _Property_6CCE2816_Out = _FuzzStrength;
                        float _Lerp_2C953D15_Out;
                        Unity_Lerp_float(0, _SampleTexture2D_4D82F05E_R, _Property_6CCE2816_Out, _Lerp_2C953D15_Out);
                        float4 _Add_A30FF2E2_Out;
                        Unity_Add_float4(_Multiply_98A7A079_Out, (_Lerp_2C953D15_Out.xxxx), _Add_A30FF2E2_Out);
                        float4 _Saturate_69BD2FF3_Out;
                        Unity_Saturate_float4(_Add_A30FF2E2_Out, _Saturate_69BD2FF3_Out);
                        float _Split_EB0B739F_R = _Saturate_69BD2FF3_Out[0];
                        float _Split_EB0B739F_G = _Saturate_69BD2FF3_Out[1];
                        float _Split_EB0B739F_B = _Saturate_69BD2FF3_Out[2];
                        float _Split_EB0B739F_A = _Saturate_69BD2FF3_Out[3];
                        float4 _SampleTexture2D_EECA7933_RGBA = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_EECA7933_R = _SampleTexture2D_EECA7933_RGBA.r;
                        float _SampleTexture2D_EECA7933_G = _SampleTexture2D_EECA7933_RGBA.g;
                        float _SampleTexture2D_EECA7933_B = _SampleTexture2D_EECA7933_RGBA.b;
                        float _SampleTexture2D_EECA7933_A = _SampleTexture2D_EECA7933_RGBA.a;
                        float _Property_88B45C0E_Out = _ThreadAOStrength01;
                        float _Property_FC0CC4C0_Out = _ThreadNormalStrength;
                        float _Property_AC495D22_Out = _ThreadSmoothnessScale;
                        float4 _Subgraph_E494B5B1_Output1;
                        float4 _Subgraph_E494B5B1_Output2;
                        float4 _Subgraph_E494B5B1_Output3;
                        float4 _Subgraph_E494B5B1_Output4;
                        sg_SGRThreadMapDetail_SurfaceDescriptionInputs_64D53B52((_Subgraph_B567E108_Output1.xy), TEXTURE2D_PARAM(_ThreadMap, sampler_ThreadMap), _Property_AC495D22_Out, _SampleTexture2D_EECA7933_G, _Property_1E54B66A_Out, _Property_88B45C0E_Out, _Property_FC0CC4C0_Out, _Remap_10DEF6A_Out, _Normalize_ACA4E10E_Out, _Split_EB0B739F_A, IN, _Subgraph_E494B5B1_Output1, _Subgraph_E494B5B1_Output2, _Subgraph_E494B5B1_Output3, _Subgraph_E494B5B1_Output4);
                        surface.Alpha = (_Subgraph_E494B5B1_Output4).x;
                        return surface;
                    }
                    
            //-------------------------------------------------------------------------------------
            // End graph generated code
            //-------------------------------------------------------------------------------------
        
        
        
        //-------------------------------------------------------------------------------------
        // TEMPLATE INCLUDE : SharedCode.template.hlsl
        //-------------------------------------------------------------------------------------
            FragInputs BuildFragInputs(VaryingsMeshToPS input)
            {
                FragInputs output;
                ZERO_INITIALIZE(FragInputs, output);
        
                // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                // to compute normals which are then passed on elsewhere to compute other values...
                output.worldToTangent = k_identity3x3;
                output.positionSS = input.positionCS;       // input.positionCS is SV_Position
        
                output.texCoord0 = input.texCoord0;
                output.texCoord1 = input.texCoord1;
                output.texCoord2 = input.texCoord2;
                output.texCoord3 = input.texCoord3;
                #if SHADER_STAGE_FRAGMENT
                #endif // SHADER_STAGE_FRAGMENT
        
                return output;
            }
        
            SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
                output.uv0 =                         input.texCoord0;
                output.uv1 =                         input.texCoord1;
                output.uv2 =                         input.texCoord2;
                output.uv3 =                         input.texCoord3;
        
                return output;
            }
        
            // existing HDRP code uses the combined function to go directly from packed to frag inputs
            FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
            {
                VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                return BuildFragInputs(unpacked);
            }
        
        //-------------------------------------------------------------------------------------
        // END TEMPLATE INCLUDE : SharedCode.template.hlsl
        //-------------------------------------------------------------------------------------
        
        
            void ApplyDecalToSurfaceData(DecalSurfaceData decalSurfaceData, inout SurfaceData surfaceData)
            {
                // using alpha compositing https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch23.html
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_DIFFUSE)
                {
                    surfaceData.baseColor.xyz = surfaceData.baseColor.xyz * decalSurfaceData.baseColor.w + decalSurfaceData.baseColor.xyz;
                }
        
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_NORMAL)
                {
                    surfaceData.normalWS.xyz = normalize(surfaceData.normalWS.xyz * decalSurfaceData.normalWS.w + decalSurfaceData.normalWS.xyz);
                }
        
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_MASK)
                {
            #ifdef DECALS_4RT // only smoothness in 3RT mode
                    // Don't apply any metallic modification
                    surfaceData.ambientOcclusion = surfaceData.ambientOcclusion * decalSurfaceData.MAOSBlend.y + decalSurfaceData.mask.y;
            #endif
        
                    surfaceData.perceptualSmoothness = surfaceData.perceptualSmoothness * decalSurfaceData.mask.w + decalSurfaceData.mask.z;
                }
            }
        
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, out SurfaceData surfaceData)
            {
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(SurfaceData, surfaceData);
        
                // copy across graph values, if defined
        
        
        
        
        
        
        
        
        
                
                // These static material feature allow compile time optimization
                surfaceData.materialFeatures = 0;
        
                // Transform the preprocess macro into a material feature (note that silk flag is deduced from the abscence of this one)
                #ifdef _MATERIAL_FEATURE_COTTON_WOOL
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_COTTON_WOOL;
                #endif
        
                #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_SUBSURFACE_SCATTERING;
                #endif
        
                #ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_TRANSMISSION;
                #endif
        
        
        #if defined (_ENERGY_CONSERVING_SPECULAR)
                // Require to have setup baseColor
                // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
        #endif
        
                // tangent-space normal
                float3 normalTS = float3(0.0f, 0.0f, 1.0f);
        
                // compute world space normal
                GetNormalWS(fragInputs, normalTS, surfaceData.normalWS);
        
                surfaceData.geomNormalWS = fragInputs.worldToTangent[2];
        
                surfaceData.tangentWS = normalize(fragInputs.worldToTangent[0].xyz);    // The tangent is not normalize in worldToTangent for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
        
                // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                // If user provide bent normal then we process a better term
                surfaceData.specularOcclusion = 1.0;
        
        #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                // Just use the value passed through via the slot (not active otherwise)
        #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                // If we have bent normal and ambient occlusion, process a specular occlusion
                surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
        #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
        #else
                surfaceData.specularOcclusion = 1.0;
                surfaceData.specularOcclusion = 1.0;
        #endif
        
        #ifdef DEBUG_DISPLAY
                // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                // as it can modify attribute use for static lighting
                ApplyDebugToSurfaceData(fragInputs.worldToTangent, surfaceData);
        #endif
            }
        
            void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
            {
        #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                uint3 fadeMaskSeed = asuint((int3)(V * _ScreenSize.xyx)); // Quantize V to _ScreenSize values
                LODDitheringTransition(fadeMaskSeed, unity_LODFade.x);
        #endif
        
                // this applies the double sided tangent space correction -- see 'ApplyDoubleSidedFlipOrMirror()'
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
        
                BuildSurfaceData(fragInputs, surfaceDescription, V, surfaceData);
        
        #if HAVE_DECALS && _DECALS
                DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, surfaceDescription.Alpha);
                ApplyDecalToSurfaceData(decalSurfaceData, surfaceData);
        #endif
        
                // Builtin Data
                // For back lighting we use the oposite vertex normal 
                InitBuiltinData(surfaceDescription.Alpha, surfaceData.normalWS, -fragInputs.worldToTangent[2], fragInputs.positionRWS, fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
        
        
                builtinData.depthOffset = 0.0;                        // ApplyPerPixelDisplacement(input, V, layerTexCoord, blendMasks); #ifdef _DEPTHOFFSET_ON : ApplyDepthOffsetPositionInput(V, depthOffset, GetWorldToHClipMatrix(), posInput);
        
                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
            }
        
            //-------------------------------------------------------------------------------------
            // Pass Includes
            //-------------------------------------------------------------------------------------
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"
            //-------------------------------------------------------------------------------------
            // End Pass Includes
            //-------------------------------------------------------------------------------------
        
            ENDHLSL
        }
        
        Pass
        {
            // based on FabricPass.template
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
        
            //-------------------------------------------------------------------------------------
            // Render Modes (Blend, Cull, ZTest, Stencil, etc)
            //-------------------------------------------------------------------------------------
                Blend One Zero
        
                Cull Back
        
                ZTest LEqual
        
                ZWrite On
        
                ZClip [_ZClip]
        
                // Default Stencil
        
                ColorMask 0
        
            //-------------------------------------------------------------------------------------
            // End Render Modes
            //-------------------------------------------------------------------------------------
        
            HLSLPROGRAM
        
                #pragma target 4.5
                #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
                //#pragma enable_d3d11_debug_symbols
        
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
        
                #pragma multi_compile _ LOD_FADE_CROSSFADE
        
            //-------------------------------------------------------------------------------------
            // Variant Definitions (active field translations to HDRP defines)
            //-------------------------------------------------------------------------------------
                #define _MATERIAL_FEATURE_COTTON_WOOL 1
                #define _SPECULAR_OCCLUSION_FROM_AO 1
                #define _ENERGY_CONSERVING_SPECULAR 1
                #define _DISABLE_SSR 1
            //-------------------------------------------------------------------------------------
            // End Variant Definitions
            //-------------------------------------------------------------------------------------
        
            #pragma vertex Vert
            #pragma fragment Frag
        
            #define UNITY_MATERIAL_FABRIC      // Need to be define before including Material.hlsl
        
            // This will be enabled in an upcoming change. 
            // #define SURFACE_GRADIENT
        
            // If we use subsurface scattering, enable output split lighting (for forward pass)
            #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
            #define OUTPUT_SPLIT_LIGHTING
            #endif
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Wind.hlsl"
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
        
            // define FragInputs structure
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
        
            //-------------------------------------------------------------------------------------
            // Defines
            //-------------------------------------------------------------------------------------
                #define SHADERPASS SHADERPASS_SHADOWS
                #define USE_LEGACY_UNITY_MATRIX_VARIABLES
        
                // this translates the new dependency tracker into the old preprocessor definitions for the existing HDRP shader code
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_TEXCOORD2
                #define ATTRIBUTES_NEED_TEXCOORD3
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_TEXCOORD2
                #define VARYINGS_NEED_TEXCOORD3
        
            //-------------------------------------------------------------------------------------
            // End Defines
            //-------------------------------------------------------------------------------------
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #ifdef DEBUG_DISPLAY
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif
        
            #if (SHADERPASS == SHADERPASS_FORWARD)
                // used for shaders that want to do lighting (and materials)
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
            #else
                // used for shaders that don't need lighting
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #endif
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
        
            //Used by SceneSelectionPass
            int _ObjectId;
            int _PassValue;
        
            // this function assumes the bitangent flip is encoded in tangentWS.w
            // TODO: move this function to HDRP shared file, once we merge with HDRP repo
            float3x3 BuildWorldToTangent(float4 tangentWS, float3 normalWS)
            {
                // tangentWS must not be normalized (mikkts requirement)
        
                // Normalize normalWS vector but keep the renormFactor to apply it to bitangent and tangent
        	    float3 unnormalizedNormalWS = normalWS;
                float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
                // bitangent on the fly option in xnormal to reduce vertex shader outputs.
        	    // this is the mikktspace transformation (must use unnormalized attributes)
                float3x3 worldToTangent = CreateWorldToTangent(unnormalizedNormalWS, tangentWS.xyz, tangentWS.w > 0.0 ? 1.0 : -1.0);
        
        	    // surface gradient based formulation requires a unit length initial normal. We can maintain compliance with mikkts
        	    // by uniformly scaling all 3 vectors since normalization of the perturbed normal will cancel it.
                worldToTangent[0] = worldToTangent[0] * renormFactor;
                worldToTangent[1] = worldToTangent[1] * renormFactor;
                worldToTangent[2] = worldToTangent[2] * renormFactor;		// normalizes the interpolated vertex normal
                return worldToTangent;
            }
        
            //-------------------------------------------------------------------------------------
            // Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
        // Generated Type: AttributesMesh
        struct AttributesMesh {
            float3 positionOS : POSITION;
            float4 uv0 : TEXCOORD0; // optional
            float4 uv1 : TEXCOORD1; // optional
            float4 uv2 : TEXCOORD2; // optional
            float4 uv3 : TEXCOORD3; // optional
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        
        // Generated Type: VaryingsMeshToPS
        struct VaryingsMeshToPS {
            float4 positionCS : SV_Position;
            float4 texCoord0; // optional
            float4 texCoord1; // optional
            float4 texCoord2; // optional
            float4 texCoord3; // optional
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        struct PackedVaryingsMeshToPS {
            float4 interp00 : TEXCOORD0; // auto-packed
            float4 interp01 : TEXCOORD1; // auto-packed
            float4 interp02 : TEXCOORD2; // auto-packed
            float4 interp03 : TEXCOORD3; // auto-packed
            float4 positionCS : SV_Position; // unpacked
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC; // unpacked
            #endif // INSTANCING_ON
        };
        PackedVaryingsMeshToPS PackVaryingsMeshToPS(VaryingsMeshToPS input)
        {
            PackedVaryingsMeshToPS output;
            output.positionCS = input.positionCS;
            output.interp00.xyzw = input.texCoord0;
            output.interp01.xyzw = input.texCoord1;
            output.interp02.xyzw = input.texCoord2;
            output.interp03.xyzw = input.texCoord3;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        VaryingsMeshToPS UnpackVaryingsMeshToPS(PackedVaryingsMeshToPS input)
        {
            VaryingsMeshToPS output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp00.xyzw;
            output.texCoord1 = input.interp01.xyzw;
            output.texCoord2 = input.interp02.xyzw;
            output.texCoord3 = input.interp03.xyzw;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        
        // Generated Type: VaryingsMeshToDS
        struct VaryingsMeshToDS {
            float3 positionRWS;
            float3 normalWS;
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        struct PackedVaryingsMeshToDS {
            float3 interp00 : TEXCOORD0; // auto-packed
            float3 interp01 : TEXCOORD1; // auto-packed
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC; // unpacked
            #endif // INSTANCING_ON
        };
        PackedVaryingsMeshToDS PackVaryingsMeshToDS(VaryingsMeshToDS input)
        {
            PackedVaryingsMeshToDS output;
            output.interp00.xyz = input.positionRWS;
            output.interp01.xyz = input.normalWS;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        VaryingsMeshToDS UnpackVaryingsMeshToDS(PackedVaryingsMeshToDS input)
        {
            VaryingsMeshToDS output;
            output.positionRWS = input.interp00.xyz;
            output.normalWS = input.interp01.xyz;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        
            //-------------------------------------------------------------------------------------
            // End Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
        
            //-------------------------------------------------------------------------------------
            // Graph generated code
            //-------------------------------------------------------------------------------------
                // Shared Graph Properties (uniform inputs)
                    CBUFFER_START(UnityPerMaterial)
                    float4 _uvBaseMask;
                    float4 _uvBaseST;
                    float4 _BaseColor;
                    float _SmoothnessMin;
                    float _SmoothnessMax;
                    float4 _SpecularColor;
                    float _NormalMapStrength;
                    float _useThreadMap;
                    float4 _uvThreadMask;
                    float4 _uvThreadST;
                    float _ThreadAOStrength01;
                    float _ThreadNormalStrength;
                    float _ThreadSmoothnessScale;
                    float _FuzzMapUVScale;
                    float _FuzzStrength;
                    float4 _EmissionColor;
                    CBUFFER_END
                
                    TEXTURE2D(_BaseColorMap); SAMPLER(sampler_BaseColorMap); float4 _BaseColorMap_TexelSize;
                    TEXTURE2D(_MaskMap); SAMPLER(sampler_MaskMap); float4 _MaskMap_TexelSize;
                    TEXTURE2D(_SpecColorMap); SAMPLER(sampler_SpecColorMap); float4 _SpecColorMap_TexelSize;
                    TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap); float4 _NormalMap_TexelSize;
                    TEXTURE2D(_ThreadMap); SAMPLER(sampler_ThreadMap); float4 _ThreadMap_TexelSize;
                    TEXTURE2D(_FuzzMap); SAMPLER(sampler_FuzzMap); float4 _FuzzMap_TexelSize;
                
                // Pixel Graph Inputs
                    struct SurfaceDescriptionInputs {
                        float4 uv0; // optional
                        float4 uv1; // optional
                        float4 uv2; // optional
                        float4 uv3; // optional
                    };
                // Pixel Graph Outputs
                    struct SurfaceDescription
                    {
                        float Alpha;
                    };
                    
                // Shared Graph Node Functions
                
                    void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
                    {
                        RGBA = float4(R, G, B, A);
                        RGB = float3(R, G, B);
                        RG = float2(R, G);
                    }
                
                    void Unity_Multiply_float (float2 A, float2 B, out float2 Out)
                    {
                        Out = A * B;
                    }
                
                    void Unity_Add_float2(float2 A, float2 B, out float2 Out)
                    {
                        Out = A + B;
                    }
                
                    // Subgraph function
                    void sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(float4 _uvST, float4 _uvMask, SurfaceDescriptionInputs IN, out float4 Output1)
                    {
                    float4 _UV_23AF8552_Out = IN.uv0;
                    float _Split_7957D60_R = _UV_23AF8552_Out[0];
                    float _Split_7957D60_G = _UV_23AF8552_Out[1];
                    float _Split_7957D60_B = _UV_23AF8552_Out[2];
                    float _Split_7957D60_A = _UV_23AF8552_Out[3];
                    float4 _Combine_5396A6C7_RGBA;
                    float3 _Combine_5396A6C7_RGB;
                    float2 _Combine_5396A6C7_RG;
                    Unity_Combine_float(_Split_7957D60_R, _Split_7957D60_G, 0, 0, _Combine_5396A6C7_RGBA, _Combine_5396A6C7_RGB, _Combine_5396A6C7_RG);
                    float4 _Property_CB55E443_Out = _uvMask;
                    float _Split_6086B0A5_R = _Property_CB55E443_Out[0];
                    float _Split_6086B0A5_G = _Property_CB55E443_Out[1];
                    float _Split_6086B0A5_B = _Property_CB55E443_Out[2];
                    float _Split_6086B0A5_A = _Property_CB55E443_Out[3];
                    float2 _Multiply_FC550A07_Out;
                    Unity_Multiply_float(_Combine_5396A6C7_RG, (_Split_6086B0A5_R.xx), _Multiply_FC550A07_Out);
                    
                    float4 _UV_3B1D042C_Out = IN.uv1;
                    float _Split_107320B6_R = _UV_3B1D042C_Out[0];
                    float _Split_107320B6_G = _UV_3B1D042C_Out[1];
                    float _Split_107320B6_B = _UV_3B1D042C_Out[2];
                    float _Split_107320B6_A = _UV_3B1D042C_Out[3];
                    float4 _Combine_2E8D3795_RGBA;
                    float3 _Combine_2E8D3795_RGB;
                    float2 _Combine_2E8D3795_RG;
                    Unity_Combine_float(_Split_107320B6_R, _Split_107320B6_G, 0, 0, _Combine_2E8D3795_RGBA, _Combine_2E8D3795_RGB, _Combine_2E8D3795_RG);
                    float2 _Multiply_FDA7BA1E_Out;
                    Unity_Multiply_float(_Combine_2E8D3795_RG, (_Split_6086B0A5_G.xx), _Multiply_FDA7BA1E_Out);
                    
                    float2 _Add_92015245_Out;
                    Unity_Add_float2(_Multiply_FC550A07_Out, _Multiply_FDA7BA1E_Out, _Add_92015245_Out);
                    float4 _UV_49BE4158_Out = IN.uv2;
                    float _Split_A24186AD_R = _UV_49BE4158_Out[0];
                    float _Split_A24186AD_G = _UV_49BE4158_Out[1];
                    float _Split_A24186AD_B = _UV_49BE4158_Out[2];
                    float _Split_A24186AD_A = _UV_49BE4158_Out[3];
                    float4 _Combine_6951B6BC_RGBA;
                    float3 _Combine_6951B6BC_RGB;
                    float2 _Combine_6951B6BC_RG;
                    Unity_Combine_float(_Split_A24186AD_R, _Split_A24186AD_G, 0, 0, _Combine_6951B6BC_RGBA, _Combine_6951B6BC_RGB, _Combine_6951B6BC_RG);
                    float2 _Multiply_1480B81_Out;
                    Unity_Multiply_float(_Combine_6951B6BC_RG, (_Split_6086B0A5_B.xx), _Multiply_1480B81_Out);
                    
                    float4 _UV_9CA65C2_Out = IN.uv3;
                    float _Split_9EC6EA10_R = _UV_9CA65C2_Out[0];
                    float _Split_9EC6EA10_G = _UV_9CA65C2_Out[1];
                    float _Split_9EC6EA10_B = _UV_9CA65C2_Out[2];
                    float _Split_9EC6EA10_A = _UV_9CA65C2_Out[3];
                    float4 _Combine_633F7D3D_RGBA;
                    float3 _Combine_633F7D3D_RGB;
                    float2 _Combine_633F7D3D_RG;
                    Unity_Combine_float(_Split_9EC6EA10_R, _Split_9EC6EA10_G, 0, 0, _Combine_633F7D3D_RGBA, _Combine_633F7D3D_RGB, _Combine_633F7D3D_RG);
                    float2 _Multiply_2A2B5227_Out;
                    Unity_Multiply_float(_Combine_633F7D3D_RG, (_Split_6086B0A5_A.xx), _Multiply_2A2B5227_Out);
                    
                    float2 _Add_B5E7679D_Out;
                    Unity_Add_float2(_Multiply_1480B81_Out, _Multiply_2A2B5227_Out, _Add_B5E7679D_Out);
                    float2 _Add_892742E3_Out;
                    Unity_Add_float2(_Add_92015245_Out, _Add_B5E7679D_Out, _Add_892742E3_Out);
                    float4 _Property_8DA1B077_Out = _uvST;
                    float _Split_1AB0DA31_R = _Property_8DA1B077_Out[0];
                    float _Split_1AB0DA31_G = _Property_8DA1B077_Out[1];
                    float _Split_1AB0DA31_B = _Property_8DA1B077_Out[2];
                    float _Split_1AB0DA31_A = _Property_8DA1B077_Out[3];
                    float4 _Combine_44459F1_RGBA;
                    float3 _Combine_44459F1_RGB;
                    float2 _Combine_44459F1_RG;
                    Unity_Combine_float(_Split_1AB0DA31_R, _Split_1AB0DA31_G, 0, 0, _Combine_44459F1_RGBA, _Combine_44459F1_RGB, _Combine_44459F1_RG);
                    float2 _Multiply_38815E23_Out;
                    Unity_Multiply_float(_Add_892742E3_Out, _Combine_44459F1_RG, _Multiply_38815E23_Out);
                    
                    float _Split_35A1DC4_R = _Property_8DA1B077_Out[0];
                    float _Split_35A1DC4_G = _Property_8DA1B077_Out[1];
                    float _Split_35A1DC4_B = _Property_8DA1B077_Out[2];
                    float _Split_35A1DC4_A = _Property_8DA1B077_Out[3];
                    float4 _Combine_91984BDF_RGBA;
                    float3 _Combine_91984BDF_RGB;
                    float2 _Combine_91984BDF_RG;
                    Unity_Combine_float(_Split_35A1DC4_B, _Split_35A1DC4_A, 0, 0, _Combine_91984BDF_RGBA, _Combine_91984BDF_RGB, _Combine_91984BDF_RG);
                    float2 _Add_63012CEE_Out;
                    Unity_Add_float2(_Multiply_38815E23_Out, _Combine_91984BDF_RG, _Add_63012CEE_Out);
                    Output1 = (float4(_Add_63012CEE_Out, 0.0, 1.0));
                    }
                
                    void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
                    {
                        Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
                    }
                
                    void Unity_Normalize_float3(float3 In, out float3 Out)
                    {
                        Out = normalize(In);
                    }
                
                    void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
                    {
                        Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
                    }
                
                    void Unity_Multiply_float (float4 A, float4 B, out float4 Out)
                    {
                        Out = A * B;
                    }
                
                    void Unity_Lerp_float(float A, float B, float T, out float Out)
                    {
                        Out = lerp(A, B, T);
                    }
                
                    void Unity_Add_float4(float4 A, float4 B, out float4 Out)
                    {
                        Out = A + B;
                    }
                
                    void Unity_Saturate_float4(float4 In, out float4 Out)
                    {
                        Out = saturate(In);
                    }
                
                    void Unity_NormalUnpack_float(float4 In, out float3 Out)
                    {
                                    Out = UnpackNormalmapRGorAG(In);
                                }
                
                    void Unity_NormalBlend_float(float3 A, float3 B, out float3 Out)
                    {
                        Out = normalize(float3(A.rg + B.rg, A.b * B.b));
                    }
                
                    void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
                    {
                        Out = lerp(False, True, Predicate);
                    }
                
                    void Unity_Add_float(float A, float B, out float Out)
                    {
                        Out = A + B;
                    }
                
                    void Unity_Saturate_float(float In, out float Out)
                    {
                        Out = saturate(In);
                    }
                
                    void Unity_Branch_float(float Predicate, float True, float False, out float Out)
                    {
                        Out = lerp(False, True, Predicate);
                    }
                
                    void Unity_Multiply_float (float A, float B, out float Out)
                    {
                        Out = A * B;
                    }
                
                    // Subgraph function
                    void sg_SGRThreadMapDetail_SurfaceDescriptionInputs_64D53B52(float2 _UV, TEXTURE2D_ARGS(_ThreadMap, sampler_ThreadMap), float _ThreadSmoothnessStrength, float _AmbientOcclusion, float _UseThreadMap, float _ThreadAOStrength, float _ThreadNormalStrength, float _Smoothness, float3 _Normals, float _Alpha, SurfaceDescriptionInputs IN, out float4 Output1, out float4 Output2, out float4 Output3, out float4 Output4)
                    {
                    float _Property_7B789410_Out = _UseThreadMap;
                    float3 _Property_D380C535_Out = _Normals;
                    float2 _Property_247E83DC_Out = _UV;
                    float4 _SampleTexture2D_B39DD828_RGBA = SAMPLE_TEXTURE2D(_ThreadMap, sampler_ThreadMap, _Property_247E83DC_Out);
                    float _SampleTexture2D_B39DD828_R = _SampleTexture2D_B39DD828_RGBA.r;
                    float _SampleTexture2D_B39DD828_G = _SampleTexture2D_B39DD828_RGBA.g;
                    float _SampleTexture2D_B39DD828_B = _SampleTexture2D_B39DD828_RGBA.b;
                    float _SampleTexture2D_B39DD828_A = _SampleTexture2D_B39DD828_RGBA.a;
                    float4 _Combine_3989CE7_RGBA;
                    float3 _Combine_3989CE7_RGB;
                    float2 _Combine_3989CE7_RG;
                    Unity_Combine_float(_SampleTexture2D_B39DD828_A, _SampleTexture2D_B39DD828_G, 1, 1, _Combine_3989CE7_RGBA, _Combine_3989CE7_RGB, _Combine_3989CE7_RG);
                    float3 _NormalUnpack_6B39F6EC_Out;
                    Unity_NormalUnpack_float((float4(_Combine_3989CE7_RGB, 1.0)), _NormalUnpack_6B39F6EC_Out);
                    float3 _Normalize_1F52E5EC_Out;
                    Unity_Normalize_float3(_NormalUnpack_6B39F6EC_Out, _Normalize_1F52E5EC_Out);
                    float _Property_2E175598_Out = _ThreadNormalStrength;
                    float3 _NormalStrength_A15875A3_Out;
                    Unity_NormalStrength_float(_Normalize_1F52E5EC_Out, _Property_2E175598_Out, _NormalStrength_A15875A3_Out);
                    float3 _NormalBlend_191D51BE_Out;
                    Unity_NormalBlend_float(_Property_D380C535_Out, _NormalStrength_A15875A3_Out, _NormalBlend_191D51BE_Out);
                    float3 _Normalize_4D9B04E_Out;
                    Unity_Normalize_float3(_NormalBlend_191D51BE_Out, _Normalize_4D9B04E_Out);
                    float3 _Branch_54FF636E_Out;
                    Unity_Branch_float3(_Property_7B789410_Out, _Normalize_4D9B04E_Out, _Property_D380C535_Out, _Branch_54FF636E_Out);
                    float _Property_B5560A97_Out = _UseThreadMap;
                    float _Property_6FAEC412_Out = _Smoothness;
                    float _Remap_C272A01C_Out;
                    Unity_Remap_float(_SampleTexture2D_B39DD828_B, float2 (0,1), float2 (-1,1), _Remap_C272A01C_Out);
                    float _Property_CF380DCA_Out = _ThreadSmoothnessStrength;
                    float _Lerp_1EB6EBC0_Out;
                    Unity_Lerp_float(0, _Remap_C272A01C_Out, _Property_CF380DCA_Out, _Lerp_1EB6EBC0_Out);
                    float _Add_2975BB_Out;
                    Unity_Add_float(_Property_6FAEC412_Out, _Lerp_1EB6EBC0_Out, _Add_2975BB_Out);
                    float _Saturate_1F46047D_Out;
                    Unity_Saturate_float(_Add_2975BB_Out, _Saturate_1F46047D_Out);
                    float _Branch_1C4EA1E2_Out;
                    Unity_Branch_float(_Property_B5560A97_Out, _Saturate_1F46047D_Out, _Property_6FAEC412_Out, _Branch_1C4EA1E2_Out);
                    float _Property_57F076E2_Out = _UseThreadMap;
                    float _Property_829FEB4F_Out = _ThreadAOStrength;
                    float _Lerp_1DC743E3_Out;
                    Unity_Lerp_float(1, _SampleTexture2D_B39DD828_R, _Property_829FEB4F_Out, _Lerp_1DC743E3_Out);
                    float _Property_416E73AE_Out = _AmbientOcclusion;
                    float _Multiply_FBD87ACD_Out;
                    Unity_Multiply_float(_Lerp_1DC743E3_Out, _Property_416E73AE_Out, _Multiply_FBD87ACD_Out);
                    
                    float _Branch_A5F3B7F9_Out;
                    Unity_Branch_float(_Property_57F076E2_Out, _Multiply_FBD87ACD_Out, _Property_416E73AE_Out, _Branch_A5F3B7F9_Out);
                    float _Property_5FDD4914_Out = _Alpha;
                    float _Multiply_716B151B_Out;
                    Unity_Multiply_float(_SampleTexture2D_B39DD828_R, _Property_5FDD4914_Out, _Multiply_716B151B_Out);
                    
                    Output1 = (float4(_Branch_54FF636E_Out, 1.0));
                    Output2 = (_Branch_1C4EA1E2_Out.xxxx);
                    Output3 = (_Branch_A5F3B7F9_Out.xxxx);
                    Output4 = (_Multiply_716B151B_Out.xxxx);
                    }
                
                // Pixel Graph Evaluation
                    SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                    {
                        SurfaceDescription surface = (SurfaceDescription)0;
                        float _Property_1E54B66A_Out = _useThreadMap;
                        float4 _Property_8AE14795_Out = _uvThreadMask;
                        float4 _Property_958B7FC9_Out = _uvThreadST;
                        float4 _Subgraph_B567E108_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_958B7FC9_Out, _Property_8AE14795_Out, IN, _Subgraph_B567E108_Output1);
                        float4 _Property_FEDB20A0_Out = _uvBaseMask;
                        float4 _Property_F42AAF3B_Out = _uvBaseST;
                        float4 _Subgraph_9D4E0F1_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_F42AAF3B_Out, _Property_FEDB20A0_Out, IN, _Subgraph_9D4E0F1_Output1);
                        float4 _SampleTexture2D_105B35B3_RGBA = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, (_Subgraph_9D4E0F1_Output1.xy));
                        _SampleTexture2D_105B35B3_RGBA.rgb = UnpackNormalmapRGorAG(_SampleTexture2D_105B35B3_RGBA);
                        float _SampleTexture2D_105B35B3_R = _SampleTexture2D_105B35B3_RGBA.r;
                        float _SampleTexture2D_105B35B3_G = _SampleTexture2D_105B35B3_RGBA.g;
                        float _SampleTexture2D_105B35B3_B = _SampleTexture2D_105B35B3_RGBA.b;
                        float _SampleTexture2D_105B35B3_A = _SampleTexture2D_105B35B3_RGBA.a;
                        float _Property_82D183C3_Out = _NormalMapStrength;
                        float3 _NormalStrength_BFF5C35E_Out;
                        Unity_NormalStrength_float((_SampleTexture2D_105B35B3_RGBA.xyz), _Property_82D183C3_Out, _NormalStrength_BFF5C35E_Out);
                        float3 _Normalize_ACA4E10E_Out;
                        Unity_Normalize_float3(_NormalStrength_BFF5C35E_Out, _Normalize_ACA4E10E_Out);
                        float4 _Property_1E040901_Out = _uvBaseMask;
                        float4 _Property_97A7EF85_Out = _uvBaseST;
                        float4 _Subgraph_8DDCEE61_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_97A7EF85_Out, _Property_1E040901_Out, IN, _Subgraph_8DDCEE61_Output1);
                        float4 _SampleTexture2D_8C3CF01A_RGBA = SAMPLE_TEXTURE2D(_SpecColorMap, sampler_SpecColorMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_8C3CF01A_R = _SampleTexture2D_8C3CF01A_RGBA.r;
                        float _SampleTexture2D_8C3CF01A_G = _SampleTexture2D_8C3CF01A_RGBA.g;
                        float _SampleTexture2D_8C3CF01A_B = _SampleTexture2D_8C3CF01A_RGBA.b;
                        float _SampleTexture2D_8C3CF01A_A = _SampleTexture2D_8C3CF01A_RGBA.a;
                        float _Property_B948927_Out = _SmoothnessMin;
                        float _Property_2962A49E_Out = _SmoothnessMax;
                        float2 _Vector2_9C783D17_Out = float2(_Property_B948927_Out,_Property_2962A49E_Out);
                        float _Remap_10DEF6A_Out;
                        Unity_Remap_float(_SampleTexture2D_8C3CF01A_A, float2 (0,1), _Vector2_9C783D17_Out, _Remap_10DEF6A_Out);
                        float4 _Property_90FAF786_Out = _BaseColor;
                        float4 _SampleTexture2D_11CFD011_RGBA = SAMPLE_TEXTURE2D(_BaseColorMap, sampler_BaseColorMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_11CFD011_R = _SampleTexture2D_11CFD011_RGBA.r;
                        float _SampleTexture2D_11CFD011_G = _SampleTexture2D_11CFD011_RGBA.g;
                        float _SampleTexture2D_11CFD011_B = _SampleTexture2D_11CFD011_RGBA.b;
                        float _SampleTexture2D_11CFD011_A = _SampleTexture2D_11CFD011_RGBA.a;
                        float4 _Multiply_98A7A079_Out;
                        Unity_Multiply_float(_Property_90FAF786_Out, _SampleTexture2D_11CFD011_RGBA, _Multiply_98A7A079_Out);
                    
                        float _Property_7C6435CB_Out = _FuzzMapUVScale;
                        float4 _Multiply_18C3A780_Out;
                        Unity_Multiply_float(_Subgraph_8DDCEE61_Output1, (_Property_7C6435CB_Out.xxxx), _Multiply_18C3A780_Out);
                    
                        float4 _SampleTexture2D_4D82F05E_RGBA = SAMPLE_TEXTURE2D(_FuzzMap, sampler_FuzzMap, (_Multiply_18C3A780_Out.xy));
                        float _SampleTexture2D_4D82F05E_R = _SampleTexture2D_4D82F05E_RGBA.r;
                        float _SampleTexture2D_4D82F05E_G = _SampleTexture2D_4D82F05E_RGBA.g;
                        float _SampleTexture2D_4D82F05E_B = _SampleTexture2D_4D82F05E_RGBA.b;
                        float _SampleTexture2D_4D82F05E_A = _SampleTexture2D_4D82F05E_RGBA.a;
                        float _Property_6CCE2816_Out = _FuzzStrength;
                        float _Lerp_2C953D15_Out;
                        Unity_Lerp_float(0, _SampleTexture2D_4D82F05E_R, _Property_6CCE2816_Out, _Lerp_2C953D15_Out);
                        float4 _Add_A30FF2E2_Out;
                        Unity_Add_float4(_Multiply_98A7A079_Out, (_Lerp_2C953D15_Out.xxxx), _Add_A30FF2E2_Out);
                        float4 _Saturate_69BD2FF3_Out;
                        Unity_Saturate_float4(_Add_A30FF2E2_Out, _Saturate_69BD2FF3_Out);
                        float _Split_EB0B739F_R = _Saturate_69BD2FF3_Out[0];
                        float _Split_EB0B739F_G = _Saturate_69BD2FF3_Out[1];
                        float _Split_EB0B739F_B = _Saturate_69BD2FF3_Out[2];
                        float _Split_EB0B739F_A = _Saturate_69BD2FF3_Out[3];
                        float4 _SampleTexture2D_EECA7933_RGBA = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_EECA7933_R = _SampleTexture2D_EECA7933_RGBA.r;
                        float _SampleTexture2D_EECA7933_G = _SampleTexture2D_EECA7933_RGBA.g;
                        float _SampleTexture2D_EECA7933_B = _SampleTexture2D_EECA7933_RGBA.b;
                        float _SampleTexture2D_EECA7933_A = _SampleTexture2D_EECA7933_RGBA.a;
                        float _Property_88B45C0E_Out = _ThreadAOStrength01;
                        float _Property_FC0CC4C0_Out = _ThreadNormalStrength;
                        float _Property_AC495D22_Out = _ThreadSmoothnessScale;
                        float4 _Subgraph_E494B5B1_Output1;
                        float4 _Subgraph_E494B5B1_Output2;
                        float4 _Subgraph_E494B5B1_Output3;
                        float4 _Subgraph_E494B5B1_Output4;
                        sg_SGRThreadMapDetail_SurfaceDescriptionInputs_64D53B52((_Subgraph_B567E108_Output1.xy), TEXTURE2D_PARAM(_ThreadMap, sampler_ThreadMap), _Property_AC495D22_Out, _SampleTexture2D_EECA7933_G, _Property_1E54B66A_Out, _Property_88B45C0E_Out, _Property_FC0CC4C0_Out, _Remap_10DEF6A_Out, _Normalize_ACA4E10E_Out, _Split_EB0B739F_A, IN, _Subgraph_E494B5B1_Output1, _Subgraph_E494B5B1_Output2, _Subgraph_E494B5B1_Output3, _Subgraph_E494B5B1_Output4);
                        surface.Alpha = (_Subgraph_E494B5B1_Output4).x;
                        return surface;
                    }
                    
            //-------------------------------------------------------------------------------------
            // End graph generated code
            //-------------------------------------------------------------------------------------
        
        
        
        //-------------------------------------------------------------------------------------
        // TEMPLATE INCLUDE : SharedCode.template.hlsl
        //-------------------------------------------------------------------------------------
            FragInputs BuildFragInputs(VaryingsMeshToPS input)
            {
                FragInputs output;
                ZERO_INITIALIZE(FragInputs, output);
        
                // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                // to compute normals which are then passed on elsewhere to compute other values...
                output.worldToTangent = k_identity3x3;
                output.positionSS = input.positionCS;       // input.positionCS is SV_Position
        
                output.texCoord0 = input.texCoord0;
                output.texCoord1 = input.texCoord1;
                output.texCoord2 = input.texCoord2;
                output.texCoord3 = input.texCoord3;
                #if SHADER_STAGE_FRAGMENT
                #endif // SHADER_STAGE_FRAGMENT
        
                return output;
            }
        
            SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
                output.uv0 =                         input.texCoord0;
                output.uv1 =                         input.texCoord1;
                output.uv2 =                         input.texCoord2;
                output.uv3 =                         input.texCoord3;
        
                return output;
            }
        
            // existing HDRP code uses the combined function to go directly from packed to frag inputs
            FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
            {
                VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                return BuildFragInputs(unpacked);
            }
        
        //-------------------------------------------------------------------------------------
        // END TEMPLATE INCLUDE : SharedCode.template.hlsl
        //-------------------------------------------------------------------------------------
        
        
            void ApplyDecalToSurfaceData(DecalSurfaceData decalSurfaceData, inout SurfaceData surfaceData)
            {
                // using alpha compositing https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch23.html
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_DIFFUSE)
                {
                    surfaceData.baseColor.xyz = surfaceData.baseColor.xyz * decalSurfaceData.baseColor.w + decalSurfaceData.baseColor.xyz;
                }
        
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_NORMAL)
                {
                    surfaceData.normalWS.xyz = normalize(surfaceData.normalWS.xyz * decalSurfaceData.normalWS.w + decalSurfaceData.normalWS.xyz);
                }
        
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_MASK)
                {
            #ifdef DECALS_4RT // only smoothness in 3RT mode
                    // Don't apply any metallic modification
                    surfaceData.ambientOcclusion = surfaceData.ambientOcclusion * decalSurfaceData.MAOSBlend.y + decalSurfaceData.mask.y;
            #endif
        
                    surfaceData.perceptualSmoothness = surfaceData.perceptualSmoothness * decalSurfaceData.mask.w + decalSurfaceData.mask.z;
                }
            }
        
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, out SurfaceData surfaceData)
            {
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(SurfaceData, surfaceData);
        
                // copy across graph values, if defined
        
        
        
        
        
        
        
        
        
                
                // These static material feature allow compile time optimization
                surfaceData.materialFeatures = 0;
        
                // Transform the preprocess macro into a material feature (note that silk flag is deduced from the abscence of this one)
                #ifdef _MATERIAL_FEATURE_COTTON_WOOL
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_COTTON_WOOL;
                #endif
        
                #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_SUBSURFACE_SCATTERING;
                #endif
        
                #ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_TRANSMISSION;
                #endif
        
        
        #if defined (_ENERGY_CONSERVING_SPECULAR)
                // Require to have setup baseColor
                // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
        #endif
        
                // tangent-space normal
                float3 normalTS = float3(0.0f, 0.0f, 1.0f);
        
                // compute world space normal
                GetNormalWS(fragInputs, normalTS, surfaceData.normalWS);
        
                surfaceData.geomNormalWS = fragInputs.worldToTangent[2];
        
                surfaceData.tangentWS = normalize(fragInputs.worldToTangent[0].xyz);    // The tangent is not normalize in worldToTangent for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
        
                // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                // If user provide bent normal then we process a better term
                surfaceData.specularOcclusion = 1.0;
        
        #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                // Just use the value passed through via the slot (not active otherwise)
        #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                // If we have bent normal and ambient occlusion, process a specular occlusion
                surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
        #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
        #else
                surfaceData.specularOcclusion = 1.0;
                surfaceData.specularOcclusion = 1.0;
        #endif
        
        #ifdef DEBUG_DISPLAY
                // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                // as it can modify attribute use for static lighting
                ApplyDebugToSurfaceData(fragInputs.worldToTangent, surfaceData);
        #endif
            }
        
            void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
            {
        #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                uint3 fadeMaskSeed = asuint((int3)(V * _ScreenSize.xyx)); // Quantize V to _ScreenSize values
                LODDitheringTransition(fadeMaskSeed, unity_LODFade.x);
        #endif
        
                // this applies the double sided tangent space correction -- see 'ApplyDoubleSidedFlipOrMirror()'
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
        
                BuildSurfaceData(fragInputs, surfaceDescription, V, surfaceData);
        
        #if HAVE_DECALS && _DECALS
                DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, surfaceDescription.Alpha);
                ApplyDecalToSurfaceData(decalSurfaceData, surfaceData);
        #endif
        
                // Builtin Data
                // For back lighting we use the oposite vertex normal 
                InitBuiltinData(surfaceDescription.Alpha, surfaceData.normalWS, -fragInputs.worldToTangent[2], fragInputs.positionRWS, fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
        
        
                builtinData.depthOffset = 0.0;                        // ApplyPerPixelDisplacement(input, V, layerTexCoord, blendMasks); #ifdef _DEPTHOFFSET_ON : ApplyDepthOffsetPositionInput(V, depthOffset, GetWorldToHClipMatrix(), posInput);
        
                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
            }
        
            //-------------------------------------------------------------------------------------
            // Pass Includes
            //-------------------------------------------------------------------------------------
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"
            //-------------------------------------------------------------------------------------
            // End Pass Includes
            //-------------------------------------------------------------------------------------
        
            ENDHLSL
        }
        
        Pass
        {
            // based on FabricPass.template
            Name "Depth prepass"
            Tags { "LightMode" = "DepthForwardOnly" }
        
            //-------------------------------------------------------------------------------------
            // Render Modes (Blend, Cull, ZTest, Stencil, etc)
            //-------------------------------------------------------------------------------------
                Blend One Zero
        
                Cull Back
        
                ZTest LEqual
        
                ZWrite On
        
                ZClip [_ZClip]
        
                Stencil
        {
           WriteMask 16
           Ref 16
           Comp Always
           Pass Replace
        }
        
                
            //-------------------------------------------------------------------------------------
            // End Render Modes
            //-------------------------------------------------------------------------------------
        
            HLSLPROGRAM
        
                #pragma target 4.5
                #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
                //#pragma enable_d3d11_debug_symbols
        
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
        
                #pragma multi_compile _ LOD_FADE_CROSSFADE
        
            //-------------------------------------------------------------------------------------
            // Variant Definitions (active field translations to HDRP defines)
            //-------------------------------------------------------------------------------------
                #define _MATERIAL_FEATURE_COTTON_WOOL 1
                #define _SPECULAR_OCCLUSION_FROM_AO 1
                #define _ENERGY_CONSERVING_SPECULAR 1
                #define _DISABLE_SSR 1
            //-------------------------------------------------------------------------------------
            // End Variant Definitions
            //-------------------------------------------------------------------------------------
        
            #pragma vertex Vert
            #pragma fragment Frag
        
            #define UNITY_MATERIAL_FABRIC      // Need to be define before including Material.hlsl
        
            // This will be enabled in an upcoming change. 
            // #define SURFACE_GRADIENT
        
            // If we use subsurface scattering, enable output split lighting (for forward pass)
            #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
            #define OUTPUT_SPLIT_LIGHTING
            #endif
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Wind.hlsl"
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
        
            // define FragInputs structure
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
        
            //-------------------------------------------------------------------------------------
            // Defines
            //-------------------------------------------------------------------------------------
                #define SHADERPASS SHADERPASS_DEPTH_ONLY
                #define WRITE_NORMAL_BUFFER
                #pragma multi_compile _ WRITE_MSAA_DEPTH
        
                // this translates the new dependency tracker into the old preprocessor definitions for the existing HDRP shader code
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_TEXCOORD2
                #define ATTRIBUTES_NEED_TEXCOORD3
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_POSITION_WS
                #define VARYINGS_NEED_TANGENT_TO_WORLD
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_TEXCOORD2
                #define VARYINGS_NEED_TEXCOORD3
                #define VARYINGS_NEED_COLOR
        
            //-------------------------------------------------------------------------------------
            // End Defines
            //-------------------------------------------------------------------------------------
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #ifdef DEBUG_DISPLAY
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif
        
            #if (SHADERPASS == SHADERPASS_FORWARD)
                // used for shaders that want to do lighting (and materials)
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
            #else
                // used for shaders that don't need lighting
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #endif
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
        
            //Used by SceneSelectionPass
            int _ObjectId;
            int _PassValue;
        
            // this function assumes the bitangent flip is encoded in tangentWS.w
            // TODO: move this function to HDRP shared file, once we merge with HDRP repo
            float3x3 BuildWorldToTangent(float4 tangentWS, float3 normalWS)
            {
                // tangentWS must not be normalized (mikkts requirement)
        
                // Normalize normalWS vector but keep the renormFactor to apply it to bitangent and tangent
        	    float3 unnormalizedNormalWS = normalWS;
                float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
                // bitangent on the fly option in xnormal to reduce vertex shader outputs.
        	    // this is the mikktspace transformation (must use unnormalized attributes)
                float3x3 worldToTangent = CreateWorldToTangent(unnormalizedNormalWS, tangentWS.xyz, tangentWS.w > 0.0 ? 1.0 : -1.0);
        
        	    // surface gradient based formulation requires a unit length initial normal. We can maintain compliance with mikkts
        	    // by uniformly scaling all 3 vectors since normalization of the perturbed normal will cancel it.
                worldToTangent[0] = worldToTangent[0] * renormFactor;
                worldToTangent[1] = worldToTangent[1] * renormFactor;
                worldToTangent[2] = worldToTangent[2] * renormFactor;		// normalizes the interpolated vertex normal
                return worldToTangent;
            }
        
            //-------------------------------------------------------------------------------------
            // Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
        // Generated Type: AttributesMesh
        struct AttributesMesh {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL; // optional
            float4 tangentOS : TANGENT; // optional
            float4 uv0 : TEXCOORD0; // optional
            float4 uv1 : TEXCOORD1; // optional
            float4 uv2 : TEXCOORD2; // optional
            float4 uv3 : TEXCOORD3; // optional
            float4 color : COLOR; // optional
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        
        // Generated Type: VaryingsMeshToPS
        struct VaryingsMeshToPS {
            float4 positionCS : SV_Position;
            float3 positionRWS; // optional
            float3 normalWS; // optional
            float4 tangentWS; // optional
            float4 texCoord0; // optional
            float4 texCoord1; // optional
            float4 texCoord2; // optional
            float4 texCoord3; // optional
            float4 color; // optional
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        struct PackedVaryingsMeshToPS {
            float3 interp00 : TEXCOORD0; // auto-packed
            float3 interp01 : TEXCOORD1; // auto-packed
            float4 interp02 : TEXCOORD2; // auto-packed
            float4 interp03 : TEXCOORD3; // auto-packed
            float4 interp04 : TEXCOORD4; // auto-packed
            float4 interp05 : TEXCOORD5; // auto-packed
            float4 interp06 : TEXCOORD6; // auto-packed
            float4 interp07 : TEXCOORD7; // auto-packed
            float4 positionCS : SV_Position; // unpacked
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC; // unpacked
            #endif // INSTANCING_ON
        };
        PackedVaryingsMeshToPS PackVaryingsMeshToPS(VaryingsMeshToPS input)
        {
            PackedVaryingsMeshToPS output;
            output.positionCS = input.positionCS;
            output.interp00.xyz = input.positionRWS;
            output.interp01.xyz = input.normalWS;
            output.interp02.xyzw = input.tangentWS;
            output.interp03.xyzw = input.texCoord0;
            output.interp04.xyzw = input.texCoord1;
            output.interp05.xyzw = input.texCoord2;
            output.interp06.xyzw = input.texCoord3;
            output.interp07.xyzw = input.color;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        VaryingsMeshToPS UnpackVaryingsMeshToPS(PackedVaryingsMeshToPS input)
        {
            VaryingsMeshToPS output;
            output.positionCS = input.positionCS;
            output.positionRWS = input.interp00.xyz;
            output.normalWS = input.interp01.xyz;
            output.tangentWS = input.interp02.xyzw;
            output.texCoord0 = input.interp03.xyzw;
            output.texCoord1 = input.interp04.xyzw;
            output.texCoord2 = input.interp05.xyzw;
            output.texCoord3 = input.interp06.xyzw;
            output.color = input.interp07.xyzw;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        
        // Generated Type: VaryingsMeshToDS
        struct VaryingsMeshToDS {
            float3 positionRWS;
            float3 normalWS;
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        struct PackedVaryingsMeshToDS {
            float3 interp00 : TEXCOORD0; // auto-packed
            float3 interp01 : TEXCOORD1; // auto-packed
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC; // unpacked
            #endif // INSTANCING_ON
        };
        PackedVaryingsMeshToDS PackVaryingsMeshToDS(VaryingsMeshToDS input)
        {
            PackedVaryingsMeshToDS output;
            output.interp00.xyz = input.positionRWS;
            output.interp01.xyz = input.normalWS;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        VaryingsMeshToDS UnpackVaryingsMeshToDS(PackedVaryingsMeshToDS input)
        {
            VaryingsMeshToDS output;
            output.positionRWS = input.interp00.xyz;
            output.normalWS = input.interp01.xyz;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        
            //-------------------------------------------------------------------------------------
            // End Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
        
            //-------------------------------------------------------------------------------------
            // Graph generated code
            //-------------------------------------------------------------------------------------
                // Shared Graph Properties (uniform inputs)
                    CBUFFER_START(UnityPerMaterial)
                    float4 _uvBaseMask;
                    float4 _uvBaseST;
                    float4 _BaseColor;
                    float _SmoothnessMin;
                    float _SmoothnessMax;
                    float4 _SpecularColor;
                    float _NormalMapStrength;
                    float _useThreadMap;
                    float4 _uvThreadMask;
                    float4 _uvThreadST;
                    float _ThreadAOStrength01;
                    float _ThreadNormalStrength;
                    float _ThreadSmoothnessScale;
                    float _FuzzMapUVScale;
                    float _FuzzStrength;
                    float4 _EmissionColor;
                    CBUFFER_END
                
                    TEXTURE2D(_BaseColorMap); SAMPLER(sampler_BaseColorMap); float4 _BaseColorMap_TexelSize;
                    TEXTURE2D(_MaskMap); SAMPLER(sampler_MaskMap); float4 _MaskMap_TexelSize;
                    TEXTURE2D(_SpecColorMap); SAMPLER(sampler_SpecColorMap); float4 _SpecColorMap_TexelSize;
                    TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap); float4 _NormalMap_TexelSize;
                    TEXTURE2D(_ThreadMap); SAMPLER(sampler_ThreadMap); float4 _ThreadMap_TexelSize;
                    TEXTURE2D(_FuzzMap); SAMPLER(sampler_FuzzMap); float4 _FuzzMap_TexelSize;
                
                // Pixel Graph Inputs
                    struct SurfaceDescriptionInputs {
                        float4 uv0; // optional
                        float4 uv1; // optional
                        float4 uv2; // optional
                        float4 uv3; // optional
                    };
                // Pixel Graph Outputs
                    struct SurfaceDescription
                    {
                        float Alpha;
                    };
                    
                // Shared Graph Node Functions
                
                    void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
                    {
                        RGBA = float4(R, G, B, A);
                        RGB = float3(R, G, B);
                        RG = float2(R, G);
                    }
                
                    void Unity_Multiply_float (float2 A, float2 B, out float2 Out)
                    {
                        Out = A * B;
                    }
                
                    void Unity_Add_float2(float2 A, float2 B, out float2 Out)
                    {
                        Out = A + B;
                    }
                
                    // Subgraph function
                    void sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(float4 _uvST, float4 _uvMask, SurfaceDescriptionInputs IN, out float4 Output1)
                    {
                    float4 _UV_23AF8552_Out = IN.uv0;
                    float _Split_7957D60_R = _UV_23AF8552_Out[0];
                    float _Split_7957D60_G = _UV_23AF8552_Out[1];
                    float _Split_7957D60_B = _UV_23AF8552_Out[2];
                    float _Split_7957D60_A = _UV_23AF8552_Out[3];
                    float4 _Combine_5396A6C7_RGBA;
                    float3 _Combine_5396A6C7_RGB;
                    float2 _Combine_5396A6C7_RG;
                    Unity_Combine_float(_Split_7957D60_R, _Split_7957D60_G, 0, 0, _Combine_5396A6C7_RGBA, _Combine_5396A6C7_RGB, _Combine_5396A6C7_RG);
                    float4 _Property_CB55E443_Out = _uvMask;
                    float _Split_6086B0A5_R = _Property_CB55E443_Out[0];
                    float _Split_6086B0A5_G = _Property_CB55E443_Out[1];
                    float _Split_6086B0A5_B = _Property_CB55E443_Out[2];
                    float _Split_6086B0A5_A = _Property_CB55E443_Out[3];
                    float2 _Multiply_FC550A07_Out;
                    Unity_Multiply_float(_Combine_5396A6C7_RG, (_Split_6086B0A5_R.xx), _Multiply_FC550A07_Out);
                    
                    float4 _UV_3B1D042C_Out = IN.uv1;
                    float _Split_107320B6_R = _UV_3B1D042C_Out[0];
                    float _Split_107320B6_G = _UV_3B1D042C_Out[1];
                    float _Split_107320B6_B = _UV_3B1D042C_Out[2];
                    float _Split_107320B6_A = _UV_3B1D042C_Out[3];
                    float4 _Combine_2E8D3795_RGBA;
                    float3 _Combine_2E8D3795_RGB;
                    float2 _Combine_2E8D3795_RG;
                    Unity_Combine_float(_Split_107320B6_R, _Split_107320B6_G, 0, 0, _Combine_2E8D3795_RGBA, _Combine_2E8D3795_RGB, _Combine_2E8D3795_RG);
                    float2 _Multiply_FDA7BA1E_Out;
                    Unity_Multiply_float(_Combine_2E8D3795_RG, (_Split_6086B0A5_G.xx), _Multiply_FDA7BA1E_Out);
                    
                    float2 _Add_92015245_Out;
                    Unity_Add_float2(_Multiply_FC550A07_Out, _Multiply_FDA7BA1E_Out, _Add_92015245_Out);
                    float4 _UV_49BE4158_Out = IN.uv2;
                    float _Split_A24186AD_R = _UV_49BE4158_Out[0];
                    float _Split_A24186AD_G = _UV_49BE4158_Out[1];
                    float _Split_A24186AD_B = _UV_49BE4158_Out[2];
                    float _Split_A24186AD_A = _UV_49BE4158_Out[3];
                    float4 _Combine_6951B6BC_RGBA;
                    float3 _Combine_6951B6BC_RGB;
                    float2 _Combine_6951B6BC_RG;
                    Unity_Combine_float(_Split_A24186AD_R, _Split_A24186AD_G, 0, 0, _Combine_6951B6BC_RGBA, _Combine_6951B6BC_RGB, _Combine_6951B6BC_RG);
                    float2 _Multiply_1480B81_Out;
                    Unity_Multiply_float(_Combine_6951B6BC_RG, (_Split_6086B0A5_B.xx), _Multiply_1480B81_Out);
                    
                    float4 _UV_9CA65C2_Out = IN.uv3;
                    float _Split_9EC6EA10_R = _UV_9CA65C2_Out[0];
                    float _Split_9EC6EA10_G = _UV_9CA65C2_Out[1];
                    float _Split_9EC6EA10_B = _UV_9CA65C2_Out[2];
                    float _Split_9EC6EA10_A = _UV_9CA65C2_Out[3];
                    float4 _Combine_633F7D3D_RGBA;
                    float3 _Combine_633F7D3D_RGB;
                    float2 _Combine_633F7D3D_RG;
                    Unity_Combine_float(_Split_9EC6EA10_R, _Split_9EC6EA10_G, 0, 0, _Combine_633F7D3D_RGBA, _Combine_633F7D3D_RGB, _Combine_633F7D3D_RG);
                    float2 _Multiply_2A2B5227_Out;
                    Unity_Multiply_float(_Combine_633F7D3D_RG, (_Split_6086B0A5_A.xx), _Multiply_2A2B5227_Out);
                    
                    float2 _Add_B5E7679D_Out;
                    Unity_Add_float2(_Multiply_1480B81_Out, _Multiply_2A2B5227_Out, _Add_B5E7679D_Out);
                    float2 _Add_892742E3_Out;
                    Unity_Add_float2(_Add_92015245_Out, _Add_B5E7679D_Out, _Add_892742E3_Out);
                    float4 _Property_8DA1B077_Out = _uvST;
                    float _Split_1AB0DA31_R = _Property_8DA1B077_Out[0];
                    float _Split_1AB0DA31_G = _Property_8DA1B077_Out[1];
                    float _Split_1AB0DA31_B = _Property_8DA1B077_Out[2];
                    float _Split_1AB0DA31_A = _Property_8DA1B077_Out[3];
                    float4 _Combine_44459F1_RGBA;
                    float3 _Combine_44459F1_RGB;
                    float2 _Combine_44459F1_RG;
                    Unity_Combine_float(_Split_1AB0DA31_R, _Split_1AB0DA31_G, 0, 0, _Combine_44459F1_RGBA, _Combine_44459F1_RGB, _Combine_44459F1_RG);
                    float2 _Multiply_38815E23_Out;
                    Unity_Multiply_float(_Add_892742E3_Out, _Combine_44459F1_RG, _Multiply_38815E23_Out);
                    
                    float _Split_35A1DC4_R = _Property_8DA1B077_Out[0];
                    float _Split_35A1DC4_G = _Property_8DA1B077_Out[1];
                    float _Split_35A1DC4_B = _Property_8DA1B077_Out[2];
                    float _Split_35A1DC4_A = _Property_8DA1B077_Out[3];
                    float4 _Combine_91984BDF_RGBA;
                    float3 _Combine_91984BDF_RGB;
                    float2 _Combine_91984BDF_RG;
                    Unity_Combine_float(_Split_35A1DC4_B, _Split_35A1DC4_A, 0, 0, _Combine_91984BDF_RGBA, _Combine_91984BDF_RGB, _Combine_91984BDF_RG);
                    float2 _Add_63012CEE_Out;
                    Unity_Add_float2(_Multiply_38815E23_Out, _Combine_91984BDF_RG, _Add_63012CEE_Out);
                    Output1 = (float4(_Add_63012CEE_Out, 0.0, 1.0));
                    }
                
                    void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
                    {
                        Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
                    }
                
                    void Unity_Normalize_float3(float3 In, out float3 Out)
                    {
                        Out = normalize(In);
                    }
                
                    void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
                    {
                        Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
                    }
                
                    void Unity_Multiply_float (float4 A, float4 B, out float4 Out)
                    {
                        Out = A * B;
                    }
                
                    void Unity_Lerp_float(float A, float B, float T, out float Out)
                    {
                        Out = lerp(A, B, T);
                    }
                
                    void Unity_Add_float4(float4 A, float4 B, out float4 Out)
                    {
                        Out = A + B;
                    }
                
                    void Unity_Saturate_float4(float4 In, out float4 Out)
                    {
                        Out = saturate(In);
                    }
                
                    void Unity_NormalUnpack_float(float4 In, out float3 Out)
                    {
                                    Out = UnpackNormalmapRGorAG(In);
                                }
                
                    void Unity_NormalBlend_float(float3 A, float3 B, out float3 Out)
                    {
                        Out = normalize(float3(A.rg + B.rg, A.b * B.b));
                    }
                
                    void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
                    {
                        Out = lerp(False, True, Predicate);
                    }
                
                    void Unity_Add_float(float A, float B, out float Out)
                    {
                        Out = A + B;
                    }
                
                    void Unity_Saturate_float(float In, out float Out)
                    {
                        Out = saturate(In);
                    }
                
                    void Unity_Branch_float(float Predicate, float True, float False, out float Out)
                    {
                        Out = lerp(False, True, Predicate);
                    }
                
                    void Unity_Multiply_float (float A, float B, out float Out)
                    {
                        Out = A * B;
                    }
                
                    // Subgraph function
                    void sg_SGRThreadMapDetail_SurfaceDescriptionInputs_64D53B52(float2 _UV, TEXTURE2D_ARGS(_ThreadMap, sampler_ThreadMap), float _ThreadSmoothnessStrength, float _AmbientOcclusion, float _UseThreadMap, float _ThreadAOStrength, float _ThreadNormalStrength, float _Smoothness, float3 _Normals, float _Alpha, SurfaceDescriptionInputs IN, out float4 Output1, out float4 Output2, out float4 Output3, out float4 Output4)
                    {
                    float _Property_7B789410_Out = _UseThreadMap;
                    float3 _Property_D380C535_Out = _Normals;
                    float2 _Property_247E83DC_Out = _UV;
                    float4 _SampleTexture2D_B39DD828_RGBA = SAMPLE_TEXTURE2D(_ThreadMap, sampler_ThreadMap, _Property_247E83DC_Out);
                    float _SampleTexture2D_B39DD828_R = _SampleTexture2D_B39DD828_RGBA.r;
                    float _SampleTexture2D_B39DD828_G = _SampleTexture2D_B39DD828_RGBA.g;
                    float _SampleTexture2D_B39DD828_B = _SampleTexture2D_B39DD828_RGBA.b;
                    float _SampleTexture2D_B39DD828_A = _SampleTexture2D_B39DD828_RGBA.a;
                    float4 _Combine_3989CE7_RGBA;
                    float3 _Combine_3989CE7_RGB;
                    float2 _Combine_3989CE7_RG;
                    Unity_Combine_float(_SampleTexture2D_B39DD828_A, _SampleTexture2D_B39DD828_G, 1, 1, _Combine_3989CE7_RGBA, _Combine_3989CE7_RGB, _Combine_3989CE7_RG);
                    float3 _NormalUnpack_6B39F6EC_Out;
                    Unity_NormalUnpack_float((float4(_Combine_3989CE7_RGB, 1.0)), _NormalUnpack_6B39F6EC_Out);
                    float3 _Normalize_1F52E5EC_Out;
                    Unity_Normalize_float3(_NormalUnpack_6B39F6EC_Out, _Normalize_1F52E5EC_Out);
                    float _Property_2E175598_Out = _ThreadNormalStrength;
                    float3 _NormalStrength_A15875A3_Out;
                    Unity_NormalStrength_float(_Normalize_1F52E5EC_Out, _Property_2E175598_Out, _NormalStrength_A15875A3_Out);
                    float3 _NormalBlend_191D51BE_Out;
                    Unity_NormalBlend_float(_Property_D380C535_Out, _NormalStrength_A15875A3_Out, _NormalBlend_191D51BE_Out);
                    float3 _Normalize_4D9B04E_Out;
                    Unity_Normalize_float3(_NormalBlend_191D51BE_Out, _Normalize_4D9B04E_Out);
                    float3 _Branch_54FF636E_Out;
                    Unity_Branch_float3(_Property_7B789410_Out, _Normalize_4D9B04E_Out, _Property_D380C535_Out, _Branch_54FF636E_Out);
                    float _Property_B5560A97_Out = _UseThreadMap;
                    float _Property_6FAEC412_Out = _Smoothness;
                    float _Remap_C272A01C_Out;
                    Unity_Remap_float(_SampleTexture2D_B39DD828_B, float2 (0,1), float2 (-1,1), _Remap_C272A01C_Out);
                    float _Property_CF380DCA_Out = _ThreadSmoothnessStrength;
                    float _Lerp_1EB6EBC0_Out;
                    Unity_Lerp_float(0, _Remap_C272A01C_Out, _Property_CF380DCA_Out, _Lerp_1EB6EBC0_Out);
                    float _Add_2975BB_Out;
                    Unity_Add_float(_Property_6FAEC412_Out, _Lerp_1EB6EBC0_Out, _Add_2975BB_Out);
                    float _Saturate_1F46047D_Out;
                    Unity_Saturate_float(_Add_2975BB_Out, _Saturate_1F46047D_Out);
                    float _Branch_1C4EA1E2_Out;
                    Unity_Branch_float(_Property_B5560A97_Out, _Saturate_1F46047D_Out, _Property_6FAEC412_Out, _Branch_1C4EA1E2_Out);
                    float _Property_57F076E2_Out = _UseThreadMap;
                    float _Property_829FEB4F_Out = _ThreadAOStrength;
                    float _Lerp_1DC743E3_Out;
                    Unity_Lerp_float(1, _SampleTexture2D_B39DD828_R, _Property_829FEB4F_Out, _Lerp_1DC743E3_Out);
                    float _Property_416E73AE_Out = _AmbientOcclusion;
                    float _Multiply_FBD87ACD_Out;
                    Unity_Multiply_float(_Lerp_1DC743E3_Out, _Property_416E73AE_Out, _Multiply_FBD87ACD_Out);
                    
                    float _Branch_A5F3B7F9_Out;
                    Unity_Branch_float(_Property_57F076E2_Out, _Multiply_FBD87ACD_Out, _Property_416E73AE_Out, _Branch_A5F3B7F9_Out);
                    float _Property_5FDD4914_Out = _Alpha;
                    float _Multiply_716B151B_Out;
                    Unity_Multiply_float(_SampleTexture2D_B39DD828_R, _Property_5FDD4914_Out, _Multiply_716B151B_Out);
                    
                    Output1 = (float4(_Branch_54FF636E_Out, 1.0));
                    Output2 = (_Branch_1C4EA1E2_Out.xxxx);
                    Output3 = (_Branch_A5F3B7F9_Out.xxxx);
                    Output4 = (_Multiply_716B151B_Out.xxxx);
                    }
                
                // Pixel Graph Evaluation
                    SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                    {
                        SurfaceDescription surface = (SurfaceDescription)0;
                        float _Property_1E54B66A_Out = _useThreadMap;
                        float4 _Property_8AE14795_Out = _uvThreadMask;
                        float4 _Property_958B7FC9_Out = _uvThreadST;
                        float4 _Subgraph_B567E108_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_958B7FC9_Out, _Property_8AE14795_Out, IN, _Subgraph_B567E108_Output1);
                        float4 _Property_FEDB20A0_Out = _uvBaseMask;
                        float4 _Property_F42AAF3B_Out = _uvBaseST;
                        float4 _Subgraph_9D4E0F1_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_F42AAF3B_Out, _Property_FEDB20A0_Out, IN, _Subgraph_9D4E0F1_Output1);
                        float4 _SampleTexture2D_105B35B3_RGBA = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, (_Subgraph_9D4E0F1_Output1.xy));
                        _SampleTexture2D_105B35B3_RGBA.rgb = UnpackNormalmapRGorAG(_SampleTexture2D_105B35B3_RGBA);
                        float _SampleTexture2D_105B35B3_R = _SampleTexture2D_105B35B3_RGBA.r;
                        float _SampleTexture2D_105B35B3_G = _SampleTexture2D_105B35B3_RGBA.g;
                        float _SampleTexture2D_105B35B3_B = _SampleTexture2D_105B35B3_RGBA.b;
                        float _SampleTexture2D_105B35B3_A = _SampleTexture2D_105B35B3_RGBA.a;
                        float _Property_82D183C3_Out = _NormalMapStrength;
                        float3 _NormalStrength_BFF5C35E_Out;
                        Unity_NormalStrength_float((_SampleTexture2D_105B35B3_RGBA.xyz), _Property_82D183C3_Out, _NormalStrength_BFF5C35E_Out);
                        float3 _Normalize_ACA4E10E_Out;
                        Unity_Normalize_float3(_NormalStrength_BFF5C35E_Out, _Normalize_ACA4E10E_Out);
                        float4 _Property_1E040901_Out = _uvBaseMask;
                        float4 _Property_97A7EF85_Out = _uvBaseST;
                        float4 _Subgraph_8DDCEE61_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_97A7EF85_Out, _Property_1E040901_Out, IN, _Subgraph_8DDCEE61_Output1);
                        float4 _SampleTexture2D_8C3CF01A_RGBA = SAMPLE_TEXTURE2D(_SpecColorMap, sampler_SpecColorMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_8C3CF01A_R = _SampleTexture2D_8C3CF01A_RGBA.r;
                        float _SampleTexture2D_8C3CF01A_G = _SampleTexture2D_8C3CF01A_RGBA.g;
                        float _SampleTexture2D_8C3CF01A_B = _SampleTexture2D_8C3CF01A_RGBA.b;
                        float _SampleTexture2D_8C3CF01A_A = _SampleTexture2D_8C3CF01A_RGBA.a;
                        float _Property_B948927_Out = _SmoothnessMin;
                        float _Property_2962A49E_Out = _SmoothnessMax;
                        float2 _Vector2_9C783D17_Out = float2(_Property_B948927_Out,_Property_2962A49E_Out);
                        float _Remap_10DEF6A_Out;
                        Unity_Remap_float(_SampleTexture2D_8C3CF01A_A, float2 (0,1), _Vector2_9C783D17_Out, _Remap_10DEF6A_Out);
                        float4 _Property_90FAF786_Out = _BaseColor;
                        float4 _SampleTexture2D_11CFD011_RGBA = SAMPLE_TEXTURE2D(_BaseColorMap, sampler_BaseColorMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_11CFD011_R = _SampleTexture2D_11CFD011_RGBA.r;
                        float _SampleTexture2D_11CFD011_G = _SampleTexture2D_11CFD011_RGBA.g;
                        float _SampleTexture2D_11CFD011_B = _SampleTexture2D_11CFD011_RGBA.b;
                        float _SampleTexture2D_11CFD011_A = _SampleTexture2D_11CFD011_RGBA.a;
                        float4 _Multiply_98A7A079_Out;
                        Unity_Multiply_float(_Property_90FAF786_Out, _SampleTexture2D_11CFD011_RGBA, _Multiply_98A7A079_Out);
                    
                        float _Property_7C6435CB_Out = _FuzzMapUVScale;
                        float4 _Multiply_18C3A780_Out;
                        Unity_Multiply_float(_Subgraph_8DDCEE61_Output1, (_Property_7C6435CB_Out.xxxx), _Multiply_18C3A780_Out);
                    
                        float4 _SampleTexture2D_4D82F05E_RGBA = SAMPLE_TEXTURE2D(_FuzzMap, sampler_FuzzMap, (_Multiply_18C3A780_Out.xy));
                        float _SampleTexture2D_4D82F05E_R = _SampleTexture2D_4D82F05E_RGBA.r;
                        float _SampleTexture2D_4D82F05E_G = _SampleTexture2D_4D82F05E_RGBA.g;
                        float _SampleTexture2D_4D82F05E_B = _SampleTexture2D_4D82F05E_RGBA.b;
                        float _SampleTexture2D_4D82F05E_A = _SampleTexture2D_4D82F05E_RGBA.a;
                        float _Property_6CCE2816_Out = _FuzzStrength;
                        float _Lerp_2C953D15_Out;
                        Unity_Lerp_float(0, _SampleTexture2D_4D82F05E_R, _Property_6CCE2816_Out, _Lerp_2C953D15_Out);
                        float4 _Add_A30FF2E2_Out;
                        Unity_Add_float4(_Multiply_98A7A079_Out, (_Lerp_2C953D15_Out.xxxx), _Add_A30FF2E2_Out);
                        float4 _Saturate_69BD2FF3_Out;
                        Unity_Saturate_float4(_Add_A30FF2E2_Out, _Saturate_69BD2FF3_Out);
                        float _Split_EB0B739F_R = _Saturate_69BD2FF3_Out[0];
                        float _Split_EB0B739F_G = _Saturate_69BD2FF3_Out[1];
                        float _Split_EB0B739F_B = _Saturate_69BD2FF3_Out[2];
                        float _Split_EB0B739F_A = _Saturate_69BD2FF3_Out[3];
                        float4 _SampleTexture2D_EECA7933_RGBA = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_EECA7933_R = _SampleTexture2D_EECA7933_RGBA.r;
                        float _SampleTexture2D_EECA7933_G = _SampleTexture2D_EECA7933_RGBA.g;
                        float _SampleTexture2D_EECA7933_B = _SampleTexture2D_EECA7933_RGBA.b;
                        float _SampleTexture2D_EECA7933_A = _SampleTexture2D_EECA7933_RGBA.a;
                        float _Property_88B45C0E_Out = _ThreadAOStrength01;
                        float _Property_FC0CC4C0_Out = _ThreadNormalStrength;
                        float _Property_AC495D22_Out = _ThreadSmoothnessScale;
                        float4 _Subgraph_E494B5B1_Output1;
                        float4 _Subgraph_E494B5B1_Output2;
                        float4 _Subgraph_E494B5B1_Output3;
                        float4 _Subgraph_E494B5B1_Output4;
                        sg_SGRThreadMapDetail_SurfaceDescriptionInputs_64D53B52((_Subgraph_B567E108_Output1.xy), TEXTURE2D_PARAM(_ThreadMap, sampler_ThreadMap), _Property_AC495D22_Out, _SampleTexture2D_EECA7933_G, _Property_1E54B66A_Out, _Property_88B45C0E_Out, _Property_FC0CC4C0_Out, _Remap_10DEF6A_Out, _Normalize_ACA4E10E_Out, _Split_EB0B739F_A, IN, _Subgraph_E494B5B1_Output1, _Subgraph_E494B5B1_Output2, _Subgraph_E494B5B1_Output3, _Subgraph_E494B5B1_Output4);
                        surface.Alpha = (_Subgraph_E494B5B1_Output4).x;
                        return surface;
                    }
                    
            //-------------------------------------------------------------------------------------
            // End graph generated code
            //-------------------------------------------------------------------------------------
        
        
        
        //-------------------------------------------------------------------------------------
        // TEMPLATE INCLUDE : SharedCode.template.hlsl
        //-------------------------------------------------------------------------------------
            FragInputs BuildFragInputs(VaryingsMeshToPS input)
            {
                FragInputs output;
                ZERO_INITIALIZE(FragInputs, output);
        
                // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                // to compute normals which are then passed on elsewhere to compute other values...
                output.worldToTangent = k_identity3x3;
                output.positionSS = input.positionCS;       // input.positionCS is SV_Position
        
                output.positionRWS = input.positionRWS;
                output.worldToTangent = BuildWorldToTangent(input.tangentWS, input.normalWS);
                output.texCoord0 = input.texCoord0;
                output.texCoord1 = input.texCoord1;
                output.texCoord2 = input.texCoord2;
                output.texCoord3 = input.texCoord3;
                output.color = input.color;
                #if SHADER_STAGE_FRAGMENT
                #endif // SHADER_STAGE_FRAGMENT
        
                return output;
            }
        
            SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
                output.uv0 =                         input.texCoord0;
                output.uv1 =                         input.texCoord1;
                output.uv2 =                         input.texCoord2;
                output.uv3 =                         input.texCoord3;
        
                return output;
            }
        
            // existing HDRP code uses the combined function to go directly from packed to frag inputs
            FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
            {
                VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                return BuildFragInputs(unpacked);
            }
        
        //-------------------------------------------------------------------------------------
        // END TEMPLATE INCLUDE : SharedCode.template.hlsl
        //-------------------------------------------------------------------------------------
        
        
            void ApplyDecalToSurfaceData(DecalSurfaceData decalSurfaceData, inout SurfaceData surfaceData)
            {
                // using alpha compositing https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch23.html
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_DIFFUSE)
                {
                    surfaceData.baseColor.xyz = surfaceData.baseColor.xyz * decalSurfaceData.baseColor.w + decalSurfaceData.baseColor.xyz;
                }
        
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_NORMAL)
                {
                    surfaceData.normalWS.xyz = normalize(surfaceData.normalWS.xyz * decalSurfaceData.normalWS.w + decalSurfaceData.normalWS.xyz);
                }
        
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_MASK)
                {
            #ifdef DECALS_4RT // only smoothness in 3RT mode
                    // Don't apply any metallic modification
                    surfaceData.ambientOcclusion = surfaceData.ambientOcclusion * decalSurfaceData.MAOSBlend.y + decalSurfaceData.mask.y;
            #endif
        
                    surfaceData.perceptualSmoothness = surfaceData.perceptualSmoothness * decalSurfaceData.mask.w + decalSurfaceData.mask.z;
                }
            }
        
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, out SurfaceData surfaceData)
            {
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(SurfaceData, surfaceData);
        
                // copy across graph values, if defined
        
        
        
        
        
        
        
        
        
                
                // These static material feature allow compile time optimization
                surfaceData.materialFeatures = 0;
        
                // Transform the preprocess macro into a material feature (note that silk flag is deduced from the abscence of this one)
                #ifdef _MATERIAL_FEATURE_COTTON_WOOL
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_COTTON_WOOL;
                #endif
        
                #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_SUBSURFACE_SCATTERING;
                #endif
        
                #ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_TRANSMISSION;
                #endif
        
        
        #if defined (_ENERGY_CONSERVING_SPECULAR)
                // Require to have setup baseColor
                // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
        #endif
        
                // tangent-space normal
                float3 normalTS = float3(0.0f, 0.0f, 1.0f);
        
                // compute world space normal
                GetNormalWS(fragInputs, normalTS, surfaceData.normalWS);
        
                surfaceData.geomNormalWS = fragInputs.worldToTangent[2];
        
                surfaceData.tangentWS = normalize(fragInputs.worldToTangent[0].xyz);    // The tangent is not normalize in worldToTangent for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
        
                // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                // If user provide bent normal then we process a better term
                surfaceData.specularOcclusion = 1.0;
        
        #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                // Just use the value passed through via the slot (not active otherwise)
        #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                // If we have bent normal and ambient occlusion, process a specular occlusion
                surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
        #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
        #else
                surfaceData.specularOcclusion = 1.0;
                surfaceData.specularOcclusion = 1.0;
        #endif
        
        #ifdef DEBUG_DISPLAY
                // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                // as it can modify attribute use for static lighting
                ApplyDebugToSurfaceData(fragInputs.worldToTangent, surfaceData);
        #endif
            }
        
            void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
            {
        #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                uint3 fadeMaskSeed = asuint((int3)(V * _ScreenSize.xyx)); // Quantize V to _ScreenSize values
                LODDitheringTransition(fadeMaskSeed, unity_LODFade.x);
        #endif
        
                // this applies the double sided tangent space correction -- see 'ApplyDoubleSidedFlipOrMirror()'
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
        
                BuildSurfaceData(fragInputs, surfaceDescription, V, surfaceData);
        
        #if HAVE_DECALS && _DECALS
                DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, surfaceDescription.Alpha);
                ApplyDecalToSurfaceData(decalSurfaceData, surfaceData);
        #endif
        
                // Builtin Data
                // For back lighting we use the oposite vertex normal 
                InitBuiltinData(surfaceDescription.Alpha, surfaceData.normalWS, -fragInputs.worldToTangent[2], fragInputs.positionRWS, fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
        
        
                builtinData.depthOffset = 0.0;                        // ApplyPerPixelDisplacement(input, V, layerTexCoord, blendMasks); #ifdef _DEPTHOFFSET_ON : ApplyDepthOffsetPositionInput(V, depthOffset, GetWorldToHClipMatrix(), posInput);
        
                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
            }
        
            //-------------------------------------------------------------------------------------
            // Pass Includes
            //-------------------------------------------------------------------------------------
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"
            //-------------------------------------------------------------------------------------
            // End Pass Includes
            //-------------------------------------------------------------------------------------
        
            ENDHLSL
        }
        
        Pass
        {
            // based on FabricPass.template
            Name "Motion Vectors"
            Tags { "LightMode" = "MotionVectors" }
        
            //-------------------------------------------------------------------------------------
            // Render Modes (Blend, Cull, ZTest, Stencil, etc)
            //-------------------------------------------------------------------------------------
                Blend One Zero
        
                Cull Back
        
                ZTest LEqual
        
                ZWrite On
        
                ZClip [_ZClip]
        
                // If velocity pass (motion vectors) is enabled we tag the stencil so it don't perform CameraMotionVelocity
        Stencil
        {
           WriteMask 128
           Ref 128
           Comp Always
           Pass Replace
        }
        
                
            //-------------------------------------------------------------------------------------
            // End Render Modes
            //-------------------------------------------------------------------------------------
        
            HLSLPROGRAM
        
                #pragma target 4.5
                #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
                //#pragma enable_d3d11_debug_symbols
        
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
        
                #pragma multi_compile _ LOD_FADE_CROSSFADE
        
            //-------------------------------------------------------------------------------------
            // Variant Definitions (active field translations to HDRP defines)
            //-------------------------------------------------------------------------------------
                #define _MATERIAL_FEATURE_COTTON_WOOL 1
                #define _SPECULAR_OCCLUSION_FROM_AO 1
                #define _ENERGY_CONSERVING_SPECULAR 1
                #define _DISABLE_SSR 1
            //-------------------------------------------------------------------------------------
            // End Variant Definitions
            //-------------------------------------------------------------------------------------
        
            #pragma vertex Vert
            #pragma fragment Frag
        
            #define UNITY_MATERIAL_FABRIC      // Need to be define before including Material.hlsl
        
            // This will be enabled in an upcoming change. 
            // #define SURFACE_GRADIENT
        
            // If we use subsurface scattering, enable output split lighting (for forward pass)
            #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
            #define OUTPUT_SPLIT_LIGHTING
            #endif
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Wind.hlsl"
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
        
            // define FragInputs structure
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
        
            //-------------------------------------------------------------------------------------
            // Defines
            //-------------------------------------------------------------------------------------
                #define SHADERPASS SHADERPASS_VELOCITY
                #define WRITE_NORMAL_BUFFER
                #pragma multi_compile _ WRITE_MSAA_DEPTH
        
                // this translates the new dependency tracker into the old preprocessor definitions for the existing HDRP shader code
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_TEXCOORD2
                #define ATTRIBUTES_NEED_TEXCOORD3
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_POSITION_WS
                #define VARYINGS_NEED_TANGENT_TO_WORLD
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_TEXCOORD2
                #define VARYINGS_NEED_TEXCOORD3
                #define VARYINGS_NEED_COLOR
        
            //-------------------------------------------------------------------------------------
            // End Defines
            //-------------------------------------------------------------------------------------
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #ifdef DEBUG_DISPLAY
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif
        
            #if (SHADERPASS == SHADERPASS_FORWARD)
                // used for shaders that want to do lighting (and materials)
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
            #else
                // used for shaders that don't need lighting
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #endif
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
        
            //Used by SceneSelectionPass
            int _ObjectId;
            int _PassValue;
        
            // this function assumes the bitangent flip is encoded in tangentWS.w
            // TODO: move this function to HDRP shared file, once we merge with HDRP repo
            float3x3 BuildWorldToTangent(float4 tangentWS, float3 normalWS)
            {
                // tangentWS must not be normalized (mikkts requirement)
        
                // Normalize normalWS vector but keep the renormFactor to apply it to bitangent and tangent
        	    float3 unnormalizedNormalWS = normalWS;
                float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
                // bitangent on the fly option in xnormal to reduce vertex shader outputs.
        	    // this is the mikktspace transformation (must use unnormalized attributes)
                float3x3 worldToTangent = CreateWorldToTangent(unnormalizedNormalWS, tangentWS.xyz, tangentWS.w > 0.0 ? 1.0 : -1.0);
        
        	    // surface gradient based formulation requires a unit length initial normal. We can maintain compliance with mikkts
        	    // by uniformly scaling all 3 vectors since normalization of the perturbed normal will cancel it.
                worldToTangent[0] = worldToTangent[0] * renormFactor;
                worldToTangent[1] = worldToTangent[1] * renormFactor;
                worldToTangent[2] = worldToTangent[2] * renormFactor;		// normalizes the interpolated vertex normal
                return worldToTangent;
            }
        
            //-------------------------------------------------------------------------------------
            // Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
        // Generated Type: AttributesMesh
        struct AttributesMesh {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL; // optional
            float4 tangentOS : TANGENT; // optional
            float4 uv0 : TEXCOORD0; // optional
            float4 uv1 : TEXCOORD1; // optional
            float4 uv2 : TEXCOORD2; // optional
            float4 uv3 : TEXCOORD3; // optional
            float4 color : COLOR; // optional
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        
        // Generated Type: VaryingsMeshToPS
        struct VaryingsMeshToPS {
            float4 positionCS : SV_Position;
            float3 positionRWS; // optional
            float3 normalWS; // optional
            float4 tangentWS; // optional
            float4 texCoord0; // optional
            float4 texCoord1; // optional
            float4 texCoord2; // optional
            float4 texCoord3; // optional
            float4 color; // optional
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        struct PackedVaryingsMeshToPS {
            float3 interp00 : TEXCOORD0; // auto-packed
            float3 interp01 : TEXCOORD1; // auto-packed
            float4 interp02 : TEXCOORD2; // auto-packed
            float4 interp03 : TEXCOORD3; // auto-packed
            float4 interp04 : TEXCOORD4; // auto-packed
            float4 interp05 : TEXCOORD5; // auto-packed
            float4 interp06 : TEXCOORD6; // auto-packed
            float4 interp07 : TEXCOORD7; // auto-packed
            float4 positionCS : SV_Position; // unpacked
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC; // unpacked
            #endif // INSTANCING_ON
        };
        PackedVaryingsMeshToPS PackVaryingsMeshToPS(VaryingsMeshToPS input)
        {
            PackedVaryingsMeshToPS output;
            output.positionCS = input.positionCS;
            output.interp00.xyz = input.positionRWS;
            output.interp01.xyz = input.normalWS;
            output.interp02.xyzw = input.tangentWS;
            output.interp03.xyzw = input.texCoord0;
            output.interp04.xyzw = input.texCoord1;
            output.interp05.xyzw = input.texCoord2;
            output.interp06.xyzw = input.texCoord3;
            output.interp07.xyzw = input.color;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        VaryingsMeshToPS UnpackVaryingsMeshToPS(PackedVaryingsMeshToPS input)
        {
            VaryingsMeshToPS output;
            output.positionCS = input.positionCS;
            output.positionRWS = input.interp00.xyz;
            output.normalWS = input.interp01.xyz;
            output.tangentWS = input.interp02.xyzw;
            output.texCoord0 = input.interp03.xyzw;
            output.texCoord1 = input.interp04.xyzw;
            output.texCoord2 = input.interp05.xyzw;
            output.texCoord3 = input.interp06.xyzw;
            output.color = input.interp07.xyzw;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        
        // Generated Type: VaryingsMeshToDS
        struct VaryingsMeshToDS {
            float3 positionRWS;
            float3 normalWS;
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        struct PackedVaryingsMeshToDS {
            float3 interp00 : TEXCOORD0; // auto-packed
            float3 interp01 : TEXCOORD1; // auto-packed
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC; // unpacked
            #endif // INSTANCING_ON
        };
        PackedVaryingsMeshToDS PackVaryingsMeshToDS(VaryingsMeshToDS input)
        {
            PackedVaryingsMeshToDS output;
            output.interp00.xyz = input.positionRWS;
            output.interp01.xyz = input.normalWS;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        VaryingsMeshToDS UnpackVaryingsMeshToDS(PackedVaryingsMeshToDS input)
        {
            VaryingsMeshToDS output;
            output.positionRWS = input.interp00.xyz;
            output.normalWS = input.interp01.xyz;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        
            //-------------------------------------------------------------------------------------
            // End Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
        
            //-------------------------------------------------------------------------------------
            // Graph generated code
            //-------------------------------------------------------------------------------------
                // Shared Graph Properties (uniform inputs)
                    CBUFFER_START(UnityPerMaterial)
                    float4 _uvBaseMask;
                    float4 _uvBaseST;
                    float4 _BaseColor;
                    float _SmoothnessMin;
                    float _SmoothnessMax;
                    float4 _SpecularColor;
                    float _NormalMapStrength;
                    float _useThreadMap;
                    float4 _uvThreadMask;
                    float4 _uvThreadST;
                    float _ThreadAOStrength01;
                    float _ThreadNormalStrength;
                    float _ThreadSmoothnessScale;
                    float _FuzzMapUVScale;
                    float _FuzzStrength;
                    float4 _EmissionColor;
                    CBUFFER_END
                
                    TEXTURE2D(_BaseColorMap); SAMPLER(sampler_BaseColorMap); float4 _BaseColorMap_TexelSize;
                    TEXTURE2D(_MaskMap); SAMPLER(sampler_MaskMap); float4 _MaskMap_TexelSize;
                    TEXTURE2D(_SpecColorMap); SAMPLER(sampler_SpecColorMap); float4 _SpecColorMap_TexelSize;
                    TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap); float4 _NormalMap_TexelSize;
                    TEXTURE2D(_ThreadMap); SAMPLER(sampler_ThreadMap); float4 _ThreadMap_TexelSize;
                    TEXTURE2D(_FuzzMap); SAMPLER(sampler_FuzzMap); float4 _FuzzMap_TexelSize;
                
                // Pixel Graph Inputs
                    struct SurfaceDescriptionInputs {
                        float4 uv0; // optional
                        float4 uv1; // optional
                        float4 uv2; // optional
                        float4 uv3; // optional
                    };
                // Pixel Graph Outputs
                    struct SurfaceDescription
                    {
                        float Alpha;
                    };
                    
                // Shared Graph Node Functions
                
                    void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
                    {
                        RGBA = float4(R, G, B, A);
                        RGB = float3(R, G, B);
                        RG = float2(R, G);
                    }
                
                    void Unity_Multiply_float (float2 A, float2 B, out float2 Out)
                    {
                        Out = A * B;
                    }
                
                    void Unity_Add_float2(float2 A, float2 B, out float2 Out)
                    {
                        Out = A + B;
                    }
                
                    // Subgraph function
                    void sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(float4 _uvST, float4 _uvMask, SurfaceDescriptionInputs IN, out float4 Output1)
                    {
                    float4 _UV_23AF8552_Out = IN.uv0;
                    float _Split_7957D60_R = _UV_23AF8552_Out[0];
                    float _Split_7957D60_G = _UV_23AF8552_Out[1];
                    float _Split_7957D60_B = _UV_23AF8552_Out[2];
                    float _Split_7957D60_A = _UV_23AF8552_Out[3];
                    float4 _Combine_5396A6C7_RGBA;
                    float3 _Combine_5396A6C7_RGB;
                    float2 _Combine_5396A6C7_RG;
                    Unity_Combine_float(_Split_7957D60_R, _Split_7957D60_G, 0, 0, _Combine_5396A6C7_RGBA, _Combine_5396A6C7_RGB, _Combine_5396A6C7_RG);
                    float4 _Property_CB55E443_Out = _uvMask;
                    float _Split_6086B0A5_R = _Property_CB55E443_Out[0];
                    float _Split_6086B0A5_G = _Property_CB55E443_Out[1];
                    float _Split_6086B0A5_B = _Property_CB55E443_Out[2];
                    float _Split_6086B0A5_A = _Property_CB55E443_Out[3];
                    float2 _Multiply_FC550A07_Out;
                    Unity_Multiply_float(_Combine_5396A6C7_RG, (_Split_6086B0A5_R.xx), _Multiply_FC550A07_Out);
                    
                    float4 _UV_3B1D042C_Out = IN.uv1;
                    float _Split_107320B6_R = _UV_3B1D042C_Out[0];
                    float _Split_107320B6_G = _UV_3B1D042C_Out[1];
                    float _Split_107320B6_B = _UV_3B1D042C_Out[2];
                    float _Split_107320B6_A = _UV_3B1D042C_Out[3];
                    float4 _Combine_2E8D3795_RGBA;
                    float3 _Combine_2E8D3795_RGB;
                    float2 _Combine_2E8D3795_RG;
                    Unity_Combine_float(_Split_107320B6_R, _Split_107320B6_G, 0, 0, _Combine_2E8D3795_RGBA, _Combine_2E8D3795_RGB, _Combine_2E8D3795_RG);
                    float2 _Multiply_FDA7BA1E_Out;
                    Unity_Multiply_float(_Combine_2E8D3795_RG, (_Split_6086B0A5_G.xx), _Multiply_FDA7BA1E_Out);
                    
                    float2 _Add_92015245_Out;
                    Unity_Add_float2(_Multiply_FC550A07_Out, _Multiply_FDA7BA1E_Out, _Add_92015245_Out);
                    float4 _UV_49BE4158_Out = IN.uv2;
                    float _Split_A24186AD_R = _UV_49BE4158_Out[0];
                    float _Split_A24186AD_G = _UV_49BE4158_Out[1];
                    float _Split_A24186AD_B = _UV_49BE4158_Out[2];
                    float _Split_A24186AD_A = _UV_49BE4158_Out[3];
                    float4 _Combine_6951B6BC_RGBA;
                    float3 _Combine_6951B6BC_RGB;
                    float2 _Combine_6951B6BC_RG;
                    Unity_Combine_float(_Split_A24186AD_R, _Split_A24186AD_G, 0, 0, _Combine_6951B6BC_RGBA, _Combine_6951B6BC_RGB, _Combine_6951B6BC_RG);
                    float2 _Multiply_1480B81_Out;
                    Unity_Multiply_float(_Combine_6951B6BC_RG, (_Split_6086B0A5_B.xx), _Multiply_1480B81_Out);
                    
                    float4 _UV_9CA65C2_Out = IN.uv3;
                    float _Split_9EC6EA10_R = _UV_9CA65C2_Out[0];
                    float _Split_9EC6EA10_G = _UV_9CA65C2_Out[1];
                    float _Split_9EC6EA10_B = _UV_9CA65C2_Out[2];
                    float _Split_9EC6EA10_A = _UV_9CA65C2_Out[3];
                    float4 _Combine_633F7D3D_RGBA;
                    float3 _Combine_633F7D3D_RGB;
                    float2 _Combine_633F7D3D_RG;
                    Unity_Combine_float(_Split_9EC6EA10_R, _Split_9EC6EA10_G, 0, 0, _Combine_633F7D3D_RGBA, _Combine_633F7D3D_RGB, _Combine_633F7D3D_RG);
                    float2 _Multiply_2A2B5227_Out;
                    Unity_Multiply_float(_Combine_633F7D3D_RG, (_Split_6086B0A5_A.xx), _Multiply_2A2B5227_Out);
                    
                    float2 _Add_B5E7679D_Out;
                    Unity_Add_float2(_Multiply_1480B81_Out, _Multiply_2A2B5227_Out, _Add_B5E7679D_Out);
                    float2 _Add_892742E3_Out;
                    Unity_Add_float2(_Add_92015245_Out, _Add_B5E7679D_Out, _Add_892742E3_Out);
                    float4 _Property_8DA1B077_Out = _uvST;
                    float _Split_1AB0DA31_R = _Property_8DA1B077_Out[0];
                    float _Split_1AB0DA31_G = _Property_8DA1B077_Out[1];
                    float _Split_1AB0DA31_B = _Property_8DA1B077_Out[2];
                    float _Split_1AB0DA31_A = _Property_8DA1B077_Out[3];
                    float4 _Combine_44459F1_RGBA;
                    float3 _Combine_44459F1_RGB;
                    float2 _Combine_44459F1_RG;
                    Unity_Combine_float(_Split_1AB0DA31_R, _Split_1AB0DA31_G, 0, 0, _Combine_44459F1_RGBA, _Combine_44459F1_RGB, _Combine_44459F1_RG);
                    float2 _Multiply_38815E23_Out;
                    Unity_Multiply_float(_Add_892742E3_Out, _Combine_44459F1_RG, _Multiply_38815E23_Out);
                    
                    float _Split_35A1DC4_R = _Property_8DA1B077_Out[0];
                    float _Split_35A1DC4_G = _Property_8DA1B077_Out[1];
                    float _Split_35A1DC4_B = _Property_8DA1B077_Out[2];
                    float _Split_35A1DC4_A = _Property_8DA1B077_Out[3];
                    float4 _Combine_91984BDF_RGBA;
                    float3 _Combine_91984BDF_RGB;
                    float2 _Combine_91984BDF_RG;
                    Unity_Combine_float(_Split_35A1DC4_B, _Split_35A1DC4_A, 0, 0, _Combine_91984BDF_RGBA, _Combine_91984BDF_RGB, _Combine_91984BDF_RG);
                    float2 _Add_63012CEE_Out;
                    Unity_Add_float2(_Multiply_38815E23_Out, _Combine_91984BDF_RG, _Add_63012CEE_Out);
                    Output1 = (float4(_Add_63012CEE_Out, 0.0, 1.0));
                    }
                
                    void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
                    {
                        Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
                    }
                
                    void Unity_Normalize_float3(float3 In, out float3 Out)
                    {
                        Out = normalize(In);
                    }
                
                    void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
                    {
                        Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
                    }
                
                    void Unity_Multiply_float (float4 A, float4 B, out float4 Out)
                    {
                        Out = A * B;
                    }
                
                    void Unity_Lerp_float(float A, float B, float T, out float Out)
                    {
                        Out = lerp(A, B, T);
                    }
                
                    void Unity_Add_float4(float4 A, float4 B, out float4 Out)
                    {
                        Out = A + B;
                    }
                
                    void Unity_Saturate_float4(float4 In, out float4 Out)
                    {
                        Out = saturate(In);
                    }
                
                    void Unity_NormalUnpack_float(float4 In, out float3 Out)
                    {
                                    Out = UnpackNormalmapRGorAG(In);
                                }
                
                    void Unity_NormalBlend_float(float3 A, float3 B, out float3 Out)
                    {
                        Out = normalize(float3(A.rg + B.rg, A.b * B.b));
                    }
                
                    void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
                    {
                        Out = lerp(False, True, Predicate);
                    }
                
                    void Unity_Add_float(float A, float B, out float Out)
                    {
                        Out = A + B;
                    }
                
                    void Unity_Saturate_float(float In, out float Out)
                    {
                        Out = saturate(In);
                    }
                
                    void Unity_Branch_float(float Predicate, float True, float False, out float Out)
                    {
                        Out = lerp(False, True, Predicate);
                    }
                
                    void Unity_Multiply_float (float A, float B, out float Out)
                    {
                        Out = A * B;
                    }
                
                    // Subgraph function
                    void sg_SGRThreadMapDetail_SurfaceDescriptionInputs_64D53B52(float2 _UV, TEXTURE2D_ARGS(_ThreadMap, sampler_ThreadMap), float _ThreadSmoothnessStrength, float _AmbientOcclusion, float _UseThreadMap, float _ThreadAOStrength, float _ThreadNormalStrength, float _Smoothness, float3 _Normals, float _Alpha, SurfaceDescriptionInputs IN, out float4 Output1, out float4 Output2, out float4 Output3, out float4 Output4)
                    {
                    float _Property_7B789410_Out = _UseThreadMap;
                    float3 _Property_D380C535_Out = _Normals;
                    float2 _Property_247E83DC_Out = _UV;
                    float4 _SampleTexture2D_B39DD828_RGBA = SAMPLE_TEXTURE2D(_ThreadMap, sampler_ThreadMap, _Property_247E83DC_Out);
                    float _SampleTexture2D_B39DD828_R = _SampleTexture2D_B39DD828_RGBA.r;
                    float _SampleTexture2D_B39DD828_G = _SampleTexture2D_B39DD828_RGBA.g;
                    float _SampleTexture2D_B39DD828_B = _SampleTexture2D_B39DD828_RGBA.b;
                    float _SampleTexture2D_B39DD828_A = _SampleTexture2D_B39DD828_RGBA.a;
                    float4 _Combine_3989CE7_RGBA;
                    float3 _Combine_3989CE7_RGB;
                    float2 _Combine_3989CE7_RG;
                    Unity_Combine_float(_SampleTexture2D_B39DD828_A, _SampleTexture2D_B39DD828_G, 1, 1, _Combine_3989CE7_RGBA, _Combine_3989CE7_RGB, _Combine_3989CE7_RG);
                    float3 _NormalUnpack_6B39F6EC_Out;
                    Unity_NormalUnpack_float((float4(_Combine_3989CE7_RGB, 1.0)), _NormalUnpack_6B39F6EC_Out);
                    float3 _Normalize_1F52E5EC_Out;
                    Unity_Normalize_float3(_NormalUnpack_6B39F6EC_Out, _Normalize_1F52E5EC_Out);
                    float _Property_2E175598_Out = _ThreadNormalStrength;
                    float3 _NormalStrength_A15875A3_Out;
                    Unity_NormalStrength_float(_Normalize_1F52E5EC_Out, _Property_2E175598_Out, _NormalStrength_A15875A3_Out);
                    float3 _NormalBlend_191D51BE_Out;
                    Unity_NormalBlend_float(_Property_D380C535_Out, _NormalStrength_A15875A3_Out, _NormalBlend_191D51BE_Out);
                    float3 _Normalize_4D9B04E_Out;
                    Unity_Normalize_float3(_NormalBlend_191D51BE_Out, _Normalize_4D9B04E_Out);
                    float3 _Branch_54FF636E_Out;
                    Unity_Branch_float3(_Property_7B789410_Out, _Normalize_4D9B04E_Out, _Property_D380C535_Out, _Branch_54FF636E_Out);
                    float _Property_B5560A97_Out = _UseThreadMap;
                    float _Property_6FAEC412_Out = _Smoothness;
                    float _Remap_C272A01C_Out;
                    Unity_Remap_float(_SampleTexture2D_B39DD828_B, float2 (0,1), float2 (-1,1), _Remap_C272A01C_Out);
                    float _Property_CF380DCA_Out = _ThreadSmoothnessStrength;
                    float _Lerp_1EB6EBC0_Out;
                    Unity_Lerp_float(0, _Remap_C272A01C_Out, _Property_CF380DCA_Out, _Lerp_1EB6EBC0_Out);
                    float _Add_2975BB_Out;
                    Unity_Add_float(_Property_6FAEC412_Out, _Lerp_1EB6EBC0_Out, _Add_2975BB_Out);
                    float _Saturate_1F46047D_Out;
                    Unity_Saturate_float(_Add_2975BB_Out, _Saturate_1F46047D_Out);
                    float _Branch_1C4EA1E2_Out;
                    Unity_Branch_float(_Property_B5560A97_Out, _Saturate_1F46047D_Out, _Property_6FAEC412_Out, _Branch_1C4EA1E2_Out);
                    float _Property_57F076E2_Out = _UseThreadMap;
                    float _Property_829FEB4F_Out = _ThreadAOStrength;
                    float _Lerp_1DC743E3_Out;
                    Unity_Lerp_float(1, _SampleTexture2D_B39DD828_R, _Property_829FEB4F_Out, _Lerp_1DC743E3_Out);
                    float _Property_416E73AE_Out = _AmbientOcclusion;
                    float _Multiply_FBD87ACD_Out;
                    Unity_Multiply_float(_Lerp_1DC743E3_Out, _Property_416E73AE_Out, _Multiply_FBD87ACD_Out);
                    
                    float _Branch_A5F3B7F9_Out;
                    Unity_Branch_float(_Property_57F076E2_Out, _Multiply_FBD87ACD_Out, _Property_416E73AE_Out, _Branch_A5F3B7F9_Out);
                    float _Property_5FDD4914_Out = _Alpha;
                    float _Multiply_716B151B_Out;
                    Unity_Multiply_float(_SampleTexture2D_B39DD828_R, _Property_5FDD4914_Out, _Multiply_716B151B_Out);
                    
                    Output1 = (float4(_Branch_54FF636E_Out, 1.0));
                    Output2 = (_Branch_1C4EA1E2_Out.xxxx);
                    Output3 = (_Branch_A5F3B7F9_Out.xxxx);
                    Output4 = (_Multiply_716B151B_Out.xxxx);
                    }
                
                // Pixel Graph Evaluation
                    SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                    {
                        SurfaceDescription surface = (SurfaceDescription)0;
                        float _Property_1E54B66A_Out = _useThreadMap;
                        float4 _Property_8AE14795_Out = _uvThreadMask;
                        float4 _Property_958B7FC9_Out = _uvThreadST;
                        float4 _Subgraph_B567E108_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_958B7FC9_Out, _Property_8AE14795_Out, IN, _Subgraph_B567E108_Output1);
                        float4 _Property_FEDB20A0_Out = _uvBaseMask;
                        float4 _Property_F42AAF3B_Out = _uvBaseST;
                        float4 _Subgraph_9D4E0F1_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_F42AAF3B_Out, _Property_FEDB20A0_Out, IN, _Subgraph_9D4E0F1_Output1);
                        float4 _SampleTexture2D_105B35B3_RGBA = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, (_Subgraph_9D4E0F1_Output1.xy));
                        _SampleTexture2D_105B35B3_RGBA.rgb = UnpackNormalmapRGorAG(_SampleTexture2D_105B35B3_RGBA);
                        float _SampleTexture2D_105B35B3_R = _SampleTexture2D_105B35B3_RGBA.r;
                        float _SampleTexture2D_105B35B3_G = _SampleTexture2D_105B35B3_RGBA.g;
                        float _SampleTexture2D_105B35B3_B = _SampleTexture2D_105B35B3_RGBA.b;
                        float _SampleTexture2D_105B35B3_A = _SampleTexture2D_105B35B3_RGBA.a;
                        float _Property_82D183C3_Out = _NormalMapStrength;
                        float3 _NormalStrength_BFF5C35E_Out;
                        Unity_NormalStrength_float((_SampleTexture2D_105B35B3_RGBA.xyz), _Property_82D183C3_Out, _NormalStrength_BFF5C35E_Out);
                        float3 _Normalize_ACA4E10E_Out;
                        Unity_Normalize_float3(_NormalStrength_BFF5C35E_Out, _Normalize_ACA4E10E_Out);
                        float4 _Property_1E040901_Out = _uvBaseMask;
                        float4 _Property_97A7EF85_Out = _uvBaseST;
                        float4 _Subgraph_8DDCEE61_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_97A7EF85_Out, _Property_1E040901_Out, IN, _Subgraph_8DDCEE61_Output1);
                        float4 _SampleTexture2D_8C3CF01A_RGBA = SAMPLE_TEXTURE2D(_SpecColorMap, sampler_SpecColorMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_8C3CF01A_R = _SampleTexture2D_8C3CF01A_RGBA.r;
                        float _SampleTexture2D_8C3CF01A_G = _SampleTexture2D_8C3CF01A_RGBA.g;
                        float _SampleTexture2D_8C3CF01A_B = _SampleTexture2D_8C3CF01A_RGBA.b;
                        float _SampleTexture2D_8C3CF01A_A = _SampleTexture2D_8C3CF01A_RGBA.a;
                        float _Property_B948927_Out = _SmoothnessMin;
                        float _Property_2962A49E_Out = _SmoothnessMax;
                        float2 _Vector2_9C783D17_Out = float2(_Property_B948927_Out,_Property_2962A49E_Out);
                        float _Remap_10DEF6A_Out;
                        Unity_Remap_float(_SampleTexture2D_8C3CF01A_A, float2 (0,1), _Vector2_9C783D17_Out, _Remap_10DEF6A_Out);
                        float4 _Property_90FAF786_Out = _BaseColor;
                        float4 _SampleTexture2D_11CFD011_RGBA = SAMPLE_TEXTURE2D(_BaseColorMap, sampler_BaseColorMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_11CFD011_R = _SampleTexture2D_11CFD011_RGBA.r;
                        float _SampleTexture2D_11CFD011_G = _SampleTexture2D_11CFD011_RGBA.g;
                        float _SampleTexture2D_11CFD011_B = _SampleTexture2D_11CFD011_RGBA.b;
                        float _SampleTexture2D_11CFD011_A = _SampleTexture2D_11CFD011_RGBA.a;
                        float4 _Multiply_98A7A079_Out;
                        Unity_Multiply_float(_Property_90FAF786_Out, _SampleTexture2D_11CFD011_RGBA, _Multiply_98A7A079_Out);
                    
                        float _Property_7C6435CB_Out = _FuzzMapUVScale;
                        float4 _Multiply_18C3A780_Out;
                        Unity_Multiply_float(_Subgraph_8DDCEE61_Output1, (_Property_7C6435CB_Out.xxxx), _Multiply_18C3A780_Out);
                    
                        float4 _SampleTexture2D_4D82F05E_RGBA = SAMPLE_TEXTURE2D(_FuzzMap, sampler_FuzzMap, (_Multiply_18C3A780_Out.xy));
                        float _SampleTexture2D_4D82F05E_R = _SampleTexture2D_4D82F05E_RGBA.r;
                        float _SampleTexture2D_4D82F05E_G = _SampleTexture2D_4D82F05E_RGBA.g;
                        float _SampleTexture2D_4D82F05E_B = _SampleTexture2D_4D82F05E_RGBA.b;
                        float _SampleTexture2D_4D82F05E_A = _SampleTexture2D_4D82F05E_RGBA.a;
                        float _Property_6CCE2816_Out = _FuzzStrength;
                        float _Lerp_2C953D15_Out;
                        Unity_Lerp_float(0, _SampleTexture2D_4D82F05E_R, _Property_6CCE2816_Out, _Lerp_2C953D15_Out);
                        float4 _Add_A30FF2E2_Out;
                        Unity_Add_float4(_Multiply_98A7A079_Out, (_Lerp_2C953D15_Out.xxxx), _Add_A30FF2E2_Out);
                        float4 _Saturate_69BD2FF3_Out;
                        Unity_Saturate_float4(_Add_A30FF2E2_Out, _Saturate_69BD2FF3_Out);
                        float _Split_EB0B739F_R = _Saturate_69BD2FF3_Out[0];
                        float _Split_EB0B739F_G = _Saturate_69BD2FF3_Out[1];
                        float _Split_EB0B739F_B = _Saturate_69BD2FF3_Out[2];
                        float _Split_EB0B739F_A = _Saturate_69BD2FF3_Out[3];
                        float4 _SampleTexture2D_EECA7933_RGBA = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_EECA7933_R = _SampleTexture2D_EECA7933_RGBA.r;
                        float _SampleTexture2D_EECA7933_G = _SampleTexture2D_EECA7933_RGBA.g;
                        float _SampleTexture2D_EECA7933_B = _SampleTexture2D_EECA7933_RGBA.b;
                        float _SampleTexture2D_EECA7933_A = _SampleTexture2D_EECA7933_RGBA.a;
                        float _Property_88B45C0E_Out = _ThreadAOStrength01;
                        float _Property_FC0CC4C0_Out = _ThreadNormalStrength;
                        float _Property_AC495D22_Out = _ThreadSmoothnessScale;
                        float4 _Subgraph_E494B5B1_Output1;
                        float4 _Subgraph_E494B5B1_Output2;
                        float4 _Subgraph_E494B5B1_Output3;
                        float4 _Subgraph_E494B5B1_Output4;
                        sg_SGRThreadMapDetail_SurfaceDescriptionInputs_64D53B52((_Subgraph_B567E108_Output1.xy), TEXTURE2D_PARAM(_ThreadMap, sampler_ThreadMap), _Property_AC495D22_Out, _SampleTexture2D_EECA7933_G, _Property_1E54B66A_Out, _Property_88B45C0E_Out, _Property_FC0CC4C0_Out, _Remap_10DEF6A_Out, _Normalize_ACA4E10E_Out, _Split_EB0B739F_A, IN, _Subgraph_E494B5B1_Output1, _Subgraph_E494B5B1_Output2, _Subgraph_E494B5B1_Output3, _Subgraph_E494B5B1_Output4);
                        surface.Alpha = (_Subgraph_E494B5B1_Output4).x;
                        return surface;
                    }
                    
            //-------------------------------------------------------------------------------------
            // End graph generated code
            //-------------------------------------------------------------------------------------
        
        
        
        //-------------------------------------------------------------------------------------
        // TEMPLATE INCLUDE : SharedCode.template.hlsl
        //-------------------------------------------------------------------------------------
            FragInputs BuildFragInputs(VaryingsMeshToPS input)
            {
                FragInputs output;
                ZERO_INITIALIZE(FragInputs, output);
        
                // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                // to compute normals which are then passed on elsewhere to compute other values...
                output.worldToTangent = k_identity3x3;
                output.positionSS = input.positionCS;       // input.positionCS is SV_Position
        
                output.positionRWS = input.positionRWS;
                output.worldToTangent = BuildWorldToTangent(input.tangentWS, input.normalWS);
                output.texCoord0 = input.texCoord0;
                output.texCoord1 = input.texCoord1;
                output.texCoord2 = input.texCoord2;
                output.texCoord3 = input.texCoord3;
                output.color = input.color;
                #if SHADER_STAGE_FRAGMENT
                #endif // SHADER_STAGE_FRAGMENT
        
                return output;
            }
        
            SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
                output.uv0 =                         input.texCoord0;
                output.uv1 =                         input.texCoord1;
                output.uv2 =                         input.texCoord2;
                output.uv3 =                         input.texCoord3;
        
                return output;
            }
        
            // existing HDRP code uses the combined function to go directly from packed to frag inputs
            FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
            {
                VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                return BuildFragInputs(unpacked);
            }
        
        //-------------------------------------------------------------------------------------
        // END TEMPLATE INCLUDE : SharedCode.template.hlsl
        //-------------------------------------------------------------------------------------
        
        
            void ApplyDecalToSurfaceData(DecalSurfaceData decalSurfaceData, inout SurfaceData surfaceData)
            {
                // using alpha compositing https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch23.html
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_DIFFUSE)
                {
                    surfaceData.baseColor.xyz = surfaceData.baseColor.xyz * decalSurfaceData.baseColor.w + decalSurfaceData.baseColor.xyz;
                }
        
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_NORMAL)
                {
                    surfaceData.normalWS.xyz = normalize(surfaceData.normalWS.xyz * decalSurfaceData.normalWS.w + decalSurfaceData.normalWS.xyz);
                }
        
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_MASK)
                {
            #ifdef DECALS_4RT // only smoothness in 3RT mode
                    // Don't apply any metallic modification
                    surfaceData.ambientOcclusion = surfaceData.ambientOcclusion * decalSurfaceData.MAOSBlend.y + decalSurfaceData.mask.y;
            #endif
        
                    surfaceData.perceptualSmoothness = surfaceData.perceptualSmoothness * decalSurfaceData.mask.w + decalSurfaceData.mask.z;
                }
            }
        
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, out SurfaceData surfaceData)
            {
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(SurfaceData, surfaceData);
        
                // copy across graph values, if defined
        
        
        
        
        
        
        
        
        
                
                // These static material feature allow compile time optimization
                surfaceData.materialFeatures = 0;
        
                // Transform the preprocess macro into a material feature (note that silk flag is deduced from the abscence of this one)
                #ifdef _MATERIAL_FEATURE_COTTON_WOOL
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_COTTON_WOOL;
                #endif
        
                #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_SUBSURFACE_SCATTERING;
                #endif
        
                #ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_TRANSMISSION;
                #endif
        
        
        #if defined (_ENERGY_CONSERVING_SPECULAR)
                // Require to have setup baseColor
                // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
        #endif
        
                // tangent-space normal
                float3 normalTS = float3(0.0f, 0.0f, 1.0f);
        
                // compute world space normal
                GetNormalWS(fragInputs, normalTS, surfaceData.normalWS);
        
                surfaceData.geomNormalWS = fragInputs.worldToTangent[2];
        
                surfaceData.tangentWS = normalize(fragInputs.worldToTangent[0].xyz);    // The tangent is not normalize in worldToTangent for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
        
                // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                // If user provide bent normal then we process a better term
                surfaceData.specularOcclusion = 1.0;
        
        #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                // Just use the value passed through via the slot (not active otherwise)
        #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                // If we have bent normal and ambient occlusion, process a specular occlusion
                surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
        #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
        #else
                surfaceData.specularOcclusion = 1.0;
                surfaceData.specularOcclusion = 1.0;
        #endif
        
        #ifdef DEBUG_DISPLAY
                // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                // as it can modify attribute use for static lighting
                ApplyDebugToSurfaceData(fragInputs.worldToTangent, surfaceData);
        #endif
            }
        
            void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
            {
        #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                uint3 fadeMaskSeed = asuint((int3)(V * _ScreenSize.xyx)); // Quantize V to _ScreenSize values
                LODDitheringTransition(fadeMaskSeed, unity_LODFade.x);
        #endif
        
                // this applies the double sided tangent space correction -- see 'ApplyDoubleSidedFlipOrMirror()'
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
        
                BuildSurfaceData(fragInputs, surfaceDescription, V, surfaceData);
        
        #if HAVE_DECALS && _DECALS
                DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, surfaceDescription.Alpha);
                ApplyDecalToSurfaceData(decalSurfaceData, surfaceData);
        #endif
        
                // Builtin Data
                // For back lighting we use the oposite vertex normal 
                InitBuiltinData(surfaceDescription.Alpha, surfaceData.normalWS, -fragInputs.worldToTangent[2], fragInputs.positionRWS, fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
        
        
                builtinData.depthOffset = 0.0;                        // ApplyPerPixelDisplacement(input, V, layerTexCoord, blendMasks); #ifdef _DEPTHOFFSET_ON : ApplyDepthOffsetPositionInput(V, depthOffset, GetWorldToHClipMatrix(), posInput);
        
                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
            }
        
            //-------------------------------------------------------------------------------------
            // Pass Includes
            //-------------------------------------------------------------------------------------
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassVelocity.hlsl"
            //-------------------------------------------------------------------------------------
            // End Pass Includes
            //-------------------------------------------------------------------------------------
        
            ENDHLSL
        }
        
        Pass
        {
            // based on FabricPass.template
            Name "Forward"
            Tags { "LightMode" = "ForwardOnly" }
        
            //-------------------------------------------------------------------------------------
            // Render Modes (Blend, Cull, ZTest, Stencil, etc)
            //-------------------------------------------------------------------------------------
                Blend One Zero
        
                Cull Back
        
                ZTest LEqual
        
                ZWrite On
        
                ZClip [_ZClip]
        
                // Stencil setup
        Stencil
        {
           WriteMask 39
           Ref  33
           Comp Always
           Pass Replace
        }
        
                
            //-------------------------------------------------------------------------------------
            // End Render Modes
            //-------------------------------------------------------------------------------------
        
            HLSLPROGRAM
        
                #pragma target 4.5
                #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
                //#pragma enable_d3d11_debug_symbols
        
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
        
                #pragma multi_compile _ LOD_FADE_CROSSFADE
        
            //-------------------------------------------------------------------------------------
            // Variant Definitions (active field translations to HDRP defines)
            //-------------------------------------------------------------------------------------
                #define _MATERIAL_FEATURE_COTTON_WOOL 1
                #define _EMISSION 1
                #define _SPECULAR_OCCLUSION_FROM_AO 1
                #define _ENERGY_CONSERVING_SPECULAR 1
                #define _DISABLE_SSR 1
            //-------------------------------------------------------------------------------------
            // End Variant Definitions
            //-------------------------------------------------------------------------------------
        
            #pragma vertex Vert
            #pragma fragment Frag
        
            #define UNITY_MATERIAL_FABRIC      // Need to be define before including Material.hlsl
        
            // This will be enabled in an upcoming change. 
            // #define SURFACE_GRADIENT
        
            // If we use subsurface scattering, enable output split lighting (for forward pass)
            #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
            #define OUTPUT_SPLIT_LIGHTING
            #endif
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Wind.hlsl"
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
        
            // define FragInputs structure
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
        
            //-------------------------------------------------------------------------------------
            // Defines
            //-------------------------------------------------------------------------------------
                #define SHADERPASS SHADERPASS_FORWARD
                #pragma multi_compile _ DEBUG_DISPLAY
                #pragma multi_compile _ LIGHTMAP_ON
                #pragma multi_compile _ DIRLIGHTMAP_COMBINED
                #pragma multi_compile _ DYNAMICLIGHTMAP_ON
                #pragma multi_compile _ SHADOWS_SHADOWMASK
                #pragma multi_compile DECALS_OFF DECALS_3RT DECALS_4RT
                #define LIGHTLOOP_TILE_PASS
                #pragma multi_compile USE_FPTL_LIGHTLIST USE_CLUSTERED_LIGHTLIST
                #pragma multi_compile PUNCTUAL_SHADOW_LOW PUNCTUAL_SHADOW_MEDIUM PUNCTUAL_SHADOW_HIGH
                #pragma multi_compile DIRECTIONAL_SHADOW_LOW DIRECTIONAL_SHADOW_MEDIUM DIRECTIONAL_SHADOW_HIGH
        
                // this translates the new dependency tracker into the old preprocessor definitions for the existing HDRP shader code
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_TEXCOORD2
                #define ATTRIBUTES_NEED_TEXCOORD3
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_POSITION_WS
                #define VARYINGS_NEED_TANGENT_TO_WORLD
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_TEXCOORD2
                #define VARYINGS_NEED_TEXCOORD3
                #define VARYINGS_NEED_COLOR
        
            //-------------------------------------------------------------------------------------
            // End Defines
            //-------------------------------------------------------------------------------------
        
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #ifdef DEBUG_DISPLAY
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif
        
            #if (SHADERPASS == SHADERPASS_FORWARD)
                // used for shaders that want to do lighting (and materials)
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
            #else
                // used for shaders that don't need lighting
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #endif
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
        
            //Used by SceneSelectionPass
            int _ObjectId;
            int _PassValue;
        
            // this function assumes the bitangent flip is encoded in tangentWS.w
            // TODO: move this function to HDRP shared file, once we merge with HDRP repo
            float3x3 BuildWorldToTangent(float4 tangentWS, float3 normalWS)
            {
                // tangentWS must not be normalized (mikkts requirement)
        
                // Normalize normalWS vector but keep the renormFactor to apply it to bitangent and tangent
        	    float3 unnormalizedNormalWS = normalWS;
                float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
                // bitangent on the fly option in xnormal to reduce vertex shader outputs.
        	    // this is the mikktspace transformation (must use unnormalized attributes)
                float3x3 worldToTangent = CreateWorldToTangent(unnormalizedNormalWS, tangentWS.xyz, tangentWS.w > 0.0 ? 1.0 : -1.0);
        
        	    // surface gradient based formulation requires a unit length initial normal. We can maintain compliance with mikkts
        	    // by uniformly scaling all 3 vectors since normalization of the perturbed normal will cancel it.
                worldToTangent[0] = worldToTangent[0] * renormFactor;
                worldToTangent[1] = worldToTangent[1] * renormFactor;
                worldToTangent[2] = worldToTangent[2] * renormFactor;		// normalizes the interpolated vertex normal
                return worldToTangent;
            }
        
            //-------------------------------------------------------------------------------------
            // Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
        // Generated Type: AttributesMesh
        struct AttributesMesh {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL; // optional
            float4 tangentOS : TANGENT; // optional
            float4 uv0 : TEXCOORD0; // optional
            float4 uv1 : TEXCOORD1; // optional
            float4 uv2 : TEXCOORD2; // optional
            float4 uv3 : TEXCOORD3; // optional
            float4 color : COLOR; // optional
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        
        // Generated Type: VaryingsMeshToPS
        struct VaryingsMeshToPS {
            float4 positionCS : SV_Position;
            float3 positionRWS; // optional
            float3 normalWS; // optional
            float4 tangentWS; // optional
            float4 texCoord0; // optional
            float4 texCoord1; // optional
            float4 texCoord2; // optional
            float4 texCoord3; // optional
            float4 color; // optional
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        struct PackedVaryingsMeshToPS {
            float3 interp00 : TEXCOORD0; // auto-packed
            float3 interp01 : TEXCOORD1; // auto-packed
            float4 interp02 : TEXCOORD2; // auto-packed
            float4 interp03 : TEXCOORD3; // auto-packed
            float4 interp04 : TEXCOORD4; // auto-packed
            float4 interp05 : TEXCOORD5; // auto-packed
            float4 interp06 : TEXCOORD6; // auto-packed
            float4 interp07 : TEXCOORD7; // auto-packed
            float4 positionCS : SV_Position; // unpacked
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC; // unpacked
            #endif // INSTANCING_ON
        };
        PackedVaryingsMeshToPS PackVaryingsMeshToPS(VaryingsMeshToPS input)
        {
            PackedVaryingsMeshToPS output;
            output.positionCS = input.positionCS;
            output.interp00.xyz = input.positionRWS;
            output.interp01.xyz = input.normalWS;
            output.interp02.xyzw = input.tangentWS;
            output.interp03.xyzw = input.texCoord0;
            output.interp04.xyzw = input.texCoord1;
            output.interp05.xyzw = input.texCoord2;
            output.interp06.xyzw = input.texCoord3;
            output.interp07.xyzw = input.color;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        VaryingsMeshToPS UnpackVaryingsMeshToPS(PackedVaryingsMeshToPS input)
        {
            VaryingsMeshToPS output;
            output.positionCS = input.positionCS;
            output.positionRWS = input.interp00.xyz;
            output.normalWS = input.interp01.xyz;
            output.tangentWS = input.interp02.xyzw;
            output.texCoord0 = input.interp03.xyzw;
            output.texCoord1 = input.interp04.xyzw;
            output.texCoord2 = input.interp05.xyzw;
            output.texCoord3 = input.interp06.xyzw;
            output.color = input.interp07.xyzw;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        
        // Generated Type: VaryingsMeshToDS
        struct VaryingsMeshToDS {
            float3 positionRWS;
            float3 normalWS;
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif // INSTANCING_ON
        };
        struct PackedVaryingsMeshToDS {
            float3 interp00 : TEXCOORD0; // auto-packed
            float3 interp01 : TEXCOORD1; // auto-packed
            #if INSTANCING_ON
            uint instanceID : INSTANCEID_SEMANTIC; // unpacked
            #endif // INSTANCING_ON
        };
        PackedVaryingsMeshToDS PackVaryingsMeshToDS(VaryingsMeshToDS input)
        {
            PackedVaryingsMeshToDS output;
            output.interp00.xyz = input.positionRWS;
            output.interp01.xyz = input.normalWS;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        VaryingsMeshToDS UnpackVaryingsMeshToDS(PackedVaryingsMeshToDS input)
        {
            VaryingsMeshToDS output;
            output.positionRWS = input.interp00.xyz;
            output.normalWS = input.interp01.xyz;
            #if INSTANCING_ON
            output.instanceID = input.instanceID;
            #endif // INSTANCING_ON
            return output;
        }
        
            //-------------------------------------------------------------------------------------
            // End Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
        
            //-------------------------------------------------------------------------------------
            // Graph generated code
            //-------------------------------------------------------------------------------------
                // Shared Graph Properties (uniform inputs)
                    CBUFFER_START(UnityPerMaterial)
                    float4 _uvBaseMask;
                    float4 _uvBaseST;
                    float4 _BaseColor;
                    float _SmoothnessMin;
                    float _SmoothnessMax;
                    float4 _SpecularColor;
                    float _NormalMapStrength;
                    float _useThreadMap;
                    float4 _uvThreadMask;
                    float4 _uvThreadST;
                    float _ThreadAOStrength01;
                    float _ThreadNormalStrength;
                    float _ThreadSmoothnessScale;
                    float _FuzzMapUVScale;
                    float _FuzzStrength;
                    float4 _EmissionColor;
                    CBUFFER_END
                
                    TEXTURE2D(_BaseColorMap); SAMPLER(sampler_BaseColorMap); float4 _BaseColorMap_TexelSize;
                    TEXTURE2D(_MaskMap); SAMPLER(sampler_MaskMap); float4 _MaskMap_TexelSize;
                    TEXTURE2D(_SpecColorMap); SAMPLER(sampler_SpecColorMap); float4 _SpecColorMap_TexelSize;
                    TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap); float4 _NormalMap_TexelSize;
                    TEXTURE2D(_ThreadMap); SAMPLER(sampler_ThreadMap); float4 _ThreadMap_TexelSize;
                    TEXTURE2D(_FuzzMap); SAMPLER(sampler_FuzzMap); float4 _FuzzMap_TexelSize;
                
                // Pixel Graph Inputs
                    struct SurfaceDescriptionInputs {
                        float4 uv0; // optional
                        float4 uv1; // optional
                        float4 uv2; // optional
                        float4 uv3; // optional
                    };
                // Pixel Graph Outputs
                    struct SurfaceDescription
                    {
                        float3 Albedo;
                        float3 Normal;
                        float Smoothness;
                        float Occlusion;
                        float3 Specular;
                        float DiffusionProfile;
                        float SubsurfaceMask;
                        float Thickness;
                        float3 Emission;
                        float Alpha;
                    };
                    
                // Shared Graph Node Functions
                
                    void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
                    {
                        RGBA = float4(R, G, B, A);
                        RGB = float3(R, G, B);
                        RG = float2(R, G);
                    }
                
                    void Unity_Multiply_float (float2 A, float2 B, out float2 Out)
                    {
                        Out = A * B;
                    }
                
                    void Unity_Add_float2(float2 A, float2 B, out float2 Out)
                    {
                        Out = A + B;
                    }
                
                    // Subgraph function
                    void sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(float4 _uvST, float4 _uvMask, SurfaceDescriptionInputs IN, out float4 Output1)
                    {
                    float4 _UV_23AF8552_Out = IN.uv0;
                    float _Split_7957D60_R = _UV_23AF8552_Out[0];
                    float _Split_7957D60_G = _UV_23AF8552_Out[1];
                    float _Split_7957D60_B = _UV_23AF8552_Out[2];
                    float _Split_7957D60_A = _UV_23AF8552_Out[3];
                    float4 _Combine_5396A6C7_RGBA;
                    float3 _Combine_5396A6C7_RGB;
                    float2 _Combine_5396A6C7_RG;
                    Unity_Combine_float(_Split_7957D60_R, _Split_7957D60_G, 0, 0, _Combine_5396A6C7_RGBA, _Combine_5396A6C7_RGB, _Combine_5396A6C7_RG);
                    float4 _Property_CB55E443_Out = _uvMask;
                    float _Split_6086B0A5_R = _Property_CB55E443_Out[0];
                    float _Split_6086B0A5_G = _Property_CB55E443_Out[1];
                    float _Split_6086B0A5_B = _Property_CB55E443_Out[2];
                    float _Split_6086B0A5_A = _Property_CB55E443_Out[3];
                    float2 _Multiply_FC550A07_Out;
                    Unity_Multiply_float(_Combine_5396A6C7_RG, (_Split_6086B0A5_R.xx), _Multiply_FC550A07_Out);
                    
                    float4 _UV_3B1D042C_Out = IN.uv1;
                    float _Split_107320B6_R = _UV_3B1D042C_Out[0];
                    float _Split_107320B6_G = _UV_3B1D042C_Out[1];
                    float _Split_107320B6_B = _UV_3B1D042C_Out[2];
                    float _Split_107320B6_A = _UV_3B1D042C_Out[3];
                    float4 _Combine_2E8D3795_RGBA;
                    float3 _Combine_2E8D3795_RGB;
                    float2 _Combine_2E8D3795_RG;
                    Unity_Combine_float(_Split_107320B6_R, _Split_107320B6_G, 0, 0, _Combine_2E8D3795_RGBA, _Combine_2E8D3795_RGB, _Combine_2E8D3795_RG);
                    float2 _Multiply_FDA7BA1E_Out;
                    Unity_Multiply_float(_Combine_2E8D3795_RG, (_Split_6086B0A5_G.xx), _Multiply_FDA7BA1E_Out);
                    
                    float2 _Add_92015245_Out;
                    Unity_Add_float2(_Multiply_FC550A07_Out, _Multiply_FDA7BA1E_Out, _Add_92015245_Out);
                    float4 _UV_49BE4158_Out = IN.uv2;
                    float _Split_A24186AD_R = _UV_49BE4158_Out[0];
                    float _Split_A24186AD_G = _UV_49BE4158_Out[1];
                    float _Split_A24186AD_B = _UV_49BE4158_Out[2];
                    float _Split_A24186AD_A = _UV_49BE4158_Out[3];
                    float4 _Combine_6951B6BC_RGBA;
                    float3 _Combine_6951B6BC_RGB;
                    float2 _Combine_6951B6BC_RG;
                    Unity_Combine_float(_Split_A24186AD_R, _Split_A24186AD_G, 0, 0, _Combine_6951B6BC_RGBA, _Combine_6951B6BC_RGB, _Combine_6951B6BC_RG);
                    float2 _Multiply_1480B81_Out;
                    Unity_Multiply_float(_Combine_6951B6BC_RG, (_Split_6086B0A5_B.xx), _Multiply_1480B81_Out);
                    
                    float4 _UV_9CA65C2_Out = IN.uv3;
                    float _Split_9EC6EA10_R = _UV_9CA65C2_Out[0];
                    float _Split_9EC6EA10_G = _UV_9CA65C2_Out[1];
                    float _Split_9EC6EA10_B = _UV_9CA65C2_Out[2];
                    float _Split_9EC6EA10_A = _UV_9CA65C2_Out[3];
                    float4 _Combine_633F7D3D_RGBA;
                    float3 _Combine_633F7D3D_RGB;
                    float2 _Combine_633F7D3D_RG;
                    Unity_Combine_float(_Split_9EC6EA10_R, _Split_9EC6EA10_G, 0, 0, _Combine_633F7D3D_RGBA, _Combine_633F7D3D_RGB, _Combine_633F7D3D_RG);
                    float2 _Multiply_2A2B5227_Out;
                    Unity_Multiply_float(_Combine_633F7D3D_RG, (_Split_6086B0A5_A.xx), _Multiply_2A2B5227_Out);
                    
                    float2 _Add_B5E7679D_Out;
                    Unity_Add_float2(_Multiply_1480B81_Out, _Multiply_2A2B5227_Out, _Add_B5E7679D_Out);
                    float2 _Add_892742E3_Out;
                    Unity_Add_float2(_Add_92015245_Out, _Add_B5E7679D_Out, _Add_892742E3_Out);
                    float4 _Property_8DA1B077_Out = _uvST;
                    float _Split_1AB0DA31_R = _Property_8DA1B077_Out[0];
                    float _Split_1AB0DA31_G = _Property_8DA1B077_Out[1];
                    float _Split_1AB0DA31_B = _Property_8DA1B077_Out[2];
                    float _Split_1AB0DA31_A = _Property_8DA1B077_Out[3];
                    float4 _Combine_44459F1_RGBA;
                    float3 _Combine_44459F1_RGB;
                    float2 _Combine_44459F1_RG;
                    Unity_Combine_float(_Split_1AB0DA31_R, _Split_1AB0DA31_G, 0, 0, _Combine_44459F1_RGBA, _Combine_44459F1_RGB, _Combine_44459F1_RG);
                    float2 _Multiply_38815E23_Out;
                    Unity_Multiply_float(_Add_892742E3_Out, _Combine_44459F1_RG, _Multiply_38815E23_Out);
                    
                    float _Split_35A1DC4_R = _Property_8DA1B077_Out[0];
                    float _Split_35A1DC4_G = _Property_8DA1B077_Out[1];
                    float _Split_35A1DC4_B = _Property_8DA1B077_Out[2];
                    float _Split_35A1DC4_A = _Property_8DA1B077_Out[3];
                    float4 _Combine_91984BDF_RGBA;
                    float3 _Combine_91984BDF_RGB;
                    float2 _Combine_91984BDF_RG;
                    Unity_Combine_float(_Split_35A1DC4_B, _Split_35A1DC4_A, 0, 0, _Combine_91984BDF_RGBA, _Combine_91984BDF_RGB, _Combine_91984BDF_RG);
                    float2 _Add_63012CEE_Out;
                    Unity_Add_float2(_Multiply_38815E23_Out, _Combine_91984BDF_RG, _Add_63012CEE_Out);
                    Output1 = (float4(_Add_63012CEE_Out, 0.0, 1.0));
                    }
                
                    void Unity_Multiply_float (float4 A, float4 B, out float4 Out)
                    {
                        Out = A * B;
                    }
                
                    void Unity_Lerp_float(float A, float B, float T, out float Out)
                    {
                        Out = lerp(A, B, T);
                    }
                
                    void Unity_Add_float4(float4 A, float4 B, out float4 Out)
                    {
                        Out = A + B;
                    }
                
                    void Unity_Saturate_float4(float4 In, out float4 Out)
                    {
                        Out = saturate(In);
                    }
                
                    void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
                    {
                        Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
                    }
                
                    void Unity_Normalize_float3(float3 In, out float3 Out)
                    {
                        Out = normalize(In);
                    }
                
                    void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
                    {
                        Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
                    }
                
                    void Unity_NormalUnpack_float(float4 In, out float3 Out)
                    {
                                    Out = UnpackNormalmapRGorAG(In);
                                }
                
                    void Unity_NormalBlend_float(float3 A, float3 B, out float3 Out)
                    {
                        Out = normalize(float3(A.rg + B.rg, A.b * B.b));
                    }
                
                    void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
                    {
                        Out = lerp(False, True, Predicate);
                    }
                
                    void Unity_Add_float(float A, float B, out float Out)
                    {
                        Out = A + B;
                    }
                
                    void Unity_Saturate_float(float In, out float Out)
                    {
                        Out = saturate(In);
                    }
                
                    void Unity_Branch_float(float Predicate, float True, float False, out float Out)
                    {
                        Out = lerp(False, True, Predicate);
                    }
                
                    void Unity_Multiply_float (float A, float B, out float Out)
                    {
                        Out = A * B;
                    }
                
                    // Subgraph function
                    void sg_SGRThreadMapDetail_SurfaceDescriptionInputs_64D53B52(float2 _UV, TEXTURE2D_ARGS(_ThreadMap, sampler_ThreadMap), float _ThreadSmoothnessStrength, float _AmbientOcclusion, float _UseThreadMap, float _ThreadAOStrength, float _ThreadNormalStrength, float _Smoothness, float3 _Normals, float _Alpha, SurfaceDescriptionInputs IN, out float4 Output1, out float4 Output2, out float4 Output3, out float4 Output4)
                    {
                    float _Property_7B789410_Out = _UseThreadMap;
                    float3 _Property_D380C535_Out = _Normals;
                    float2 _Property_247E83DC_Out = _UV;
                    float4 _SampleTexture2D_B39DD828_RGBA = SAMPLE_TEXTURE2D(_ThreadMap, sampler_ThreadMap, _Property_247E83DC_Out);
                    float _SampleTexture2D_B39DD828_R = _SampleTexture2D_B39DD828_RGBA.r;
                    float _SampleTexture2D_B39DD828_G = _SampleTexture2D_B39DD828_RGBA.g;
                    float _SampleTexture2D_B39DD828_B = _SampleTexture2D_B39DD828_RGBA.b;
                    float _SampleTexture2D_B39DD828_A = _SampleTexture2D_B39DD828_RGBA.a;
                    float4 _Combine_3989CE7_RGBA;
                    float3 _Combine_3989CE7_RGB;
                    float2 _Combine_3989CE7_RG;
                    Unity_Combine_float(_SampleTexture2D_B39DD828_A, _SampleTexture2D_B39DD828_G, 1, 1, _Combine_3989CE7_RGBA, _Combine_3989CE7_RGB, _Combine_3989CE7_RG);
                    float3 _NormalUnpack_6B39F6EC_Out;
                    Unity_NormalUnpack_float((float4(_Combine_3989CE7_RGB, 1.0)), _NormalUnpack_6B39F6EC_Out);
                    float3 _Normalize_1F52E5EC_Out;
                    Unity_Normalize_float3(_NormalUnpack_6B39F6EC_Out, _Normalize_1F52E5EC_Out);
                    float _Property_2E175598_Out = _ThreadNormalStrength;
                    float3 _NormalStrength_A15875A3_Out;
                    Unity_NormalStrength_float(_Normalize_1F52E5EC_Out, _Property_2E175598_Out, _NormalStrength_A15875A3_Out);
                    float3 _NormalBlend_191D51BE_Out;
                    Unity_NormalBlend_float(_Property_D380C535_Out, _NormalStrength_A15875A3_Out, _NormalBlend_191D51BE_Out);
                    float3 _Normalize_4D9B04E_Out;
                    Unity_Normalize_float3(_NormalBlend_191D51BE_Out, _Normalize_4D9B04E_Out);
                    float3 _Branch_54FF636E_Out;
                    Unity_Branch_float3(_Property_7B789410_Out, _Normalize_4D9B04E_Out, _Property_D380C535_Out, _Branch_54FF636E_Out);
                    float _Property_B5560A97_Out = _UseThreadMap;
                    float _Property_6FAEC412_Out = _Smoothness;
                    float _Remap_C272A01C_Out;
                    Unity_Remap_float(_SampleTexture2D_B39DD828_B, float2 (0,1), float2 (-1,1), _Remap_C272A01C_Out);
                    float _Property_CF380DCA_Out = _ThreadSmoothnessStrength;
                    float _Lerp_1EB6EBC0_Out;
                    Unity_Lerp_float(0, _Remap_C272A01C_Out, _Property_CF380DCA_Out, _Lerp_1EB6EBC0_Out);
                    float _Add_2975BB_Out;
                    Unity_Add_float(_Property_6FAEC412_Out, _Lerp_1EB6EBC0_Out, _Add_2975BB_Out);
                    float _Saturate_1F46047D_Out;
                    Unity_Saturate_float(_Add_2975BB_Out, _Saturate_1F46047D_Out);
                    float _Branch_1C4EA1E2_Out;
                    Unity_Branch_float(_Property_B5560A97_Out, _Saturate_1F46047D_Out, _Property_6FAEC412_Out, _Branch_1C4EA1E2_Out);
                    float _Property_57F076E2_Out = _UseThreadMap;
                    float _Property_829FEB4F_Out = _ThreadAOStrength;
                    float _Lerp_1DC743E3_Out;
                    Unity_Lerp_float(1, _SampleTexture2D_B39DD828_R, _Property_829FEB4F_Out, _Lerp_1DC743E3_Out);
                    float _Property_416E73AE_Out = _AmbientOcclusion;
                    float _Multiply_FBD87ACD_Out;
                    Unity_Multiply_float(_Lerp_1DC743E3_Out, _Property_416E73AE_Out, _Multiply_FBD87ACD_Out);
                    
                    float _Branch_A5F3B7F9_Out;
                    Unity_Branch_float(_Property_57F076E2_Out, _Multiply_FBD87ACD_Out, _Property_416E73AE_Out, _Branch_A5F3B7F9_Out);
                    float _Property_5FDD4914_Out = _Alpha;
                    float _Multiply_716B151B_Out;
                    Unity_Multiply_float(_SampleTexture2D_B39DD828_R, _Property_5FDD4914_Out, _Multiply_716B151B_Out);
                    
                    Output1 = (float4(_Branch_54FF636E_Out, 1.0));
                    Output2 = (_Branch_1C4EA1E2_Out.xxxx);
                    Output3 = (_Branch_A5F3B7F9_Out.xxxx);
                    Output4 = (_Multiply_716B151B_Out.xxxx);
                    }
                
                // Pixel Graph Evaluation
                    SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                    {
                        SurfaceDescription surface = (SurfaceDescription)0;
                        float4 _Property_90FAF786_Out = _BaseColor;
                        float4 _Property_1E040901_Out = _uvBaseMask;
                        float4 _Property_97A7EF85_Out = _uvBaseST;
                        float4 _Subgraph_8DDCEE61_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_97A7EF85_Out, _Property_1E040901_Out, IN, _Subgraph_8DDCEE61_Output1);
                        float4 _SampleTexture2D_11CFD011_RGBA = SAMPLE_TEXTURE2D(_BaseColorMap, sampler_BaseColorMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_11CFD011_R = _SampleTexture2D_11CFD011_RGBA.r;
                        float _SampleTexture2D_11CFD011_G = _SampleTexture2D_11CFD011_RGBA.g;
                        float _SampleTexture2D_11CFD011_B = _SampleTexture2D_11CFD011_RGBA.b;
                        float _SampleTexture2D_11CFD011_A = _SampleTexture2D_11CFD011_RGBA.a;
                        float4 _Multiply_98A7A079_Out;
                        Unity_Multiply_float(_Property_90FAF786_Out, _SampleTexture2D_11CFD011_RGBA, _Multiply_98A7A079_Out);
                    
                        float _Property_7C6435CB_Out = _FuzzMapUVScale;
                        float4 _Multiply_18C3A780_Out;
                        Unity_Multiply_float(_Subgraph_8DDCEE61_Output1, (_Property_7C6435CB_Out.xxxx), _Multiply_18C3A780_Out);
                    
                        float4 _SampleTexture2D_4D82F05E_RGBA = SAMPLE_TEXTURE2D(_FuzzMap, sampler_FuzzMap, (_Multiply_18C3A780_Out.xy));
                        float _SampleTexture2D_4D82F05E_R = _SampleTexture2D_4D82F05E_RGBA.r;
                        float _SampleTexture2D_4D82F05E_G = _SampleTexture2D_4D82F05E_RGBA.g;
                        float _SampleTexture2D_4D82F05E_B = _SampleTexture2D_4D82F05E_RGBA.b;
                        float _SampleTexture2D_4D82F05E_A = _SampleTexture2D_4D82F05E_RGBA.a;
                        float _Property_6CCE2816_Out = _FuzzStrength;
                        float _Lerp_2C953D15_Out;
                        Unity_Lerp_float(0, _SampleTexture2D_4D82F05E_R, _Property_6CCE2816_Out, _Lerp_2C953D15_Out);
                        float4 _Add_A30FF2E2_Out;
                        Unity_Add_float4(_Multiply_98A7A079_Out, (_Lerp_2C953D15_Out.xxxx), _Add_A30FF2E2_Out);
                        float4 _Saturate_69BD2FF3_Out;
                        Unity_Saturate_float4(_Add_A30FF2E2_Out, _Saturate_69BD2FF3_Out);
                        float _Property_1E54B66A_Out = _useThreadMap;
                        float4 _Property_8AE14795_Out = _uvThreadMask;
                        float4 _Property_958B7FC9_Out = _uvThreadST;
                        float4 _Subgraph_B567E108_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_958B7FC9_Out, _Property_8AE14795_Out, IN, _Subgraph_B567E108_Output1);
                        float4 _Property_FEDB20A0_Out = _uvBaseMask;
                        float4 _Property_F42AAF3B_Out = _uvBaseST;
                        float4 _Subgraph_9D4E0F1_Output1;
                        sg_SGRuvCombine_SurfaceDescriptionInputs_EDB73AA(_Property_F42AAF3B_Out, _Property_FEDB20A0_Out, IN, _Subgraph_9D4E0F1_Output1);
                        float4 _SampleTexture2D_105B35B3_RGBA = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, (_Subgraph_9D4E0F1_Output1.xy));
                        _SampleTexture2D_105B35B3_RGBA.rgb = UnpackNormalmapRGorAG(_SampleTexture2D_105B35B3_RGBA);
                        float _SampleTexture2D_105B35B3_R = _SampleTexture2D_105B35B3_RGBA.r;
                        float _SampleTexture2D_105B35B3_G = _SampleTexture2D_105B35B3_RGBA.g;
                        float _SampleTexture2D_105B35B3_B = _SampleTexture2D_105B35B3_RGBA.b;
                        float _SampleTexture2D_105B35B3_A = _SampleTexture2D_105B35B3_RGBA.a;
                        float _Property_82D183C3_Out = _NormalMapStrength;
                        float3 _NormalStrength_BFF5C35E_Out;
                        Unity_NormalStrength_float((_SampleTexture2D_105B35B3_RGBA.xyz), _Property_82D183C3_Out, _NormalStrength_BFF5C35E_Out);
                        float3 _Normalize_ACA4E10E_Out;
                        Unity_Normalize_float3(_NormalStrength_BFF5C35E_Out, _Normalize_ACA4E10E_Out);
                        float4 _SampleTexture2D_8C3CF01A_RGBA = SAMPLE_TEXTURE2D(_SpecColorMap, sampler_SpecColorMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_8C3CF01A_R = _SampleTexture2D_8C3CF01A_RGBA.r;
                        float _SampleTexture2D_8C3CF01A_G = _SampleTexture2D_8C3CF01A_RGBA.g;
                        float _SampleTexture2D_8C3CF01A_B = _SampleTexture2D_8C3CF01A_RGBA.b;
                        float _SampleTexture2D_8C3CF01A_A = _SampleTexture2D_8C3CF01A_RGBA.a;
                        float _Property_B948927_Out = _SmoothnessMin;
                        float _Property_2962A49E_Out = _SmoothnessMax;
                        float2 _Vector2_9C783D17_Out = float2(_Property_B948927_Out,_Property_2962A49E_Out);
                        float _Remap_10DEF6A_Out;
                        Unity_Remap_float(_SampleTexture2D_8C3CF01A_A, float2 (0,1), _Vector2_9C783D17_Out, _Remap_10DEF6A_Out);
                        float _Split_EB0B739F_R = _Saturate_69BD2FF3_Out[0];
                        float _Split_EB0B739F_G = _Saturate_69BD2FF3_Out[1];
                        float _Split_EB0B739F_B = _Saturate_69BD2FF3_Out[2];
                        float _Split_EB0B739F_A = _Saturate_69BD2FF3_Out[3];
                        float4 _SampleTexture2D_EECA7933_RGBA = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, (_Subgraph_8DDCEE61_Output1.xy));
                        float _SampleTexture2D_EECA7933_R = _SampleTexture2D_EECA7933_RGBA.r;
                        float _SampleTexture2D_EECA7933_G = _SampleTexture2D_EECA7933_RGBA.g;
                        float _SampleTexture2D_EECA7933_B = _SampleTexture2D_EECA7933_RGBA.b;
                        float _SampleTexture2D_EECA7933_A = _SampleTexture2D_EECA7933_RGBA.a;
                        float _Property_88B45C0E_Out = _ThreadAOStrength01;
                        float _Property_FC0CC4C0_Out = _ThreadNormalStrength;
                        float _Property_AC495D22_Out = _ThreadSmoothnessScale;
                        float4 _Subgraph_E494B5B1_Output1;
                        float4 _Subgraph_E494B5B1_Output2;
                        float4 _Subgraph_E494B5B1_Output3;
                        float4 _Subgraph_E494B5B1_Output4;
                        sg_SGRThreadMapDetail_SurfaceDescriptionInputs_64D53B52((_Subgraph_B567E108_Output1.xy), TEXTURE2D_PARAM(_ThreadMap, sampler_ThreadMap), _Property_AC495D22_Out, _SampleTexture2D_EECA7933_G, _Property_1E54B66A_Out, _Property_88B45C0E_Out, _Property_FC0CC4C0_Out, _Remap_10DEF6A_Out, _Normalize_ACA4E10E_Out, _Split_EB0B739F_A, IN, _Subgraph_E494B5B1_Output1, _Subgraph_E494B5B1_Output2, _Subgraph_E494B5B1_Output3, _Subgraph_E494B5B1_Output4);
                        float4 _Property_BFE334DC_Out = _SpecularColor;
                        surface.Albedo = (_Saturate_69BD2FF3_Out.xyz);
                        surface.Normal = (_Subgraph_E494B5B1_Output1.xyz);
                        surface.Smoothness = (_Subgraph_E494B5B1_Output2).x;
                        surface.Occlusion = (_Subgraph_E494B5B1_Output3).x;
                        surface.Specular = (_Property_BFE334DC_Out.xyz);
                        surface.DiffusionProfile = 4;
                        surface.SubsurfaceMask = _SampleTexture2D_EECA7933_B;
                        surface.Thickness = _SampleTexture2D_EECA7933_R;
                        surface.Emission = float3(0, 0, 0);
                        surface.Alpha = (_Subgraph_E494B5B1_Output4).x;
                        return surface;
                    }
                    
            //-------------------------------------------------------------------------------------
            // End graph generated code
            //-------------------------------------------------------------------------------------
        
        
        
        //-------------------------------------------------------------------------------------
        // TEMPLATE INCLUDE : SharedCode.template.hlsl
        //-------------------------------------------------------------------------------------
            FragInputs BuildFragInputs(VaryingsMeshToPS input)
            {
                FragInputs output;
                ZERO_INITIALIZE(FragInputs, output);
        
                // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                // to compute normals which are then passed on elsewhere to compute other values...
                output.worldToTangent = k_identity3x3;
                output.positionSS = input.positionCS;       // input.positionCS is SV_Position
        
                output.positionRWS = input.positionRWS;
                output.worldToTangent = BuildWorldToTangent(input.tangentWS, input.normalWS);
                output.texCoord0 = input.texCoord0;
                output.texCoord1 = input.texCoord1;
                output.texCoord2 = input.texCoord2;
                output.texCoord3 = input.texCoord3;
                output.color = input.color;
                #if SHADER_STAGE_FRAGMENT
                #endif // SHADER_STAGE_FRAGMENT
        
                return output;
            }
        
            SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
                output.uv0 =                         input.texCoord0;
                output.uv1 =                         input.texCoord1;
                output.uv2 =                         input.texCoord2;
                output.uv3 =                         input.texCoord3;
        
                return output;
            }
        
            // existing HDRP code uses the combined function to go directly from packed to frag inputs
            FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
            {
                VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                return BuildFragInputs(unpacked);
            }
        
        //-------------------------------------------------------------------------------------
        // END TEMPLATE INCLUDE : SharedCode.template.hlsl
        //-------------------------------------------------------------------------------------
        
        
            void ApplyDecalToSurfaceData(DecalSurfaceData decalSurfaceData, inout SurfaceData surfaceData)
            {
                // using alpha compositing https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch23.html
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_DIFFUSE)
                {
                    surfaceData.baseColor.xyz = surfaceData.baseColor.xyz * decalSurfaceData.baseColor.w + decalSurfaceData.baseColor.xyz;
                }
        
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_NORMAL)
                {
                    surfaceData.normalWS.xyz = normalize(surfaceData.normalWS.xyz * decalSurfaceData.normalWS.w + decalSurfaceData.normalWS.xyz);
                }
        
                if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_MASK)
                {
            #ifdef DECALS_4RT // only smoothness in 3RT mode
                    // Don't apply any metallic modification
                    surfaceData.ambientOcclusion = surfaceData.ambientOcclusion * decalSurfaceData.MAOSBlend.y + decalSurfaceData.mask.y;
            #endif
        
                    surfaceData.perceptualSmoothness = surfaceData.perceptualSmoothness * decalSurfaceData.mask.w + decalSurfaceData.mask.z;
                }
            }
        
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, out SurfaceData surfaceData)
            {
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(SurfaceData, surfaceData);
        
                // copy across graph values, if defined
                surfaceData.baseColor =                 surfaceDescription.Albedo;
        
        
                surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
        
                surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
        
                surfaceData.specularColor =             surfaceDescription.Specular;
        
                surfaceData.diffusionProfile =          surfaceDescription.DiffusionProfile;
        
                surfaceData.subsurfaceMask =            surfaceDescription.SubsurfaceMask;
        
                surfaceData.thickness =                 surfaceDescription.Thickness;
        
                surfaceData.diffusionProfile =          surfaceDescription.DiffusionProfile;
        
                
                // These static material feature allow compile time optimization
                surfaceData.materialFeatures = 0;
        
                // Transform the preprocess macro into a material feature (note that silk flag is deduced from the abscence of this one)
                #ifdef _MATERIAL_FEATURE_COTTON_WOOL
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_COTTON_WOOL;
                #endif
        
                #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_SUBSURFACE_SCATTERING;
                #endif
        
                #ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_FABRIC_TRANSMISSION;
                #endif
        
        
        #if defined (_ENERGY_CONSERVING_SPECULAR)
                // Require to have setup baseColor
                // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
        #endif
        
                // tangent-space normal
                float3 normalTS = float3(0.0f, 0.0f, 1.0f);
                normalTS = surfaceDescription.Normal;
        
                // compute world space normal
                GetNormalWS(fragInputs, normalTS, surfaceData.normalWS);
        
                surfaceData.geomNormalWS = fragInputs.worldToTangent[2];
        
                surfaceData.tangentWS = normalize(fragInputs.worldToTangent[0].xyz);    // The tangent is not normalize in worldToTangent for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
        
                // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                // If user provide bent normal then we process a better term
                surfaceData.specularOcclusion = 1.0;
        
        #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                // Just use the value passed through via the slot (not active otherwise)
        #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                // If we have bent normal and ambient occlusion, process a specular occlusion
                surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
        #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
        #else
                surfaceData.specularOcclusion = 1.0;
                surfaceData.specularOcclusion = 1.0;
        #endif
        
        #ifdef DEBUG_DISPLAY
                // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                // as it can modify attribute use for static lighting
                ApplyDebugToSurfaceData(fragInputs.worldToTangent, surfaceData);
        #endif
            }
        
            void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
            {
        #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                uint3 fadeMaskSeed = asuint((int3)(V * _ScreenSize.xyx)); // Quantize V to _ScreenSize values
                LODDitheringTransition(fadeMaskSeed, unity_LODFade.x);
        #endif
        
                // this applies the double sided tangent space correction -- see 'ApplyDoubleSidedFlipOrMirror()'
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
        
                BuildSurfaceData(fragInputs, surfaceDescription, V, surfaceData);
        
        #if HAVE_DECALS && _DECALS
                DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, surfaceDescription.Alpha);
                ApplyDecalToSurfaceData(decalSurfaceData, surfaceData);
        #endif
        
                // Builtin Data
                // For back lighting we use the oposite vertex normal 
                InitBuiltinData(surfaceDescription.Alpha, surfaceData.normalWS, -fragInputs.worldToTangent[2], fragInputs.positionRWS, fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
        
                builtinData.emissiveColor = surfaceDescription.Emission;
        
                builtinData.depthOffset = 0.0;                        // ApplyPerPixelDisplacement(input, V, layerTexCoord, blendMasks); #ifdef _DEPTHOFFSET_ON : ApplyDepthOffsetPositionInput(V, depthOffset, GetWorldToHClipMatrix(), posInput);
        
                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
            }
        
            //-------------------------------------------------------------------------------------
            // Pass Includes
            //-------------------------------------------------------------------------------------
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassForward.hlsl"
            //-------------------------------------------------------------------------------------
            // End Pass Includes
            //-------------------------------------------------------------------------------------
        
            ENDHLSL
        }
        
    }
    FallBack "Hidden/InternalErrorShader"
}
