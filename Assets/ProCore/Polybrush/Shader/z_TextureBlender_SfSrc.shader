// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:Standard,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:1,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:2865,x:33505,y:33100,cmnt:define Z_TEXTURE_CHANNELS 12,varname:node_2865,prsc:2|diff-8957-OUT,spec-358-OUT,gloss-1813-OUT;n:type:ShaderForge.SFN_Multiply,id:6343,x:32186,y:32327,varname:node_6343,prsc:2|A-7736-RGB,B-956-R;n:type:ShaderForge.SFN_Tex2d,id:7736,x:31553,y:32033,ptovrint:True,ptlb:Base Color,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Slider,id:358,x:32866,y:33382,ptovrint:False,ptlb:Metallic,ptin:_Metallic,varname:_Metallic,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.25,max:1;n:type:ShaderForge.SFN_Slider,id:1813,x:32866,y:33508,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:_Gloss,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5,max:1;n:type:ShaderForge.SFN_VertexColor,id:956,x:31021,y:32319,varname:node_956,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:8021,x:31550,y:32251,ptovrint:False,ptlb:Texture 1,ptin:_Texture1,varname:_node_8021,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9220,x:31550,y:32462,ptovrint:False,ptlb:Texture 2,ptin:_Texture2,varname:_node_9220,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:4425,x:31550,y:32672,ptovrint:False,ptlb:Texture 3,ptin:_Texture3,varname:_node_4425,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:8116,x:32186,y:32464,varname:node_8116,prsc:2|A-8021-RGB,B-956-G;n:type:ShaderForge.SFN_Multiply,id:7449,x:32186,y:32607,varname:node_7449,prsc:2|A-9220-RGB,B-956-B;n:type:ShaderForge.SFN_Multiply,id:13,x:32186,y:32745,varname:node_13,prsc:2|A-4425-RGB,B-956-A;n:type:ShaderForge.SFN_Add,id:3502,x:32466,y:32483,varname:node_3502,prsc:2|A-6343-OUT,B-8116-OUT,C-7449-OUT,D-13-OUT;n:type:ShaderForge.SFN_Add,id:1614,x:31293,y:32404,varname:node_1614,prsc:2|A-956-R,B-956-G,C-956-B,D-956-A;n:type:ShaderForge.SFN_Tex2d,id:5725,x:31550,y:32879,ptovrint:False,ptlb:Texture 4,ptin:_Texture4,varname:node_5725,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:3727,x:32186,y:32893,varname:node_3727,prsc:2|A-5725-RGB,B-5693-X;n:type:ShaderForge.SFN_Tex2d,id:376,x:31550,y:33094,ptovrint:False,ptlb:Texture 5,ptin:_Texture5,varname:node_376,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:b66bceaf0cc0ace4e9bdc92f14bba709,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:8321,x:32186,y:33035,varname:node_8321,prsc:2|A-376-RGB,B-5693-Y;n:type:ShaderForge.SFN_Add,id:9064,x:31302,y:32836,varname:node_9064,prsc:2|A-9060-U,B-9060-V,C-5693-Z,D-5693-W;n:type:ShaderForge.SFN_Add,id:1748,x:32478,y:33094,varname:node_1748,prsc:2|A-3727-OUT,B-8321-OUT,C-5568-OUT,D-4512-OUT;n:type:ShaderForge.SFN_Tex2d,id:4771,x:31550,y:33300,ptovrint:False,ptlb:Texture 6,ptin:_Texture6,varname:node_4771,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:3954,x:31550,y:33511,ptovrint:False,ptlb:Texture 7,ptin:_Texture7,varname:node_3954,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:5568,x:32186,y:33185,varname:node_5568,prsc:2|A-4771-RGB,B-5693-Z;n:type:ShaderForge.SFN_Multiply,id:4512,x:32186,y:33368,varname:node_4512,prsc:2|A-3954-RGB,B-5693-W;n:type:ShaderForge.SFN_Vector4Property,id:5693,x:30790,y:32953,ptovrint:False,ptlb:TEMP_CHANNEL_UV2,ptin:_TEMP_CHANNEL_UV2,cmnt:this is mesh.uv3 channel aka uv2 in shader,varname:node_5693,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_TexCoord,id:9060,x:30790,y:32761,cmnt:mesh.uv3 is functionally TEMP_CHANNEL_UV2,varname:node_9060,prsc:2,uv:2;n:type:ShaderForge.SFN_Divide,id:9558,x:32980,y:33131,varname:node_9558,prsc:2|A-9837-OUT,B-1933-OUT;n:type:ShaderForge.SFN_Vector1,id:9837,x:32783,y:33255,varname:node_9837,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:8957,x:33226,y:33100,varname:node_8957,prsc:2|A-8646-OUT,B-9558-OUT;n:type:ShaderForge.SFN_Add,id:1933,x:32783,y:33131,varname:node_1933,prsc:2|A-1614-OUT,B-9064-OUT,C-127-OUT;n:type:ShaderForge.SFN_Add,id:8646,x:32783,y:33010,varname:node_8646,prsc:2|A-3502-OUT,B-1748-OUT,C-5884-OUT;n:type:ShaderForge.SFN_TexCoord,id:3415,x:31039,y:33720,varname:node_3415,prsc:2,uv:3;n:type:ShaderForge.SFN_Tex2d,id:8464,x:31550,y:33726,ptovrint:False,ptlb:Texture 8,ptin:_Texture8,varname:node_8464,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:1983,x:31550,y:33935,ptovrint:False,ptlb:Texture 9,ptin:_Texture9,varname:node_1983,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:5260,x:31550,y:34134,ptovrint:False,ptlb:Texture 10,ptin:_Texture10,varname:node_5260,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9308,x:31550,y:34349,ptovrint:False,ptlb:Texture 11,ptin:_Texture11,varname:node_9308,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Vector4Property,id:2377,x:31039,y:33912,ptovrint:False,ptlb:TEMP_CHANNEL_UV3,ptin:_TEMP_CHANNEL_UV3,varname:node_2377,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_Add,id:127,x:31262,y:33798,varname:node_127,prsc:2|A-3415-U,B-3415-V,C-2377-Z,D-2377-W;n:type:ShaderForge.SFN_Multiply,id:9497,x:32186,y:33539,varname:node_9497,prsc:2|A-8464-RGB,B-2377-X;n:type:ShaderForge.SFN_Multiply,id:2702,x:32186,y:33690,varname:node_2702,prsc:2|A-1983-RGB,B-2377-Y;n:type:ShaderForge.SFN_Multiply,id:8611,x:32186,y:33846,varname:node_8611,prsc:2|A-5260-RGB,B-2377-Z;n:type:ShaderForge.SFN_Multiply,id:6212,x:32186,y:33992,varname:node_6212,prsc:2|A-9308-RGB,B-2377-W;n:type:ShaderForge.SFN_Add,id:5884,x:32481,y:33693,varname:node_5884,prsc:2|A-9497-OUT,B-2702-OUT,C-8611-OUT,D-6212-OUT;n:type:ShaderForge.SFN_TexCoord,id:9802,x:30790,y:32551,cmnt: define Z_MESH_ATTRIBUTES COLOR UV3 UV4,varname:node_9802,prsc:2,uv:0;proporder:358-1813-7736-8021-9220-4425-5725-376-4771-3954-5693-8464-1983-5260-9308-2377;pass:END;sub:END;*/

