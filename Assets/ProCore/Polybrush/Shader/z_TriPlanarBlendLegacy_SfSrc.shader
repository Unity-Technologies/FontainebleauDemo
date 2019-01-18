// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with '_Object2World'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:Legacy Shaders/Diffuse,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:2865,x:32719,y:32712,cmnt:define Z_TEXTURE_CHANNELS 4,varname:node_2865,prsc:2|diff-467-OUT;n:type:ShaderForge.SFN_Tex2d,id:7736,x:31778,y:32662,varname:_MainTex,prsc:2,ntxv:0,isnm:False|UVIN-6565-OUT,TEX-1383-TEX;n:type:ShaderForge.SFN_NormalVector,id:4413,x:30693,y:32811,prsc:2,pt:False;n:type:ShaderForge.SFN_FragmentPosition,id:2156,x:30793,y:33063,varname:node_2156,prsc:2;n:type:ShaderForge.SFN_Append,id:6565,x:31114,y:32951,varname:node_6565,prsc:2|A-2156-Y,B-2156-Z;n:type:ShaderForge.SFN_Append,id:311,x:31114,y:33214,varname:node_311,prsc:2|A-2156-Y,B-2156-X;n:type:ShaderForge.SFN_Append,id:3477,x:31114,y:33083,varname:node_3477,prsc:2|A-2156-X,B-2156-Z;n:type:ShaderForge.SFN_ChannelBlend,id:9004,x:32075,y:32753,varname:node_9004,prsc:2,chbt:0|M-3954-OUT,R-7736-RGB,G-3536-RGB,B-3386-RGB;n:type:ShaderForge.SFN_Abs,id:8849,x:30890,y:32811,varname:node_8849,prsc:2|IN-4413-OUT;n:type:ShaderForge.SFN_Tex2d,id:3536,x:31778,y:32793,varname:node_3536,prsc:2,ntxv:0,isnm:False|UVIN-3477-OUT,TEX-1383-TEX;n:type:ShaderForge.SFN_Tex2d,id:3386,x:31778,y:32922,varname:node_3386,prsc:2,ntxv:0,isnm:False|UVIN-311-OUT,TEX-1383-TEX;n:type:ShaderForge.SFN_Multiply,id:3954,x:31114,y:32811,varname:node_3954,prsc:2|A-8849-OUT,B-8849-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:1383,x:31475,y:32837,ptovrint:False,ptlb:Texture 1,ptin:_Texture1,varname:node_1383,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_ChannelBlend,id:3889,x:32081,y:33166,varname:node_3889,prsc:2,chbt:0|M-3954-OUT,R-5742-RGB,G-4501-RGB,B-2195-RGB;n:type:ShaderForge.SFN_Tex2dAsset,id:8684,x:31405,y:33198,ptovrint:False,ptlb:Texture 2,ptin:_Texture2,varname:node_8684,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:5742,x:31778,y:33099,varname:node_5742,prsc:2,ntxv:0,isnm:False|UVIN-6565-OUT,TEX-8684-TEX;n:type:ShaderForge.SFN_Tex2d,id:4501,x:31778,y:33229,varname:node_4501,prsc:2,ntxv:0,isnm:False|UVIN-3477-OUT,TEX-8684-TEX;n:type:ShaderForge.SFN_Tex2d,id:2195,x:31778,y:33357,varname:node_2195,prsc:2,ntxv:0,isnm:False|UVIN-311-OUT,TEX-8684-TEX;n:type:ShaderForge.SFN_Tex2d,id:972,x:31774,y:33567,varname:node_972,prsc:2,ntxv:0,isnm:False|UVIN-6565-OUT,TEX-2519-TEX;n:type:ShaderForge.SFN_Tex2d,id:4295,x:31774,y:33694,varname:node_4295,prsc:2,ntxv:0,isnm:False|UVIN-3477-OUT,TEX-2519-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:2519,x:31413,y:33511,ptovrint:False,ptlb:Texture 3,ptin:_Texture3,varname:node_2519,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:1734,x:31774,y:33819,varname:node_1734,prsc:2,ntxv:0,isnm:False|UVIN-311-OUT,TEX-2519-TEX;n:type:ShaderForge.SFN_ChannelBlend,id:4831,x:32098,y:33713,varname:node_4831,prsc:2,chbt:0|M-3954-OUT,R-972-RGB,G-4295-RGB,B-1734-RGB;n:type:ShaderForge.SFN_TexCoord,id:8482,x:32286,y:33230,varname:node_8482,prsc:2,uv:2;n:type:ShaderForge.SFN_Append,id:1190,x:32512,y:33317,varname:node_1190,prsc:2|A-8482-U,B-8482-V,C-2380-Z,D-2380-W;n:type:ShaderForge.SFN_Vector4Property,id:2380,x:32286,y:33423,ptovrint:False,ptlb:TEMP_CHANNEL_UV2,ptin:_TEMP_CHANNEL_UV2,varname:node_2380,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_ChannelBlend,id:467,x:32360,y:32895,cmnt:define Z_MESH_ATTRIBUTES UV3,varname:node_467,prsc:2,chbt:0|M-1190-OUT,R-9004-OUT,G-3889-OUT,B-4831-OUT,A-6986-OUT;n:type:ShaderForge.SFN_ChannelBlend,id:6986,x:32114,y:34151,varname:node_6986,prsc:2,chbt:0|M-3954-OUT,R-7081-RGB,G-3235-RGB,B-9532-RGB;n:type:ShaderForge.SFN_Tex2d,id:7081,x:31774,y:33993,varname:node_7081,prsc:2,ntxv:0,isnm:False|UVIN-6565-OUT,TEX-3669-TEX;n:type:ShaderForge.SFN_Tex2d,id:3235,x:31774,y:34124,varname:node_3235,prsc:2,ntxv:0,isnm:False|UVIN-3477-OUT,TEX-3669-TEX;n:type:ShaderForge.SFN_Tex2d,id:9532,x:31774,y:34256,varname:node_9532,prsc:2,ntxv:0,isnm:False|UVIN-311-OUT,TEX-3669-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:3669,x:31414,y:33796,ptovrint:False,ptlb:Texture 4,ptin:_Texture4,varname:node_3669,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;proporder:1383-8684-2380-2519-3669;pass:END;sub:END;*/

