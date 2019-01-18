// define Z_TEXTURE_CHANNELS 12
// define Z_MESH_ATTRIBUTES 
// Important!  This is a generated file, any changes will be overwritten
// when the _SfSrc suffixed version of this shader is modified.


Shader "Polybrush/Standard Texture Blend" {
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
        
        _Texture8 ("Texture 8", 2D) = "white" {}
        _Texture9 ("Texture 9", 2D) = "white" {}
        _Texture10 ("Texture 10", 2D) = "white" {}
        _Texture11 ("Texture 11", 2D) = "white" {}
        
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
            
            uniform sampler2D _Texture8; uniform float4 _Texture8_ST;
            uniform sampler2D _Texture9; uniform float4 _Texture9_ST;
            uniform sampler2D _Texture10; uniform float4 _Texture10_ST;
            uniform sampler2D _Texture11; uniform float4 _Texture11_ST;
            
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 texcoord0 : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 texcoord2 : TEXCOORD2;
                float4 texcoord3 : TEXCOORD3;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 uv3 : TEXCOORD3;
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
                float3 diffuseColor = ((((_MainTex_var.rgb*i.vertexColor.r)+(_Texture1_var.rgb*i.vertexColor.g)+(_Texture2_var.rgb*i.vertexColor.b)+(_Texture3_var.rgb*i.vertexColor.a))+((_Texture4_var.rgb*i.uv2.r)+(_Texture5_var.rgb*i.uv2.g)+(_Texture6_var.rgb*i.uv2.b)+(_Texture7_var.rgb*i.uv2.a))+((_Texture8_var.rgb*i.uv3.r)+(_Texture9_var.rgb*i.uv3.g)+(_Texture10_var.rgb*i.uv3.b)+(_Texture11_var.rgb*i.uv3.a)))*(1.0/((i.vertexColor.r+i.vertexColor.g+i.vertexColor.b+i.vertexColor.a)+(i.uv2.r+i.uv2.g+i.uv2.b+i.uv2.a)+(i.uv3.r+i.uv3.g+i.uv3.b+i.uv3.a)))); // Need this for specular when using metallic
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
            
            uniform sampler2D _Texture8; uniform float4 _Texture8_ST;
            uniform sampler2D _Texture9; uniform float4 _Texture9_ST;
            uniform sampler2D _Texture10; uniform float4 _Texture10_ST;
            uniform sampler2D _Texture11; uniform float4 _Texture11_ST;
            
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 texcoord0 : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 texcoord2 : TEXCOORD2;
                float4 texcoord3 : TEXCOORD3;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 uv3 : TEXCOORD3;
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
                float3 diffuseColor = ((((_MainTex_var.rgb*i.vertexColor.r)+(_Texture1_var.rgb*i.vertexColor.g)+(_Texture2_var.rgb*i.vertexColor.b)+(_Texture3_var.rgb*i.vertexColor.a))+((_Texture4_var.rgb*i.uv2.r)+(_Texture5_var.rgb*i.uv2.g)+(_Texture6_var.rgb*i.uv2.b)+(_Texture7_var.rgb*i.uv2.a))+((_Texture8_var.rgb*i.uv3.r)+(_Texture9_var.rgb*i.uv3.g)+(_Texture10_var.rgb*i.uv3.b)+(_Texture11_var.rgb*i.uv3.a)))*(1.0/((i.vertexColor.r+i.vertexColor.g+i.vertexColor.b+i.vertexColor.a)+(i.uv2.r+i.uv2.g+i.uv2.b+i.uv2.a)+(i.uv3.r+i.uv3.g+i.uv3.b+i.uv3.a)))); // Need this for specular when using metallic
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
            
            uniform sampler2D _Texture8; uniform float4 _Texture8_ST;
            uniform sampler2D _Texture9; uniform float4 _Texture9_ST;
            uniform sampler2D _Texture10; uniform float4 _Texture10_ST;
            uniform sampler2D _Texture11; uniform float4 _Texture11_ST;
            
            struct VertexInput {
                float4 vertex : POSITION;
                float4 texcoord0 : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 texcoord2 : TEXCOORD2;
                float4 texcoord3 : TEXCOORD3;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 uv3 : TEXCOORD3;
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
                float3 diffColor = ((((_MainTex_var.rgb*i.vertexColor.r)+(_Texture1_var.rgb*i.vertexColor.g)+(_Texture2_var.rgb*i.vertexColor.b)+(_Texture3_var.rgb*i.vertexColor.a))+((_Texture4_var.rgb*i.uv2.r)+(_Texture5_var.rgb*i.uv2.g)+(_Texture6_var.rgb*i.uv2.b)+(_Texture7_var.rgb*i.uv2.a))+((_Texture8_var.rgb*i.uv3.r)+(_Texture9_var.rgb*i.uv3.g)+(_Texture10_var.rgb*i.uv3.b)+(_Texture11_var.rgb*i.uv3.a)))*(1.0/((i.vertexColor.r+i.vertexColor.g+i.vertexColor.b+i.vertexColor.a)+(i.uv2.r+i.uv2.g+i.uv2.b+i.uv2.a)+(i.uv3.r+i.uv3.g+i.uv3.b+i.uv3.a))));
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
    CustomEditor "z_BlendMaterialInspector"
}