Shader "Hidden/Polybrush/Standard Texture Blend" {
    Properties {
        _Metallic ("Metallic", Range(0, 1)) = 0.25
        _Gloss ("Gloss", Range(0, 1)) = 0.5
        _MainTex ("Base Color", 2D) = "white" {}
        _Texture1 ("Texture 1", 2D) = "white" {}
        _Texture2 ("Texture 2", 2D) = "white" {}
        _Texture3 ("Texture 3", 2D) = "white" {}
        _Texture4 ("Texture 4", 2D) = "white" {}
        _Texture5 ("Texture 5", 2D) = "white" {}
        _Texture6 ("Texture 6", 2D) = "white" {}
        _Texture7 ("Texture 7", 2D) = "white" {}
        _TEMP_CHANNEL_UV2 ("TEMP_CHANNEL_UV2", Vector) = (0,0,0,0)
        _Texture8 ("Texture 8", 2D) = "white" {}
        _Texture9 ("Texture 9", 2D) = "white" {}
        _Texture10 ("Texture 10", 2D) = "white" {}
        _Texture11 ("Texture 11", 2D) = "white" {}
        _TEMP_CHANNEL_UV3 ("TEMP_CHANNEL_UV3", Vector) = (0,0,0,0)
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
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Metallic;
            uniform float _Gloss;
            uniform sampler2D _Texture1; uniform float4 _Texture1_ST;
            uniform sampler2D _Texture2; uniform float4 _Texture2_ST;
            uniform sampler2D _Texture3; uniform float4 _Texture3_ST;
            uniform sampler2D _Texture4; uniform float4 _Texture4_ST;
            uniform sampler2D _Texture5; uniform float4 _Texture5_ST;
            uniform sampler2D _Texture6; uniform float4 _Texture6_ST;
            uniform sampler2D _Texture7; uniform float4 _Texture7_ST;
            uniform float4 _TEMP_CHANNEL_UV2;
            uniform sampler2D _Texture8; uniform float4 _Texture8_ST;
            uniform sampler2D _Texture9; uniform float4 _Texture9_ST;
            uniform sampler2D _Texture10; uniform float4 _Texture10_ST;
            uniform sampler2D _Texture11; uniform float4 _Texture11_ST;
            uniform float4 _TEMP_CHANNEL_UV3;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                float2 texcoord3 : TEXCOORD3;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
                float4 posWorld : TEXCOORD4;
                float3 normalDir : TEXCOORD5;
                float3 tangentDir : TEXCOORD6;
                float3 bitangentDir : TEXCOORD7;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(8,9)
                UNITY_FOG_COORDS(10)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD11;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.uv3 = v.texcoord3;
                o.vertexColor = v.vertexColor;
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
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = 1.0 - _Gloss; // Convert roughness to gloss
                float specPow = exp2( gloss * 10.0+1.0);
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
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 _Texture1_var = tex2D(_Texture1,TRANSFORM_TEX(i.uv0, _Texture1));
                float4 _Texture2_var = tex2D(_Texture2,TRANSFORM_TEX(i.uv0, _Texture2));
                float4 _Texture3_var = tex2D(_Texture3,TRANSFORM_TEX(i.uv0, _Texture3));
                float4 _Texture4_var = tex2D(_Texture4,TRANSFORM_TEX(i.uv0, _Texture4));
                float4 _Texture5_var = tex2D(_Texture5,TRANSFORM_TEX(i.uv0, _Texture5));
                float4 _Texture6_var = tex2D(_Texture6,TRANSFORM_TEX(i.uv0, _Texture6));
                float4 _Texture7_var = tex2D(_Texture7,TRANSFORM_TEX(i.uv0, _Texture7));
                float4 _Texture8_var = tex2D(_Texture8,TRANSFORM_TEX(i.uv0, _Texture8));
                float4 _Texture9_var = tex2D(_Texture9,TRANSFORM_TEX(i.uv0, _Texture9));
                float4 _Texture10_var = tex2D(_Texture10,TRANSFORM_TEX(i.uv0, _Texture10));
                float4 _Texture11_var = tex2D(_Texture11,TRANSFORM_TEX(i.uv0, _Texture11));
                float3 diffuseColor = ((((_MainTex_var.rgb*i.vertexColor.r)+(_Texture1_var.rgb*i.vertexColor.g)+(_Texture2_var.rgb*i.vertexColor.b)+(_Texture3_var.rgb*i.vertexColor.a))+((_Texture4_var.rgb*_TEMP_CHANNEL_UV2.r)+(_Texture5_var.rgb*_TEMP_CHANNEL_UV2.g)+(_Texture6_var.rgb*_TEMP_CHANNEL_UV2.b)+(_Texture7_var.rgb*_TEMP_CHANNEL_UV2.a))+((_Texture8_var.rgb*_TEMP_CHANNEL_UV3.r)+(_Texture9_var.rgb*_TEMP_CHANNEL_UV3.g)+(_Texture10_var.rgb*_TEMP_CHANNEL_UV3.b)+(_Texture11_var.rgb*_TEMP_CHANNEL_UV3.a)))*(1.0/((i.vertexColor.r+i.vertexColor.g+i.vertexColor.b+i.vertexColor.a)+(i.uv2.r+i.uv2.g+_TEMP_CHANNEL_UV2.b+_TEMP_CHANNEL_UV2.a)+(i.uv3.r+i.uv3.g+_TEMP_CHANNEL_UV3.b+_TEMP_CHANNEL_UV3.a)))); // Need this for specular when using metallic
                float specularMonochrome;
                float3 specularColor;
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, _Metallic, specularColor, specularMonochrome );
                specularMonochrome = 1-specularMonochrome;
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                float NdotH = max(0.0,dot( normalDirection, halfDirection ));
                float VdotH = max(0.0,dot( viewDirection, halfDirection ));
                float visTerm = SmithBeckmannVisibilityTerm( NdotL, NdotV, 1.0-gloss );
                float normTerm = max(0.0, NDFBlinnPhongNormalizedTerm(NdotH, RoughnessToSpecPower(1.0-gloss)));
                float specularPBL = max(0, (NdotL*visTerm*normTerm) * (UNITY_PI / 4) );
                float3 directSpecular = 1 * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float3 directDiffuse = ((1 +(fd90 - 1)*pow((1.00001-NdotL), 5)) * (1 + (fd90 - 1)*pow((1.00001-NdotV), 5)) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
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
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Metallic;
            uniform float _Gloss;
            uniform sampler2D _Texture1; uniform float4 _Texture1_ST;
            uniform sampler2D _Texture2; uniform float4 _Texture2_ST;
            uniform sampler2D _Texture3; uniform float4 _Texture3_ST;
            uniform sampler2D _Texture4; uniform float4 _Texture4_ST;
            uniform sampler2D _Texture5; uniform float4 _Texture5_ST;
            uniform sampler2D _Texture6; uniform float4 _Texture6_ST;
            uniform sampler2D _Texture7; uniform float4 _Texture7_ST;
            uniform float4 _TEMP_CHANNEL_UV2;
            uniform sampler2D _Texture8; uniform float4 _Texture8_ST;
            uniform sampler2D _Texture9; uniform float4 _Texture9_ST;
            uniform sampler2D _Texture10; uniform float4 _Texture10_ST;
            uniform sampler2D _Texture11; uniform float4 _Texture11_ST;
            uniform float4 _TEMP_CHANNEL_UV3;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                float2 texcoord3 : TEXCOORD3;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
                float4 posWorld : TEXCOORD4;
                float3 normalDir : TEXCOORD5;
                float3 tangentDir : TEXCOORD6;
                float3 bitangentDir : TEXCOORD7;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(8,9)
                UNITY_FOG_COORDS(10)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.uv3 = v.texcoord3;
                o.vertexColor = v.vertexColor;
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
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = 1.0 - _Gloss; // Convert roughness to gloss
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 _Texture1_var = tex2D(_Texture1,TRANSFORM_TEX(i.uv0, _Texture1));
                float4 _Texture2_var = tex2D(_Texture2,TRANSFORM_TEX(i.uv0, _Texture2));
                float4 _Texture3_var = tex2D(_Texture3,TRANSFORM_TEX(i.uv0, _Texture3));
                float4 _Texture4_var = tex2D(_Texture4,TRANSFORM_TEX(i.uv0, _Texture4));
                float4 _Texture5_var = tex2D(_Texture5,TRANSFORM_TEX(i.uv0, _Texture5));
                float4 _Texture6_var = tex2D(_Texture6,TRANSFORM_TEX(i.uv0, _Texture6));
                float4 _Texture7_var = tex2D(_Texture7,TRANSFORM_TEX(i.uv0, _Texture7));
                float4 _Texture8_var = tex2D(_Texture8,TRANSFORM_TEX(i.uv0, _Texture8));
                float4 _Texture9_var = tex2D(_Texture9,TRANSFORM_TEX(i.uv0, _Texture9));
                float4 _Texture10_var = tex2D(_Texture10,TRANSFORM_TEX(i.uv0, _Texture10));
                float4 _Texture11_var = tex2D(_Texture11,TRANSFORM_TEX(i.uv0, _Texture11));
                float3 diffuseColor = ((((_MainTex_var.rgb*i.vertexColor.r)+(_Texture1_var.rgb*i.vertexColor.g)+(_Texture2_var.rgb*i.vertexColor.b)+(_Texture3_var.rgb*i.vertexColor.a))+((_Texture4_var.rgb*_TEMP_CHANNEL_UV2.r)+(_Texture5_var.rgb*_TEMP_CHANNEL_UV2.g)+(_Texture6_var.rgb*_TEMP_CHANNEL_UV2.b)+(_Texture7_var.rgb*_TEMP_CHANNEL_UV2.a))+((_Texture8_var.rgb*_TEMP_CHANNEL_UV3.r)+(_Texture9_var.rgb*_TEMP_CHANNEL_UV3.g)+(_Texture10_var.rgb*_TEMP_CHANNEL_UV3.b)+(_Texture11_var.rgb*_TEMP_CHANNEL_UV3.a)))*(1.0/((i.vertexColor.r+i.vertexColor.g+i.vertexColor.b+i.vertexColor.a)+(i.uv2.r+i.uv2.g+_TEMP_CHANNEL_UV2.b+_TEMP_CHANNEL_UV2.a)+(i.uv3.r+i.uv3.g+_TEMP_CHANNEL_UV3.b+_TEMP_CHANNEL_UV3.a)))); // Need this for specular when using metallic
                float specularMonochrome;
                float3 specularColor;
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, _Metallic, specularColor, specularMonochrome );
                specularMonochrome = 1-specularMonochrome;
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                float NdotH = max(0.0,dot( normalDirection, halfDirection ));
                float VdotH = max(0.0,dot( viewDirection, halfDirection ));
                float visTerm = SmithBeckmannVisibilityTerm( NdotL, NdotV, 1.0-gloss );
                float normTerm = max(0.0, NDFBlinnPhongNormalizedTerm(NdotH, RoughnessToSpecPower(1.0-gloss)));
                float specularPBL = max(0, (NdotL*visTerm*normTerm) * (UNITY_PI / 4) );
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float3 directDiffuse = ((1 +(fd90 - 1)*pow((1.00001-NdotL), 5)) * (1 + (fd90 - 1)*pow((1.00001-NdotV), 5)) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
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
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Metallic;
            uniform float _Gloss;
            uniform sampler2D _Texture1; uniform float4 _Texture1_ST;
            uniform sampler2D _Texture2; uniform float4 _Texture2_ST;
            uniform sampler2D _Texture3; uniform float4 _Texture3_ST;
            uniform sampler2D _Texture4; uniform float4 _Texture4_ST;
            uniform sampler2D _Texture5; uniform float4 _Texture5_ST;
            uniform sampler2D _Texture6; uniform float4 _Texture6_ST;
            uniform sampler2D _Texture7; uniform float4 _Texture7_ST;
            uniform float4 _TEMP_CHANNEL_UV2;
            uniform sampler2D _Texture8; uniform float4 _Texture8_ST;
            uniform sampler2D _Texture9; uniform float4 _Texture9_ST;
            uniform sampler2D _Texture10; uniform float4 _Texture10_ST;
            uniform sampler2D _Texture11; uniform float4 _Texture11_ST;
            uniform float4 _TEMP_CHANNEL_UV3;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                float2 texcoord3 : TEXCOORD3;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
                float4 posWorld : TEXCOORD4;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.uv3 = v.texcoord3;
                o.vertexColor = v.vertexColor;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                o.Emission = 0;
                
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 _Texture1_var = tex2D(_Texture1,TRANSFORM_TEX(i.uv0, _Texture1));
                float4 _Texture2_var = tex2D(_Texture2,TRANSFORM_TEX(i.uv0, _Texture2));
                float4 _Texture3_var = tex2D(_Texture3,TRANSFORM_TEX(i.uv0, _Texture3));
                float4 _Texture4_var = tex2D(_Texture4,TRANSFORM_TEX(i.uv0, _Texture4));
                float4 _Texture5_var = tex2D(_Texture5,TRANSFORM_TEX(i.uv0, _Texture5));
                float4 _Texture6_var = tex2D(_Texture6,TRANSFORM_TEX(i.uv0, _Texture6));
                float4 _Texture7_var = tex2D(_Texture7,TRANSFORM_TEX(i.uv0, _Texture7));
                float4 _Texture8_var = tex2D(_Texture8,TRANSFORM_TEX(i.uv0, _Texture8));
                float4 _Texture9_var = tex2D(_Texture9,TRANSFORM_TEX(i.uv0, _Texture9));
                float4 _Texture10_var = tex2D(_Texture10,TRANSFORM_TEX(i.uv0, _Texture10));
                float4 _Texture11_var = tex2D(_Texture11,TRANSFORM_TEX(i.uv0, _Texture11));
                float3 diffColor = ((((_MainTex_var.rgb*i.vertexColor.r)+(_Texture1_var.rgb*i.vertexColor.g)+(_Texture2_var.rgb*i.vertexColor.b)+(_Texture3_var.rgb*i.vertexColor.a))+((_Texture4_var.rgb*_TEMP_CHANNEL_UV2.r)+(_Texture5_var.rgb*_TEMP_CHANNEL_UV2.g)+(_Texture6_var.rgb*_TEMP_CHANNEL_UV2.b)+(_Texture7_var.rgb*_TEMP_CHANNEL_UV2.a))+((_Texture8_var.rgb*_TEMP_CHANNEL_UV3.r)+(_Texture9_var.rgb*_TEMP_CHANNEL_UV3.g)+(_Texture10_var.rgb*_TEMP_CHANNEL_UV3.b)+(_Texture11_var.rgb*_TEMP_CHANNEL_UV3.a)))*(1.0/((i.vertexColor.r+i.vertexColor.g+i.vertexColor.b+i.vertexColor.a)+(i.uv2.r+i.uv2.g+_TEMP_CHANNEL_UV2.b+_TEMP_CHANNEL_UV2.a)+(i.uv3.r+i.uv3.g+_TEMP_CHANNEL_UV3.b+_TEMP_CHANNEL_UV3.a))));
                float specularMonochrome;
                float3 specColor;
                diffColor = DiffuseAndSpecularFromMetallic( diffColor, _Metallic, specColor, specularMonochrome );
                float roughness = _Gloss;
                o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Standard"
    CustomEditor "ShaderForgeMaterialInspector"
}