Shader "Hidden/Polybrush/TriPlanar Blend Legacy" {
    Properties {
        _Texture1 ("Texture 1", 2D) = "white" {}
        _Texture2 ("Texture 2", 2D) = "white" {}
        _TEMP_CHANNEL_UV2 ("TEMP_CHANNEL_UV2", Vector) = (0,0,0,0)
        _Texture3 ("Texture 3", 2D) = "white" {}
        _Texture4 ("Texture 4", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Texture1; uniform float4 _Texture1_ST;
            uniform sampler2D _Texture2; uniform float4 _Texture2_ST;
            uniform sampler2D _Texture3; uniform float4 _Texture3_ST;
            uniform float4 _TEMP_CHANNEL_UV2;
            uniform sampler2D _Texture4; uniform float4 _Texture4_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv1 : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
                LIGHTING_COORDS(6,7)
                UNITY_FOG_COORDS(8)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD9;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #elif UNITY_SHOULD_SAMPLE_SH
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - 0;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                float4 node_1190 = float4(i.uv2.r,i.uv2.g,_TEMP_CHANNEL_UV2.b,_TEMP_CHANNEL_UV2.a);
                float3 node_8849 = abs(i.normalDir);
                float3 node_3954 = (node_8849*node_8849);
                float2 node_6565 = float2(i.posWorld.g,i.posWorld.b);
                float4 _MainTex = tex2D(_Texture1,TRANSFORM_TEX(node_6565, _Texture1));
                float2 node_3477 = float2(i.posWorld.r,i.posWorld.b);
                float4 node_3536 = tex2D(_Texture1,TRANSFORM_TEX(node_3477, _Texture1));
                float2 node_311 = float2(i.posWorld.g,i.posWorld.r);
                float4 node_3386 = tex2D(_Texture1,TRANSFORM_TEX(node_311, _Texture1));
                float4 node_5742 = tex2D(_Texture2,TRANSFORM_TEX(node_6565, _Texture2));
                float4 node_4501 = tex2D(_Texture2,TRANSFORM_TEX(node_3477, _Texture2));
                float4 node_2195 = tex2D(_Texture2,TRANSFORM_TEX(node_311, _Texture2));
                float4 node_972 = tex2D(_Texture3,TRANSFORM_TEX(node_6565, _Texture3));
                float4 node_4295 = tex2D(_Texture3,TRANSFORM_TEX(node_3477, _Texture3));
                float4 node_1734 = tex2D(_Texture3,TRANSFORM_TEX(node_311, _Texture3));
                float4 node_7081 = tex2D(_Texture4,TRANSFORM_TEX(node_6565, _Texture4));
                float4 node_3235 = tex2D(_Texture4,TRANSFORM_TEX(node_3477, _Texture4));
                float4 node_9532 = tex2D(_Texture4,TRANSFORM_TEX(node_311, _Texture4));
                float3 diffuseColor = (node_1190.r*(node_3954.r*_MainTex.rgb + node_3954.g*node_3536.rgb + node_3954.b*node_3386.rgb) + node_1190.g*(node_3954.r*node_5742.rgb + node_3954.g*node_4501.rgb + node_3954.b*node_2195.rgb) + node_1190.b*(node_3954.r*node_972.rgb + node_3954.g*node_4295.rgb + node_3954.b*node_1734.rgb) + node_1190.a*(node_3954.r*node_7081.rgb + node_3954.g*node_3235.rgb + node_3954.b*node_9532.rgb));
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Texture1; uniform float4 _Texture1_ST;
            uniform sampler2D _Texture2; uniform float4 _Texture2_ST;
            uniform sampler2D _Texture3; uniform float4 _Texture3_ST;
            uniform float4 _TEMP_CHANNEL_UV2;
            uniform sampler2D _Texture4; uniform float4 _Texture4_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv1 : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
                LIGHTING_COORDS(6,7)
                UNITY_FOG_COORDS(8)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float4 node_1190 = float4(i.uv2.r,i.uv2.g,_TEMP_CHANNEL_UV2.b,_TEMP_CHANNEL_UV2.a);
                float3 node_8849 = abs(i.normalDir);
                float3 node_3954 = (node_8849*node_8849);
                float2 node_6565 = float2(i.posWorld.g,i.posWorld.b);
                float4 _MainTex = tex2D(_Texture1,TRANSFORM_TEX(node_6565, _Texture1));
                float2 node_3477 = float2(i.posWorld.r,i.posWorld.b);
                float4 node_3536 = tex2D(_Texture1,TRANSFORM_TEX(node_3477, _Texture1));
                float2 node_311 = float2(i.posWorld.g,i.posWorld.r);
                float4 node_3386 = tex2D(_Texture1,TRANSFORM_TEX(node_311, _Texture1));
                float4 node_5742 = tex2D(_Texture2,TRANSFORM_TEX(node_6565, _Texture2));
                float4 node_4501 = tex2D(_Texture2,TRANSFORM_TEX(node_3477, _Texture2));
                float4 node_2195 = tex2D(_Texture2,TRANSFORM_TEX(node_311, _Texture2));
                float4 node_972 = tex2D(_Texture3,TRANSFORM_TEX(node_6565, _Texture3));
                float4 node_4295 = tex2D(_Texture3,TRANSFORM_TEX(node_3477, _Texture3));
                float4 node_1734 = tex2D(_Texture3,TRANSFORM_TEX(node_311, _Texture3));
                float4 node_7081 = tex2D(_Texture4,TRANSFORM_TEX(node_6565, _Texture4));
                float4 node_3235 = tex2D(_Texture4,TRANSFORM_TEX(node_3477, _Texture4));
                float4 node_9532 = tex2D(_Texture4,TRANSFORM_TEX(node_311, _Texture4));
                float3 diffuseColor = (node_1190.r*(node_3954.r*_MainTex.rgb + node_3954.g*node_3536.rgb + node_3954.b*node_3386.rgb) + node_1190.g*(node_3954.r*node_5742.rgb + node_3954.g*node_4501.rgb + node_3954.b*node_2195.rgb) + node_1190.b*(node_3954.r*node_972.rgb + node_3954.g*node_4295.rgb + node_3954.b*node_1734.rgb) + node_1190.a*(node_3954.r*node_7081.rgb + node_3954.g*node_3235.rgb + node_3954.b*node_9532.rgb));
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Texture1; uniform float4 _Texture1_ST;
            uniform sampler2D _Texture2; uniform float4 _Texture2_ST;
            uniform sampler2D _Texture3; uniform float4 _Texture3_ST;
            uniform float4 _TEMP_CHANNEL_UV2;
            uniform sampler2D _Texture4; uniform float4 _Texture4_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv1 : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                o.Emission = 0;
                
                float4 node_1190 = float4(i.uv2.r,i.uv2.g,_TEMP_CHANNEL_UV2.b,_TEMP_CHANNEL_UV2.a);
                float3 node_8849 = abs(i.normalDir);
                float3 node_3954 = (node_8849*node_8849);
                float2 node_6565 = float2(i.posWorld.g,i.posWorld.b);
                float4 _MainTex = tex2D(_Texture1,TRANSFORM_TEX(node_6565, _Texture1));
                float2 node_3477 = float2(i.posWorld.r,i.posWorld.b);
                float4 node_3536 = tex2D(_Texture1,TRANSFORM_TEX(node_3477, _Texture1));
                float2 node_311 = float2(i.posWorld.g,i.posWorld.r);
                float4 node_3386 = tex2D(_Texture1,TRANSFORM_TEX(node_311, _Texture1));
                float4 node_5742 = tex2D(_Texture2,TRANSFORM_TEX(node_6565, _Texture2));
                float4 node_4501 = tex2D(_Texture2,TRANSFORM_TEX(node_3477, _Texture2));
                float4 node_2195 = tex2D(_Texture2,TRANSFORM_TEX(node_311, _Texture2));
                float4 node_972 = tex2D(_Texture3,TRANSFORM_TEX(node_6565, _Texture3));
                float4 node_4295 = tex2D(_Texture3,TRANSFORM_TEX(node_3477, _Texture3));
                float4 node_1734 = tex2D(_Texture3,TRANSFORM_TEX(node_311, _Texture3));
                float4 node_7081 = tex2D(_Texture4,TRANSFORM_TEX(node_6565, _Texture4));
                float4 node_3235 = tex2D(_Texture4,TRANSFORM_TEX(node_3477, _Texture4));
                float4 node_9532 = tex2D(_Texture4,TRANSFORM_TEX(node_311, _Texture4));
                float3 diffColor = (node_1190.r*(node_3954.r*_MainTex.rgb + node_3954.g*node_3536.rgb + node_3954.b*node_3386.rgb) + node_1190.g*(node_3954.r*node_5742.rgb + node_3954.g*node_4501.rgb + node_3954.b*node_2195.rgb) + node_1190.b*(node_3954.r*node_972.rgb + node_3954.g*node_4295.rgb + node_3954.b*node_1734.rgb) + node_1190.a*(node_3954.r*node_7081.rgb + node_3954.g*node_3235.rgb + node_3954.b*node_9532.rgb));
                o.Albedo = diffColor;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Legacy Shaders/Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
