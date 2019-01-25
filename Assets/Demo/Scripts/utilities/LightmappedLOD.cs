using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightmappedLOD : MonoBehaviour {

    private MeshRenderer currentRenderer;

    private void Awake()
    {
        currentRenderer = gameObject.GetComponent<MeshRenderer>();
        RendererInfoTransfer();
    }

#if UNITY_EDITOR
    void OnBecameVisible()
    {
        if(!Application.isPlaying)
            RendererInfoTransfer();
    }
#endif

    void RendererInfoTransfer()
    {
        if (GetComponentInParent<LODGroup>() == null)
            return;
        var lods = GetComponentInParent<LODGroup>().GetLODs();
        int currentRendererLodIndex = -1;

        if(currentRenderer == null)
        {
            return;
        }

        for (int i = 0; i < lods.Length; i++)
        {
            for (int j = 0; j < lods[i].renderers.Length; j++)
            {
                if (currentRenderer == lods[i].renderers[j])
                    currentRendererLodIndex = i;
            }
        }
        if (currentRendererLodIndex == -1)
        {
            Debug.Log("Lightmapped LOD : lod index not found on " + gameObject.name);
            return;
        }

        var renderers = lods[currentRendererLodIndex].renderers;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                try
                {
                    renderers[i].lightProbeUsage = lods[0].renderers[i].lightProbeUsage;
                    renderers[i].lightmapIndex = lods[0].renderers[i].lightmapIndex;
                    renderers[i].lightmapScaleOffset = lods[0].renderers[i].lightmapScaleOffset;

                }
                catch
                {
                    if(Debug.isDebugBuild)
                        Debug.Log("Lightmapped LOD : Error setting lightmap settings on " + gameObject.name);
                }
            }
        }
    }
}
