using UnityEngine.Rendering;
using UnityEngine;

namespace HDRPSamples
{
    [System.Serializable]
    public class BasicWind : VolumeComponent
    {
        [Header("General Parameters")]
        public FloatParameter attenuation = new FloatParameter(1.0f);
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
