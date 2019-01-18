using UnityEngine.Experimental.Rendering;
using UnityEngine;

namespace Fontainebleau
{
    [System.Serializable]
    public class BasicWind : VolumeComponent
    {
        [Header("General Parameters")]
        public FloatParameter attenuation = new FloatParameter(1.0f);
        //[Tooltip("Wind Direction vector")]
        //public Vector3Parameter windDirection = new Vector3Parameter(new Vector3(0f,0.5f,0.5f));
        //[Tooltip("Wind Speed in Kilometers per hour")]
        //public FloatParameter windSpeed = new FloatParameter(30.0f);
        //[Tooltip("Wind Turbulence in percentage of wind Speed")]
        //public FloatParameter turbulence = new FloatParameter(0.25f);
        [Header("Noise Parameters")]
        [Tooltip("Texture used for wind turbulence")]
        public TextureParameter noiseTexture = new TextureParameter(null);
        [Tooltip("Size of one world tiling patch of the Noise Texture, for bending trees")]
        public FloatParameter flexNoiseWorldSize = new FloatParameter(175.0f);
        [Tooltip("Size of one world tiling patch of the Noise Texture, for leaf shivering")]
        public FloatParameter shiverNoiseWorldSize = new FloatParameter(10.0f);
        [Header("Gust Parameters")]
        [Tooltip("Texture used for wind gusts")]
        public TextureParameter gustMaskTexture = new TextureParameter(null);
        [Tooltip("Size of one world tiling patch of the Gust Texture, for leaf shivering")]
        public FloatParameter gustWorldSize = new FloatParameter(600.0f);
        [Tooltip("Wind Gust Speed in Kilometers per hour")]
        public FloatParameter gustSpeed = new FloatParameter(50.0f);
        [Tooltip("Wind Gust Influence on trees")]
        public FloatParameter gustScale = new FloatParameter(1.0f);
    }
}
