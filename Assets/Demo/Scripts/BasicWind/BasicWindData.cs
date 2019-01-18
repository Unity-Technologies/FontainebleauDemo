using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Fontainebleau
{
    [ExecuteAlways]
    public class BasicWindData : MonoBehaviour
    {
        //Windzone and its properties
        private WindZone windZone;
        private Vector3 windDirection = Vector3.forward;
        private float windSpeed = 0;
        private float windTurbulence = 0;

        //Wind Direction and occlusion texture
        public Texture2D windDirectionAndOcclusionTex;

        //Debug
        public bool debug = true;

        private void OnEnable()
        {
            CreateRenderTexture();
        }

        void Start()
        {
            windZone = gameObject.GetComponent<WindZone>();
            ApplySettings();
        }

        void Update()
        {
            ApplySettings();
        }

        void OnValidate()
        {
            ApplySettings();
        }

        void ApplySettings()
        {
            BasicWind windSettings = VolumeManager.instance.stack.GetComponent<BasicWind>();

            if (windSettings == null)
                return;

            if (windZone == null)
                windZone = gameObject.GetComponent<WindZone>();
            if (windZone != null && windDirectionAndOcclusionTex != null)
            {
                GetDirectionAndSpeed();
            }

            Shader.SetGlobalTexture(BasicWindShaderIDs.TexNoise, windSettings.noiseTexture.value);
            Shader.SetGlobalTexture(BasicWindShaderIDs.TexGust, windSettings.gustMaskTexture.value);
            Shader.SetGlobalTexture(BasicWindShaderIDs.TexDirectionOcclusion, windDirectionAndOcclusionTex);
            Shader.SetGlobalVector(BasicWindShaderIDs.WorldDirectionAndSpeed, new Vector4(windDirection.x, windDirection.y, windDirection.z, windSpeed * 0.2777f));
            Shader.SetGlobalFloat(BasicWindShaderIDs.FlexNoiseScale, 1.0f / Mathf.Max(0.01f, windSettings.flexNoiseWorldSize.value));
            Shader.SetGlobalFloat(BasicWindShaderIDs.ShiverNoiseScale, 1.0f / Mathf.Max(0.01f, windSettings.shiverNoiseWorldSize.value));
            Shader.SetGlobalFloat(BasicWindShaderIDs.Turbulence, windSpeed * windTurbulence);
            Shader.SetGlobalFloat(BasicWindShaderIDs.GustSpeed, windSettings.gustSpeed.value);
            Shader.SetGlobalFloat(BasicWindShaderIDs.GustScale, windSettings.gustScale.value);
            Shader.SetGlobalFloat(BasicWindShaderIDs.GustWorldScale, 1.0f / Mathf.Max(0.01f, windSettings.gustWorldSize.value));
            Shader.SetGlobalFloat(BasicWindShaderIDs.Attenuation, windSettings.attenuation.value);
        }

        void GetDirectionAndSpeed()
        {
            windDirection = windZone.transform.forward;
            windSpeed = windZone.windMain;
            windTurbulence = windZone.windTurbulence;
            Debug.Log("Direction" + windDirection);

            Color[] directionAndOcclusion = new Color[1];
            directionAndOcclusion[0] = new Color(windDirection.x, windDirection.y, windDirection.z, 1);
            windDirectionAndOcclusionTex.SetPixels(directionAndOcclusion);
            Debug.Log("Color" + directionAndOcclusion[0]);
            var pixel = windDirectionAndOcclusionTex.GetPixel(0, 0);
            Debug.Log("ReadColor" + pixel);
            Debug.DrawLine(Vector3.zero, new Vector3(pixel.r, pixel.g, pixel.b),Color.red);
            Debug.Log("Shader value" + Shader.GetGlobalVector(BasicWindShaderIDs.WorldDirectionAndSpeed));
            
        }

        void CreateRenderTexture()
        {
            windDirectionAndOcclusionTex = new Texture2D(1, 1, TextureFormat.RGBAFloat, false, true)
            {
                filterMode = FilterMode.Point,
                name = "GlobalWindDirectionAndOcclusionTexture"
            };
        }

        private void OnDisable()
        {
            OnDestroy();
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                if (windDirectionAndOcclusionTex != null)
                    Destroy(windDirectionAndOcclusionTex);
            }
            else
            {
                if (windDirectionAndOcclusionTex != null)
                    DestroyImmediate(windDirectionAndOcclusionTex);
            }
        }
    }

    static class BasicWindShaderIDs
    {
        internal static readonly int TexNoise = Shader.PropertyToID("_BASICWIND_TexNoise");
        internal static readonly int TexGust = Shader.PropertyToID("_BASICWIND_TexGust");
        internal static readonly int WorldDirectionAndSpeed = Shader.PropertyToID("_BASICWIND_WorldDirectionAndSpeed");
        internal static readonly int FlexNoiseScale = Shader.PropertyToID("_BASICWIND_FlexNoiseScale");
        internal static readonly int ShiverNoiseScale = Shader.PropertyToID("_BASICWIND_ShiverNoiseScale");
        internal static readonly int Turbulence = Shader.PropertyToID("_BASICWIND_Turbulence");
        internal static readonly int GustSpeed = Shader.PropertyToID("_BASICWIND_GustSpeed");
        internal static readonly int GustScale = Shader.PropertyToID("_BASICWIND_GustScale");
        internal static readonly int GustWorldScale = Shader.PropertyToID("_BASICWIND_GustWorldScale");
        internal static readonly int Attenuation = Shader.PropertyToID("_BASICWIND_Attenuation");
        internal static readonly int TexDirectionOcclusion = Shader.PropertyToID("_BASICWIND_TexDirectionOcclusion");
    }
}