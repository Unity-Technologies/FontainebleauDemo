using UnityEngine;
using UnityEngine.Rendering;

namespace HDRPSamples
{
    [ExecuteAlways]
    public class BasicWindData : MonoBehaviour
    {
        //Windzone and its properties
        private WindZone windZone;
        private Vector3 windDirection = Vector3.forward;
        private float windSpeed = 0;
        private float windTurbulence = 0;

        //Debug
        public bool debug = false;

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
            if (windZone != null)
            {
                GetDirectionAndSpeed();
            }

            Shader.SetGlobalTexture(BasicWindShaderIDs.TexNoise, windSettings.noiseTexture.value);
            Shader.SetGlobalTexture(BasicWindShaderIDs.TexGust, windSettings.gustMaskTexture.value);
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

            if(debug)
            {
                Debug.Log("Entity Direction " + windDirection);
                Debug.Log("Shader value " + Shader.GetGlobalVector(BasicWindShaderIDs.WorldDirectionAndSpeed));
            }
        }
    }

    static class BasicWindShaderIDs
    {
        internal static readonly int PlayerPos = Shader.PropertyToID("_BASICWIND_PlayerPositionAndRadius");
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
    }
}