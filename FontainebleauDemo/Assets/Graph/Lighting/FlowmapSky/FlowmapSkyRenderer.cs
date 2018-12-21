namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    public class FlowmapSkyRenderer : SkyRenderer
    {
        Material m_FlowmapSkyMaterial; // Renders a cubemap into a render texture (can be cube or 2D)
        MaterialPropertyBlock m_PropertyBlock;
        FlowmapSky m_FlowmapSkyParams;

        public FlowmapSkyRenderer(FlowmapSky flowmapSkyParams)
        {
            m_FlowmapSkyParams = flowmapSkyParams;
            m_PropertyBlock = new MaterialPropertyBlock();
        }

        public override void Build()
        {
            m_FlowmapSkyMaterial = CoreUtils.CreateEngineMaterial("Hidden/HDRenderPipeline/Sky/FlowmapSky");
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(m_FlowmapSkyMaterial);
        }

        public override void SetRenderTargets(BuiltinSkyParameters builtinParams)
        {
            if (builtinParams.depthBuffer == BuiltinSkyParameters.nullRT)
            {
                HDUtils.SetRenderTarget(builtinParams.commandBuffer, builtinParams.hdCamera, builtinParams.colorBuffer);
            }
            else
            {
                HDUtils.SetRenderTarget(builtinParams.commandBuffer, builtinParams.hdCamera, builtinParams.colorBuffer, builtinParams.depthBuffer);
            }
        }

        public override void RenderSky(BuiltinSkyParameters builtinParams, bool renderForCubemap, bool renderSunDisk /* TODO Handle sun disk?*/)
        {
            m_FlowmapSkyMaterial.SetTexture(HDShaderIDs._Cubemap, m_FlowmapSkyParams.skyHDRI);
            m_FlowmapSkyMaterial.SetFloat("_Period", m_FlowmapSkyParams.period);
            m_FlowmapSkyMaterial.SetVector(HDShaderIDs._SkyParam, new Vector4(m_FlowmapSkyParams.exposure, m_FlowmapSkyParams.multiplier, m_FlowmapSkyParams.rotation, 0.0f));

            // This matrix needs to be updated at the draw call frequency.
            m_PropertyBlock.SetMatrix(HDShaderIDs._PixelCoordToViewDirWS, builtinParams.pixelCoordToViewDirMatrix);

            CoreUtils.DrawFullScreen(builtinParams.commandBuffer, m_FlowmapSkyMaterial, m_PropertyBlock, renderForCubemap ? 0 : 1);
        }

        public override bool IsValid()
        {
            return m_FlowmapSkyParams != null && m_FlowmapSkyMaterial != null;
        }
    }
}
