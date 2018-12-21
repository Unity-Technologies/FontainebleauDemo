using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    public class ParticlesShaderGUI : ShaderGUI
    {
        protected static class Styles
        {
            public static string optionText = "Options";
            public static string surfaceTypeText = "Surface Type";
            public static string blendModeText = "Blend Mode";

            public static GUIContent alphaCutoffEnableText = new GUIContent("Alpha Cutoff Enable", "Threshold for alpha cutoff");
            public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");
            public static GUIContent doubleSidedModeText = new GUIContent("Double Sided", "This will render the two face of the objects (disable backface culling)");
            public static GUIContent distortionEnableText = new GUIContent("Distortion", "Enable distortion on this shader");
            public static GUIContent distortionOnlyText = new GUIContent("Distortion Only", "This shader will only be use to render distortion");
            public static GUIContent distortionDepthTestText = new GUIContent("Distortion Depth Test", "Enable the depth test for distortion");

            public static readonly string[] surfaceTypeNames = Enum.GetNames(typeof(SurfaceType));
            public static readonly string[] blendModeNames = Enum.GetNames(typeof(BlendMode));

            public static string InputsOptionsText = "Inputs options";

            public static string InputsHeader = "Color Texture";
            public static string FadingHeader = "Fading Options";
            public static string DistortionHeader = "Distortion";

            public static string InputsMapText = "";

            public static GUIContent softParticlesLabel = new GUIContent("Soft Particles Distance", "In world units, depth distance to first opaque pixel behind from which the particle will fade. Zero disables the feature");
            public static GUIContent cameraFadingLabel = new GUIContent("CameraFade Distance (Min/Max)", "In world units, min and max depth distance which the current pixel will fade.");

            public static GUIContent colorText = new GUIContent("Color + Opacity", "Albedo (RGB) and Opacity (A)");
            public static GUIContent distortionMapText = new GUIContent("Distortion Map (RG) and Blur (B)");
            public static GUIContent emissiveText = new GUIContent("Emissive Color", "Emissive");
            public static GUIContent emissiveIntensityText = new GUIContent("Emissive Intensity", "Emissive");

            public static GUIContent emissiveWarning = new GUIContent("Emissive value is animated but the material has not been configured to support emissive. Please make sure the material itself has some amount of emissive.");
            public static GUIContent emissiveColorWarning = new GUIContent("Ensure emissive color is non-black for emission to have effect.");

        }

        public enum SurfaceType
        {
            Opaque,
            Transparent
        }

        public enum BlendMode
        {
            Lerp,
            Add,
            SoftAdd,
            Multiply,
            Premultiply
        }

       void SurfaceTypePopup()
        {
            EditorGUI.showMixedValue = surfaceType.hasMixedValue;
            var mode = (SurfaceType)surfaceType.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (SurfaceType)EditorGUILayout.Popup(Styles.surfaceTypeText, (int)mode, Styles.surfaceTypeNames);
            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Surface Type");
                surfaceType.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;
        }

        void BlendModePopup()
        {
            EditorGUI.showMixedValue = blendMode.hasMixedValue;
            var mode = (BlendMode)blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (BlendMode)EditorGUILayout.Popup(Styles.blendModeText, (int)mode, Styles.blendModeNames);
            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Blend Mode");
                blendMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;
        }

        protected void ShaderOptionsGUI()
        {
            EditorGUI.indentLevel++;
            GUILayout.Label(Styles.optionText, EditorStyles.boldLabel);
            SurfaceTypePopup();
            if ((SurfaceType)surfaceType.floatValue == SurfaceType.Transparent)
            {
                BlendModePopup();
                m_MaterialEditor.ShaderProperty(distortionEnable, Styles.distortionEnableText.text);

                if (distortionEnable.floatValue == 1.0)
                {
                    m_MaterialEditor.ShaderProperty(distortionOnly, Styles.distortionOnlyText.text);
                    m_MaterialEditor.ShaderProperty(distortionDepthTest, Styles.distortionDepthTestText.text);
                }
            }
            m_MaterialEditor.ShaderProperty(alphaCutoffEnable, Styles.alphaCutoffEnableText.text);
            if (alphaCutoffEnable.floatValue == 1.0)
            {
                m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text);
            }

            EditorGUI.indentLevel--;
        }

        public void FindCommonOptionProperties(MaterialProperty[] props)
        {
            surfaceType = FindProperty(kSurfaceType, props);
            blendMode = FindProperty(kBlendMode, props);
            alphaCutoff = FindProperty(kAlphaCutoff, props);
            alphaCutoffEnable = FindProperty(kAlphaCutoffEnabled, props);
            distortionEnable = FindProperty(kDistortionEnable, props);
            distortionOnly = FindProperty(kDistortionOnly, props);
            distortionDepthTest = FindProperty(kDistortionDepthTest, props);
        }

		protected void SetupCommonOptionsKeywords(Material material)
		{
			// Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)

            bool alphaTestEnable = material.GetFloat(kAlphaCutoffEnabled) == 1.0;
            SurfaceType surfaceType = (SurfaceType)material.GetFloat(kSurfaceType);
            BlendMode blendMode = (BlendMode)material.GetFloat(kBlendMode);

            if (surfaceType == SurfaceType.Opaque)
            {
                material.SetOverrideTag("RenderType", alphaTestEnable ? "TransparentCutout" : "");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.renderQueue = alphaTestEnable ? (int)UnityEngine.Rendering.RenderQueue.AlphaTest : -1;
            }
            else
            {
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_ZWrite", 0);
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                switch (blendMode)
                {
                    case BlendMode.Lerp:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        break;

                    case BlendMode.Add:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        break;

                    case BlendMode.SoftAdd:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        break;

                    case BlendMode.Multiply:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        break;

                    case BlendMode.Premultiply:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        break;
                }
            }

            // No culling (always doublesided)
            material.SetInt("_CullMode", (int)UnityEngine.Rendering.CullMode.Off);

            SetKeyword(material, "_ALPHATEST_ON", alphaTestEnable);

            bool distortionEnable = material.GetFloat(kDistortionEnable) == 1.0;
            bool distortionOnly = material.GetFloat(kDistortionOnly) == 1.0;
            bool distortionDepthTest = material.GetFloat(kDistortionDepthTest) == 1.0;

            if (distortionEnable)
            {
                material.SetShaderPassEnabled("DistortionVectors", true);
            }
            else
            {
                material.SetShaderPassEnabled("DistortionVectors", false);
            }

            if (distortionEnable && distortionOnly)
            {
                // Disable all passes except dbug material
                material.SetShaderPassEnabled("DebugViewMaterial", true);
                material.SetShaderPassEnabled("Meta", false);
                material.SetShaderPassEnabled("Forward", false);
                material.SetShaderPassEnabled("ForwardOnlyOpaque", false);
            }
            else
            {
                material.SetShaderPassEnabled("DebugViewMaterial", true);
                material.SetShaderPassEnabled("Meta", true);
                material.SetShaderPassEnabled("Forward", true);
                material.SetShaderPassEnabled("ForwardOnlyOpaque", true);
            }

            if (distortionDepthTest)
            {
                material.SetInt("_ZTestMode", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
            }
            else
            {
                material.SetInt("_ZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
            }

            SetKeyword(material, "_DISTORTION_ON", distortionEnable);

            SetupEmissionGIFlags(material);
		}

        protected void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }

        public void ShaderPropertiesGUI(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                ShaderOptionsGUI();
                EditorGUILayout.Space();
                ShaderInputGUI();
            }

            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in m_MaterialEditor.targets)
                    SetupMaterialKeywords((Material)obj);
            }
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindCommonOptionProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
            FindMaterialProperties(props);

            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;
            ShaderPropertiesGUI(material);
        }

        protected virtual void SetupEmissionGIFlags(Material material)
        {
            // Setup lightmap emissive flags
            MaterialGlobalIlluminationFlags flags = material.globalIlluminationFlags;
            if ((flags & (MaterialGlobalIlluminationFlags.BakedEmissive | MaterialGlobalIlluminationFlags.RealtimeEmissive)) != 0)
            {
                if (ShouldEmissionBeEnabled(material))
                    flags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                else
                    flags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;

                material.globalIlluminationFlags = flags;
            }
        }

        protected MaterialEditor m_MaterialEditor;

        MaterialProperty surfaceType = null;
        protected const string kSurfaceType = "_SurfaceType";
        MaterialProperty blendMode = null;
        protected const string kBlendMode = "_BlendMode";

        MaterialProperty alphaCutoff = null;
        protected const string kAlphaCutoff = "_AlphaCutoff";
        MaterialProperty alphaCutoffEnable = null;
        protected const string kAlphaCutoffEnabled = "_AlphaCutoffEnable";

        MaterialProperty distortionEnable = null;
        const string kDistortionEnable = "_DistortionEnable";
        MaterialProperty distortionOnly = null;
        const string kDistortionOnly = "_DistortionOnly";
        MaterialProperty distortionDepthTest = null;
        const string kDistortionDepthTest = "_DistortionDepthTest";
        MaterialProperty distortionMap = null;
        protected const string kDistortionMap = "_DistortionVectorMap";
        MaterialProperty distortionIntensity = null;
        protected const string kDistortionIntensity = "_DistortionIntensity";


        MaterialProperty colorMap = null;
        protected const string kColorMap = "_ColorMap";
        MaterialProperty brightness = null;
        protected const string kBrightness = "_Brightness";


        MaterialProperty softParticleInvDistance = null;
        protected const string kSoftParticleInvDistance = "_SoftParticleInvDistance";
        MaterialProperty cameraFadeNearDistance = null;
        protected const string kCameraFadeNearDistance = "_CameraFadeNearDistance";
        MaterialProperty cameraFadeFarDistance = null;
        protected const string kCameraFadeFarDistance = "_CameraFadeFarDistance";

        protected void FindMaterialProperties(MaterialProperty[] props)
        {
            colorMap = FindProperty(kColorMap, props);
            brightness = FindProperty(kBrightness, props);

            distortionMap = FindProperty(kDistortionMap, props);
            distortionIntensity = FindProperty(kDistortionIntensity, props);

            softParticleInvDistance = FindProperty(kSoftParticleInvDistance, props);
            cameraFadeNearDistance = FindProperty(kCameraFadeNearDistance, props);
            cameraFadeFarDistance = FindProperty(kCameraFadeFarDistance, props);
        }

        protected void ShaderInputGUI()
        {
            EditorGUI.indentLevel++;
            {
                GUILayout.Label(Styles.InputsHeader, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                m_MaterialEditor.TexturePropertySingleLine(Styles.colorText, colorMap);
                m_MaterialEditor.FloatProperty(brightness, "Brigthness");
                EditorGUI.indentLevel--;

                GUILayout.Label(Styles.FadingHeader, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                // Soft Particles
                EditorGUI.BeginChangeCheck();

                float invDist = softParticleInvDistance.floatValue;
                float dist = invDist < 0? 0.0f : 1.0f / invDist;
                dist = Mathf.Max(0.0f, EditorGUILayout.FloatField(Styles.softParticlesLabel, dist));

                if(EditorGUI.EndChangeCheck())
                {
                    if (dist == 0) // Disable Feature
                    {
                        softParticleInvDistance.floatValue = -1.0f;
                    }
                    else
                        softParticleInvDistance.floatValue = 1.0f / dist;
                }

                // Canera Fading
                Vector2 distances = new Vector2(cameraFadeNearDistance.floatValue, cameraFadeFarDistance.floatValue);
                EditorGUI.BeginChangeCheck();

                distances = EditorGUILayout.Vector2Field(Styles.cameraFadingLabel, distances);

                if(EditorGUI.EndChangeCheck())
                {
                    if(distances.x < 0.0f)
                    {
                        distances.x = 0.0f;
                    }

                    if(distances.y < 0.0f)
                    {
                        distances.y = 0.0f;
                    }
                    cameraFadeNearDistance.floatValue = distances.x;
                    cameraFadeFarDistance.floatValue = distances.y;
                }


                EditorGUI.indentLevel--;

                GUILayout.Label(Styles.DistortionHeader, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                m_MaterialEditor.TexturePropertySingleLine(Styles.distortionMapText, distortionMap);
                m_MaterialEditor.FloatProperty(distortionIntensity, "Distortion Intensity");
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        protected void SetupMaterialKeywords(Material material)
        {
            SetupCommonOptionsKeywords(material);

            // Soft Particles
            bool bSoftParticlesEnabled = material.GetFloat(kSoftParticleInvDistance) >= 0.0f;
            SetKeyword(material, "_SOFTPARTICLES_ON", bSoftParticlesEnabled);
            bool bCameraFadeEnabled = !(material.GetFloat(kCameraFadeNearDistance) == 0.0f && material.GetFloat(kCameraFadeFarDistance) == 0.0f);
            SetKeyword(material, "_CAMERAFADING_ON", bCameraFadeEnabled);
        }

        protected bool ShouldEmissionBeEnabled(Material mat)
        {
            return false;
        }

        protected static string[] reservedProperties = new string[] { kSurfaceType, kBlendMode, kAlphaCutoff, kAlphaCutoffEnabled };
    }

}
