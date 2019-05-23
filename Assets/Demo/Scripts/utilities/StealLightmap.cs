using UnityEngine;

[ExecuteInEditMode]
public class StealLightmap : MonoBehaviour {
    //public int LODIndex = 1;
    private MeshRenderer currentRenderer;
    public MeshRenderer lightmappedObject;
    // Use this for initialization
    private void OnEnable()
    {
        Awake();
    }

    private void Awake()
    {
        currentRenderer = gameObject.GetComponent<MeshRenderer>();
        RendererInfoTransfer();
    }

#if UNITY_EDITOR
    void OnBecameVisible()
    {
        RendererInfoTransfer();
    }
#endif

    void RendererInfoTransfer()
    {
        if(lightmappedObject == null || currentRenderer == null)
            return;
        if(!currentRenderer.isPartOfStaticBatch)
        {
            currentRenderer.lightmapIndex = lightmappedObject.lightmapIndex;
            currentRenderer.lightmapScaleOffset = lightmappedObject.lightmapScaleOffset;
            currentRenderer.lightProbeUsage = lightmappedObject.lightProbeUsage;
        }
    }
}