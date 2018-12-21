using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    public class TextShaderGUI : ShaderGUI
    {
        protected static class Styles
        {
            public static string optionText = "Options";
            public static string surfaceTypeText = "Surface Type";
            public static string blendModeText = "Blend Mode";

            public static GUIContent alphaCutoffEnableText = new GUIContent("Alpha Cutoff Enable", "Threshold for alpha cutoff");
            public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");
            public static GUIContent doubleSidedModeText = new GUIContent("Double Sided", "This will render the two face of the objects (disable backface culling)");
            public static GUIContent cullModeText = new GUIContent("Culling Mode");

            public static readonly string[] surfaceTypeNames = Enum.GetNames(typeof(SurfaceType));
            public static readonly string[] blendModeNames = Enum.GetNames(typeof(BlendMode));

            public static GUIContent fontTextureText = new GUIContent("Font Texture");

            public static string InputsOptionsText = "Inputs options";
            public static string InputsMapText = "";
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

            BlendModePopup();

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
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent +2; // Hack to fix sorting

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

            SetKeyword(material, "_ALPHATEST_ON", alphaTestEnable);

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

        MaterialProperty fontTexture = null;
        protected const string kFontTexture = "_MainTex";

        MaterialProperty surfaceType = null;
        protected const string kSurfaceType = "_SurfaceType";
        MaterialProperty blendMode = null;
        protected const string kBlendMode = "_BlendMode";

        MaterialProperty alphaCutoff = null;
        protected const string kAlphaCutoff = "_AlphaCutoff";
        MaterialProperty alphaCutoffEnable = null;
        protected const string kAlphaCutoffEnabled = "_AlphaCutoffEnable";


        protected void FindMaterialProperties(MaterialProperty[] props)
        {
            fontTexture = FindProperty(kFontTexture, props);
        }

        protected void ShaderInputGUI()
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.fontTextureText, fontTexture);
        }

        protected void SetupMaterialKeywords(Material material)
        {
            SetupCommonOptionsKeywords(material);
        }

        protected bool ShouldEmissionBeEnabled(Material mat)
        {
            return false;
        }

        protected static string[] reservedProperties = new string[] { kSurfaceType, kBlendMode, kAlphaCutoff, kAlphaCutoffEnabled };
    }

}
