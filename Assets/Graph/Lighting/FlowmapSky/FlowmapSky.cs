namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [SkyUniqueID(150)]
    public class FlowmapSky : SkySettings
    {
        [Tooltip("Cubemap used to render the sky.")]
        public CubemapParameter skyHDRI = new CubemapParameter(null);

        public FloatParameter period = new FloatParameter(15);

        public override SkyRenderer CreateRenderer()
        {
            return new FlowmapSkyRenderer(this);
        }

        public override int GetHashCode()
        {
            int hash = base.GetHashCode();

            unchecked
            {
                hash = skyHDRI.value != null ? hash * 23 + skyHDRI.GetHashCode() : hash;
            }

            return hash;
        }
    }
}
